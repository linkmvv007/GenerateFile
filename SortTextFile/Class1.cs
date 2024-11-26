using SortTextFile;
using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;

internal sealed class SortAndMergeTextBlocks
{
    const int blockSize = 10000; // Количество строк в блоке
                                 // private readonly string _destFile;
    private readonly string _chunksFolder;
    private readonly string _resultFolder;
    private readonly SplitterOptions _options;
    private readonly IFileSplitterLexicon _splitter;
    internal SortAndMergeTextBlocks(IFileSplitterLexicon splitter, SplitterOptions options)
    {
        _splitter = splitter;
        // _destFile = destFile; //sortedfile.txt"
        _options = options;
        _chunksFolder = Path.Combine(_options.TempDirectory, "chunks");

        _resultFolder = Path.Combine(_chunksFolder, "Results");
        Directory.CreateDirectory(_resultFolder);
    }

    internal void Process()
    {
        //var files = _splitter.GetIndexs;
        var files = new HashSet<string>() { "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\SortTextFile\\bin\\Release\\net8.0\\Temp\\BookIndex\\aban" };

        var indexPath = _splitter.IndexFolder;
        foreach (var file in files)
        {
            GetBlocksAndSort(file);
        }
    }

    /// <summary>
    /// source file
    /// </summary>
    /// <param name="fileName"></param>
    private void GetBlocksAndSort(string srcFullName)
    {
        //var tasks = new List<Task>();
        int blockIndex = 0;

        using (var mmf = MemoryMappedFile.CreateFromFile(srcFullName, FileMode.Open, "MMF"))
        using (var stream = mmf.CreateViewStream())
        using (var reader = new StreamReader(stream))
        {
            var lineList = new List<string>();
            string line;

            while ((line = reader.ReadLine()) != null && line[0] != '\0')
            {
                lineList.Add(line);

                if (lineList.Count >= blockSize)
                {
                    SortAndSaveBlock(lineList, _chunksFolder, blockIndex++);
                    //tasks.Add(Task.Run(() => SortAndSaveBlock(new List<string>(lineList), _tempFolder, blockIndex++)));
                    lineList.Clear();
                }
            }

            if (lineList.Count > 0)
            {
                SortAndSaveBlock(lineList, _chunksFolder, blockIndex);
                //tasks.Add(Task.Run(() => SortAndSaveBlock(lineList, _tempFolder, blockIndex)));
            }


        }

        //Task.WaitAll(tasks.ToArray());

        MergeSortedFiles(Path.Combine(_resultFolder, Path.GetFileName(srcFullName)), blockIndex); //todo
    }

    static void SortAndSaveBlock(List<string> block, string tempDirectory, int blockIndex)
    {
        block.Sort((x, y) => TextFileLinePositions.CompareFunc(x, y));

        File.WriteAllLines(Path.Combine(tempDirectory, $"chunk{blockIndex}.ind"), block);
    }

    void MergeSortedFiles(string outputPath, int blockIndex)
    {
        var readers = Enumerable.Range(0, blockIndex + 1)
            .Select(f => new StreamReader(Path.Combine(_chunksFolder, $"chunk{f}.ind")))
            .ToList();

        try
        {
            using var output = new StreamWriter(outputPath);

            var queue = new SortedDictionary<string, Queue<int>>(new CustomComparer());

            for (int i = 0; i < readers.Count; i++)
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
                string line = kvp.Key;                  //строка
                Queue<int> fileIndices = kvp.Value;  //  очередь

                // Записываем строку столько раз, сколько раз она встречается
                while (fileIndices.Count > 0)
                {
                    output.WriteLine(line);
                    int streamIndex = fileIndices.Dequeue();

                    if (!readers[streamIndex].EndOfStream)
                    {
                        string? newLine = readers[streamIndex].ReadLine(); // читаем

                        if (newLine == line)
                        {
                            fileIndices.Enqueue(streamIndex); // если совпадает - добавляем опять в очередь
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
            readers.ForEach(x => x?.Dispose());
        }
    }
}

