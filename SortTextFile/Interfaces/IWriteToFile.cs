namespace SortTextFile.Interfaces;

internal interface IWriteToFile : IDisposable
{
    void WriteToFile(string text);
}
