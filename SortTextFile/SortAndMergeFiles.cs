using SortTextFile.Interfaces;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;

namespace SortTextFile;

interface ISortAndMergeFiles
{
    void Sort();
}

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
        //var sortedNames = new List<string>();

        //// sorting:
        //foreach (var fileIndex in _infoFiles.Keys)
        //{
        //    var nameFile = $"chunk_{fileIndex}";
        //    var sortedNameFile = $"{nameFile}_sorted.ind";

        //    //using (IFileSorting file = new TextFileLinePositions($"{nameFile}.ind", _maxLinesCount))  // src file
        //    using (IFileSorting file = new TextFileLinePositions($"{nameFile}.ind", (int)_infoFiles[fileIndex]))  // src file
        //    using (var writer = new WriteToFile($"{sortedNameFile}"))              // output file
        //    {
        //        IOutputWriter processor = new WriterProcessor(file);
        //        processor.SortingAndWriteToOutput(writer);
        //    }
        //    sortedNames.Add(sortedNameFile);
        //}



        //merge files:
        // MergeSortedChunks(sortedNames.AsReadOnly(), $"{_sourceFileName}_sorted");

        // MergeFiles(sortedNames.AsReadOnly(), $"{_sourceFileName}_sorted");
        MergeSortedChunks(ParallelSort(), $"{_sourceFileName}_sorted");

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
            //var nameFile = $"chunk_{fileIndex}";
            //var sortedNameFile = $"{nameFile}_sorted.ind";
            var sortedFullNameFile = GetSortedChunkFullNameFile(fileIndex);

            //todo

            //using (IFileSorting file = new TextFileLinePositions($"{nameFile}.ind", (int)_infoFiles[fileIndex]))  // src file
            using (IFileSorting file = new TextFileLinePositions(GetChunkFullNameFile(fileIndex), (int)_infoFiles[fileIndex]))  // src file
            using (var writer = new WriteToFile($"{sortedFullNameFile}"))              // output file
            {
                IOutputWriter processor = new WriterProcessor(file);
                processor.SortingAndWriteToOutput(writer);
            }

            сoncurrentBag.Add(sortedFullNameFile);
        });

        //todo: removes files
        foreach (var fileIndex in _infoFiles.Keys)
        {
            DeleteFile(GetChunkFullNameFile(fileIndex));

        }
        return сoncurrentBag.ToList().AsReadOnly();
    }

    private static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    record MergeInfo(string line, int nReader);

    static void MergeSortedChunks(IReadOnlyList<string> sortedChunks, string outputFile)
    {
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

                while (buffer.Count > 0)
                {
                    buffer.Sort((x, y) => SortMethod(x, y));

                    var kvp = buffer[0];

                    sw.WriteLine(kvp.line); // вывести в файл строку

                    buffer.Remove(buffer[0]); // удалить из буфера

                    string line = readers[kvp.nReader].ReadLine();
                    if (line != null) // пока реадеру есть что читать
                    {
                        buffer.Add(new MergeInfo(line, kvp.nReader));
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

            // removed sorted chunks files
            foreach (var item in sortedChunks)
            {
                DeleteFile(item);
            }
        }
    }

    private static int SortMethod(MergeInfo x, MergeInfo y) =>
        TextFileLinePositions.CompareFunc(x.line, y.line);


    internal static void MergeFiles(IReadOnlyList<string> files, string outputFile)
    {
        using (var fs = new FileStream(outputFile, FileMode.Create, FileAccess.Write))
        using (var writer = new StreamWriter(new BufferedStream(fs)))
        {
            foreach (string file in files)
            {
                using (var fsr = new FileStream(file, FileMode.Open, FileAccess.Read))
                using (var reader = new StreamReader(fsr))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        writer.WriteLine(line);
                    }
                }
            }
        }
    }
}
