using SortTextFile;
using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;

internal sealed class ParrallelSort : ISortAndMergeTextBlocks
{
    private readonly IFoldersHelper _folderHelper;
    private readonly HashSet<string> _indexesHash;
    private readonly int _threadsCount;
    internal ParrallelSort(HashSet<string> indexes, IFoldersHelper folderHelper, int threadsCount = 4)
    {
        _indexesHash = indexes;
        _folderHelper = folderHelper;
        _threadsCount = threadsCount;
    }
    void ISortAndMergeTextBlocks.Process()
    {

        var count = _indexesHash.Count;
        var it = 0L;
        Console.WriteLine("Merging ....");



        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _threadsCount
        };


        var results = Parallel.ForEach(_indexesHash, parallelOptions, fileIndex =>
        {
            // Console.Write($"\r{it * 100 / count}%");
            Console.WriteLine($"'{fileIndex}' Merging ....");
            var sort = new SortAndMergeTextBlocks(_folderHelper);
            sort.GetBlocksAndSort(fileIndex);
            it++;

            Console.WriteLine();
            Console.WriteLine($"'{fileIndex}' Merging ....Ok");

        });

    }


}
internal sealed class SortAndMergeTextBlocks //: ISortAndMergeTextBlocks
{
    const int blockSize = 3072; // The number of lines in the block

    private readonly IFoldersHelper _folderHelper;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="folderHelper"></param>
    internal SortAndMergeTextBlocks(IFoldersHelper folderHelper)
    {
        _folderHelper = folderHelper;
    }

    /// <summary>
    /// source file
    /// </summary>
    /// <param name="fileName"></param>
    internal void GetBlocksAndSort(string fileName)
    {
        int blockIndex = 0;

        var bookIndexFile = _folderHelper.GetBookIndexFile(fileName);
        using (var mmf = MemoryMappedFile.CreateFromFile(bookIndexFile, FileMode.Open, $"MMF_BookIndex_{fileName}"))
        using (var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))
        using (var reader = new StreamReader(stream))
        //using (var fs = new FileStream(bookIndexFile, FileMode.Open, FileAccess.Read))
        //using (var reader = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 10 МБ
        {
            var lineList = new List<string>(capacity: blockSize);
            string line;

            while ((line = reader.ReadLine()) != null && line[0] != '\0')
            //while ((line = reader.ReadLine()) != null)
            {
                lineList.Add(line);

                if (lineList.Count >= blockSize)
                {
                    SortAndSaveBlock(lineList, blockIndex++, fileName);
                    lineList.Clear();
                }
            }

            if (lineList.Count > 0)
            {
                SortAndSaveBlock(lineList, blockIndex, fileName);
            }
        }

        //todo : remove book index file:
        Utils.DeleteFile(bookIndexFile);

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
        for (int i = 0; i <= blockIndex; i++)
        {
            readers[i] = new StreamReader(_folderHelper.GetChunkFullNameFile(fileName, i));
        }

        try
        {
            using var output = new StreamWriter(_folderHelper.GetSortedChunkFullNameFile(fileName));

            var queue = new SortedDictionary<string, Queue<int>>(new CustomComparer());

            for (int i = 0; i < readers.Length; i++)
            {
                if (!readers[i].EndOfStream)
                {
                    string line = readers[i].ReadLine();
                    if (!queue.ContainsKey(line))
                    {
                        queue[line] = new Queue<int>();
                    }
                    queue[line].Enqueue(i);  // the line in which queue is located
                }
            }

            while (queue.Count > 0)
            {
                var kvp = queue.First();
                string line = kvp.Key;               // line
                Queue<int> fileIndices = kvp.Value;  // queue

                // We write the lines as many times as it occurs
                while (fileIndices.Count > 0)
                {
                    output.WriteLine(line);
                    int streamIndex = fileIndices.Dequeue();

                    if (!readers[streamIndex].EndOfStream)
                    {
                        string? newLine = readers[streamIndex].ReadLine(); // reading the line

                        if (newLine == line)
                        {
                            fileIndices.Enqueue(streamIndex); // If it matches, we add it back to the queue
                        }
                        else
                        {
                            if (!queue.ContainsKey(newLine))
                            {
                                queue[newLine] = new Queue<int>();
                            }

                            queue[newLine].Enqueue(streamIndex);
                        }
                    }
                }

                queue.Remove(line);
            }
        }
        finally
        {
            for (int i = 0; i <= blockIndex; i++)
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
        public int Compare(string? x, string? y) => CompareFunc(x, y);

    }
}

