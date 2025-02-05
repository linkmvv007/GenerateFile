﻿using SortTextFile;
using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;

internal sealed class SortAndMergeTextBlocks : ISortAndMergeTextBlocks
{
    const int blockSize = 10240; // The number of lines in the block
    //const int blockSize = 50; // The number of lines in the block
    //const int blockSize = 1; // The number of lines in the block

    private readonly IFoldersHelper _folderHelper;
    private readonly bool _isDeleteFiles;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderHelper"></param>
    internal SortAndMergeTextBlocks(IFoldersHelper folderHelper, bool isDeleteFiles = true)
    {
        _folderHelper = folderHelper;
        _isDeleteFiles = isDeleteFiles;
    }

    /// <summary>
    /// source file
    /// </summary>
    /// <param name="fileName"></param>
    void ISortAndMergeTextBlocks.GetBlocksAndSort(string fileName)
    {
        int blockIndex = -1;

        var lineList = new List<string>(capacity: blockSize);
        string line;


        var bookIndexFile = _folderHelper.GetBookIndexFile(fileName);
        using (var mmf = MemoryMappedFile.CreateFromFile(bookIndexFile, FileMode.Open, $"MMF_BookIndex_{DateTime.Now:HHmmss}_{fileName}"))
        using (var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
        using (var reader = new StreamReader(stream))
        //using (var fs = new FileStream(bookIndexFile, FileMode.Open, FileAccess.Read))
        //using (var reader = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 10 МБ
        {
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Length > 0 && line[0] == '\0')
                    continue;

                Utils.TrimEndNulls(ref line);

                lineList.Add(line);

                if (lineList.Count >= blockSize)
                {
                    SortAndSaveBlock(lineList, ++blockIndex, fileName);
                    lineList.Clear();
                }
            }
        }

        if (lineList.Count > 0)
        {
            SortAndSaveBlock(lineList, ++blockIndex, fileName);
        }

        // remove book index file:
        if (_isDeleteFiles)
        {
            Utils.DeleteFile(bookIndexFile);
        }

        MergeSortedFiles(fileName, blockIndex);
    }

    private void SortAndSaveBlock(List<string> block, int blockIndex, string fileName)
    {
        block.Sort((x, y) => CompareFunc(x, y));

        File.WriteAllLines(_folderHelper.GetChunkFullNameFile(fileName, blockIndex), block);
    }

    private static long GetNumber(ReadOnlySpan<char> xSpan, int xDotIndex) =>
        long.Parse(xSpan[..xDotIndex]);

    private static void GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();
        xDotIndex = xSpan.IndexOf('.');
        xTextPart = xSpan[(xDotIndex + 1)..];
    }

    private void MergeSortedFiles(string fileName, int blockIndex)
    {
        if (blockIndex < 1)
        {
            // already sorted => move to the results folder
            File.Move(
                _folderHelper.GetChunkFullNameFile(fileName, blockIndex),
                _folderHelper.GetSortedChunkFullNameFile(fileName)
                );

            return;
        }


        var readers = new StreamReader[blockIndex + 1];
        for (int i = 0; i < readers.Length; i++)
        {
            readers[i] = new StreamReader(_folderHelper.GetChunkFullNameFile(fileName, i));
        }

        try
        {
            using var output = new StreamWriter(_folderHelper.GetSortedChunkFullNameFile(fileName));

            var sortedLinesDict = new SortedDictionary<string, Queue<int>>(new CustomComparer());

            for (int i = 0; i < readers.Length; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    string line = readers[i].ReadLine();
                    if (!sortedLinesDict.ContainsKey(line))
                    {
                        sortedLinesDict[line] = new Queue<int>();
                    }
                    sortedLinesDict[line].Enqueue(i);  // the line in which queue is located
                }
            }

            while (sortedLinesDict.Count > 0)
            {
                var kvp = sortedLinesDict.First();
                string line = kvp.Key;               // line
                Queue<int> fileIndices = kvp.Value;  // queue

                // We write the lines as many times as it occurs
                while (fileIndices.Count > 0)
                {
                    output.WriteLine(line);
                    int streamIndex = fileIndices.Dequeue();

                    if (!readers[streamIndex].EndOfStream)
                    {
                        string newLine = readers[streamIndex].ReadLine(); // reading the line

                        if (newLine == line)
                        {
                            fileIndices.Enqueue(streamIndex); // If it matches, we add it back to the queue
                        }
                        else
                        {
                            if (!sortedLinesDict.ContainsKey(newLine))
                            {
                                sortedLinesDict[newLine] = new Queue<int>();
                            }

                            sortedLinesDict[newLine].Enqueue(streamIndex);
                        }
                    }
                }

                sortedLinesDict.Remove(line);
            }
        }
        finally
        {
            for (int i = 0; i < readers.Length; i++)
            {
                readers[i]?.Dispose();
                Utils.DeleteFile(_folderHelper.GetChunkFullNameFile(fileName, i));
            }

        }
    }

    /// <summary>
    /// Sorting function
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int CompareFunc(string x, string y)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(x, out xSpan, out xDotIndex, out xTextPart);

        ReadOnlySpan<char> ySpan, yTextPart;
        int yDotIndex;
        GetStringKey(y, out ySpan, out yDotIndex, out yTextPart);

        int textComparison = xTextPart.CompareTo(yTextPart, StringComparison.Ordinal);
        if (textComparison != 0)
        {
            return textComparison;
        }

        long xNumber = GetNumber(xSpan, xDotIndex);
        long yNumber = GetNumber(ySpan, yDotIndex);

        return (xNumber == yNumber) ? 0 : (xNumber > yNumber ? 1 : -1);
    }

    private class CustomComparer : IComparer<string>
    {
        public int Compare(string x, string y) => CompareFunc(x, y);
    }
}

