using SortTextFile.Interfaces;

namespace SortTextFile;

/// <summary>
/// writes a sorted file
/// </summary>
internal sealed class WriterProcessor : IFileWriter
{
    private readonly IFileSorting _file;
    internal WriterProcessor(IFileSorting file)
    {
        _file = file;
    }

    void IFileWriter.WriteToFile(IWriteToFile writer)
    {
        //using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
        //using (var sr = new StreamWriter(new BufferedStream(fs)))
        //{

        foreach (var pos in _file.SortedPositions)
        {
            //sr.WriteLine(_file.ReadLine(pos));
            writer.WriteToFile(_file.ReadLine(pos));
        }
        //}
        writer.Dispose();
    }
}