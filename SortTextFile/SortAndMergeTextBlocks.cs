using SortTextFile;
using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;

internal sealed class SortAndMergeTextBlocks
{
    const int blockSize = 1000; // Количество строк в блоке

    private readonly FolderHelper _folderHelper;
    private readonly IFileSplitterLexicon _splitter;
    internal SortAndMergeTextBlocks(IFileSplitterLexicon splitter, FolderHelper folderHelper)
    {
        _splitter = splitter;
        _folderHelper = folderHelper;
    }

    internal void Process()
    {
        var files = _splitter.GetIndexs;

        foreach (var file in files)
        {
            GetBlocksAndSort(file);
        }
    }

    /// <summary>
    /// source file
    /// </summary>
    /// <param name="fileName"></param>
    private void GetBlocksAndSort(string fileName)
    {
        int blockIndex = 0;

        using (var mmf = MemoryMappedFile.CreateFromFile(_folderHelper.GetBookIndexFile(fileName), FileMode.Open, "MMF"))
        using (var stream = mmf.CreateViewStream())
        using (var reader = new StreamReader(stream))
        //using (var fs = new FileStream(_folderHelper.GetBookIndexFile(Utils.FixFileName(fileName)), FileMode.Open, FileAccess.Read))
        //using (var reader = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 1 МБ
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

        MergeSortedFiles(fileName, blockIndex);
    }

    void SortAndSaveBlock(List<string> block, int blockIndex, string fileName)
    {
        block.Sort((x, y) => CompareFunc(x, y));

        File.WriteAllLines(_folderHelper.GetChunkFullNameFile(fileName, blockIndex), block);
    }
    internal static int CompareFunc(string x, string y)
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
    private static long GetNumber(ReadOnlySpan<char> xSpan, int xDotIndex) =>
        long.Parse(xSpan[..xDotIndex]);

    private static void GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();
        xDotIndex = xSpan.IndexOf('.');
        xTextPart = xSpan[(xDotIndex + 1)..];
    }

    void MergeSortedFiles(string fileName, int blockIndex)
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
                    queue[line].Enqueue(i);  // строка в какой очереди находится
                }
            }

            while (queue.Count > 0)
            {
                var kvp = queue.First();
                string line = kvp.Key;                  //line
                Queue<int> fileIndices = kvp.Value;  //  queue

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
}

