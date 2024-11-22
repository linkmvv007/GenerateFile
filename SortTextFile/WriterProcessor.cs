using SortTextFile.Interfaces;

namespace SortTextFile;

/// <summary>
/// writes a sorted file
/// </summary>
internal sealed class WriterProcessor : IOutputWriter
{
    private readonly IFileSorting _file;
    internal WriterProcessor(IFileSorting file)
    {
        _file = file;
    }

    void IOutputWriter.SortingAndWriteToOutput(IWriteToFile writer)
    {
        foreach (var pos in _file.SortedPositions)
        {
            writer.WriteToFile(_file.ReadLine(pos));
        }
    }
}