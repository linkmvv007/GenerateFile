using SortTextFile.Interfaces;
using System.Collections.ObjectModel;
using System.Text;

namespace SortTextFile;
interface IFileSplitter
{
    void Split();

    ReadOnlyDictionary<long, long> InfoFiles { get; }

    int MaxLinesCount { get; }

    string SourceFileName { get; }


}

internal sealed class FileSplitter : IFileSplitter
{
    private readonly Dictionary<long, long> _dictInfoFiles = new(); //todo;

    private readonly string _fileName;
    private string _fullFileName;
    private readonly SplitterOptions _options;
    private long _fileIndex = 0L;

    internal FileSplitter(string fileName, SplitterOptions options)
    {
        _fileName = fileName;
        _options = options;

        _fullFileName = Path.Combine(_options.TempDirectory, fileName);
    }

    ReadOnlyDictionary<long, long> IFileSplitter.InfoFiles => _dictInfoFiles.AsReadOnly();
    string IFileSplitter.SourceFileName => _fileName;

    int IFileSplitter.MaxLinesCount =>
        _dictInfoFiles is null ? 0 : (checked((int)_dictInfoFiles.Values.Max()));


    /// <summary>
    /// Splits a large file into many small ones
    /// </summary>
    void IFileSplitter.Split()
    {
        var maxSize = 1024 * 1024 * _options.FileSizeInMb;
        var newLineLength = Environment.NewLine.Length;

        Console.WriteLine($"Splitting {_fileName} file...");

        string? line;
        long linesCount = 0L;

        IWriteToFile? wr = NextNameFile(linesCount);
        long currentFileSize = 0;
        try
        {
            using (var fs = new FileStream(_fullFileName, FileMode.Open, FileAccess.Read))
            using (var sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Reading in blocks of 10 MB
            {
                while ((line = sr.ReadLine()) != null)
                {
                    wr.WriteToFile(line);

                    linesCount++;
                    currentFileSize += Encoding.UTF8.GetByteCount(line) + newLineLength;

                    if (currentFileSize >= maxSize)
                    {
                        wr = NextNameFile(linesCount, wr);

                        currentFileSize = linesCount = 0L;
                    }
                }
            }
        }
        finally
        {
            wr?.Dispose();

            if (linesCount > 0)
            {
                _dictInfoFiles.Add(_fileIndex, linesCount);
            }
        }

        Console.WriteLine("Splitting file... Ok");
    }

    private IWriteToFile NextNameFile(long linesCount, IWriteToFile? outputSplitFile = null)
    {
        outputSplitFile?.Dispose();

        if (linesCount > 0)
        {
            _dictInfoFiles.Add(_fileIndex, linesCount);
        }

        var chunkFileName = $"chunk_{++_fileIndex}.ind";


        return new WriteToFile(Path.Combine(_options.TempDirectory, chunkFileName));
    }

}
