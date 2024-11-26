using SortTextFile.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace SortTextFile;


internal sealed class SortAndMergeFiles : ISortAndMergeFiles
{
    private readonly ReadOnlyDictionary<long, long> _infoFiles;
    //private readonly int _maxLinesCount;
    //private readonly int _maxThreadsCount;
    private readonly string _sourceFileName;
    private readonly SplitterOptions _options;

    internal SortAndMergeFiles(IFileSplitter splitter, SplitterOptions options)
    {
        _infoFiles = splitter.InfoFiles;
        //_maxLinesCount = splitter.MaxLinesCount;
        _sourceFileName = splitter.SourceFileName;

        //_maxThreadsCount = maxThreadsCount;
        _options = options;
    }

    void ISortAndMergeFiles.Sort()
    {

        var sortedChunkFiles = ParallelSort();


        MergeSortedChunks(sortedChunkFiles, $"{_sourceFileName}_sorted");

    }
    private static string GetChunkNameFile(long fileIndex) =>
       $"chunk_{fileIndex}";

    private string GetChunkFullNameFile(long fileIndex) =>
  Path.Combine(_options.TempDirectory, $"{GetChunkNameFile(fileIndex)}.ind");

    private string GetSortedChunkFullNameFile(long fileIndex) =>
       $"{GetChunkFullNameFile(fileIndex)}.sorted";


    private ReadOnlyCollection<string> ParallelSort()
    {
        var сoncurrentBag = new ConcurrentBag<string>();

        ParallelOptions parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxNumberThreads
        };

        var results = Parallel.ForEach(_infoFiles.Keys, parallelOptions, fileIndex =>
        {
            var sortedFullNameFile = GetSortedChunkFullNameFile(fileIndex);

            using (IFileSorting file = new TextFileLinePositions(GetChunkFullNameFile(fileIndex), (int)_infoFiles[fileIndex]))  // src file
            using (var writer = new WriteToFile($"{sortedFullNameFile}"))              // output file
            {
                IOutputWriter processor = new WriterProcessor(file);
                processor.SortingAndWriteToOutput(writer);
            }

            сoncurrentBag.Add(sortedFullNameFile);
        });

        //removes chunk-files:
        foreach (var fileIndex in _infoFiles.Keys)
        {
            Utils.DeleteFile(GetChunkFullNameFile(fileIndex));
        }

        return сoncurrentBag.ToList().AsReadOnly();
    }

    record MergeInfo(string line, int nReader);

    static void MergeSortedChunksDraft(IReadOnlyList<string> sortedChunks, string outputFile)
    {
        Console.WriteLine($"{sortedChunks.Count} chuks files  merging to {outputFile} ...");

        var progress = 0L;

        var readers = new List<StreamReader>();
        foreach (string chunk in sortedChunks)
        {
            readers.Add(new StreamReader(chunk));
        }

        try
        {
            using (var sw = new StreamWriter(outputFile))
            {
                List<MergeInfo> buffer = new();
                for (int i = 0; i < readers.Count; i++)
                {
                    if (!readers[i].EndOfStream)
                    {
                        buffer.Add(new MergeInfo(readers[i].ReadLine(), i));
                    }
                }

                var dublicates = new List<int>();

                while (buffer.Count > 0)
                {
                    progress++;
                    if (progress > 10000)
                    {
                        Console.WriteLine($"{progress} lines are merged ...");
                        progress = 0;
                    }

                    buffer.Sort((x, y) => SortMethod(x, y));
                    dublicates.Clear();

                    var kvp = buffer[0];

                    //todo: посмотреть нет ли в буфере дубликатов минимальной строки
                    for (int i = 1; i < buffer.Count; i++)
                    {
                        if (kvp.line == buffer[i].line)
                            dublicates.Add(i);
                        else
                            break;
                    }

                    for (int i = 0; i <= dublicates.Count; i++)
                    {
                        sw.WriteLine(kvp.line); // вывести в файл строку
                        buffer.Remove(buffer[i]); // удалить из буфера
                    }

                    for (var i = 0; i <= dublicates.Count; i++)
                    {
                        string? line = readers[buffer[i].nReader].ReadLine();
                        if (line != null) // пока реадеру есть что читать
                        {
                            while (line == kvp.line) // проверить еще раз дубликат при очередном пополнении буфера
                            {
                                sw.WriteLine(kvp.line);
                                line = readers[buffer[i].nReader].ReadLine();
                            }

                            buffer.Add(new MergeInfo(line, buffer[i].nReader));
                        }
                    }
                }
            }
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader.Dispose();
            }

            // removed sorted chunks files:
            Utils.DeleteFiles(sortedChunks);
        }

        Console.WriteLine($"{sortedChunks.Count} chuks files  merging ...Ok");
    }

    private static int SortMethod(MergeInfo x, MergeInfo y) =>
        TextFileLinePositions.CompareFunc(x.line, y.line);

    static void MergeSortedChunks(IReadOnlyList<string> sortedChunks, string outputFile)
    {
        Console.WriteLine($"{sortedChunks.Count} chuks files  merging to {outputFile} ...");

        var progress = 0L;

        var readers = new List<StreamReader>();
        foreach (string chunk in sortedChunks)
        {
            readers.Add(new StreamReader(chunk));
        }

        try
        {
            using (var sw = new StreamWriter(outputFile))
            {
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
                    progress++;
                    if (progress > 10000)
                    {
                        Console.WriteLine($"{progress} lines are merged ...");
                        progress = 0;
                    }

                    var kvp = queue.First();
                    string line = kvp.Key;                  //строка
                    Queue<int> fileIndices = kvp.Value;  //  очередь

                    // Записываем строку столько раз, сколько раз она встречается
                    while (fileIndices.Count > 0)
                    {
                        sw.WriteLine(line);
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
        }
        finally
        {
            foreach (var reader in readers)
            {
                reader?.Dispose();
            }

            // removed sorted chunks files:
            Utils.DeleteFiles(sortedChunks);
        }

        Console.WriteLine($"{sortedChunks.Count} chuks files  merging ...Ok");
    }



    //internal static void MergeFiles(IReadOnlyList<string> files, string outputFile)
    //{
    //    using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
    //    using (var writer = new StreamWriter(new BufferedStream(fs)))
    //    {
    //        foreach (string file in files)
    //        {
    //            using (var fsr = new FileStream(file, FileMode.Open, FileAccess.Read))
    //            using (var reader = new StreamReader(fsr))
    //            {
    //                string line;
    //                while ((line = reader.ReadLine()) != null)
    //                {
    //                    writer.WriteLine(line);
    //                }
    //            }
    //        }
    //    }
    //}
}

internal class CustomComparer : IComparer<string>
{
    public int Compare(string? x, string? y) => TextFileLinePositions.CompareFunc(x, y);

}
