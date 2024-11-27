using SortTextFile.Interfaces;

namespace SortTextFile;

internal sealed class WriteToFile : IWriteToFile
{
    private readonly string _fileName;
    private readonly FileStream _fs;
    private readonly StreamWriter _sw;
    internal WriteToFile(string fileName)
    {
        _fileName = fileName;

        _fs = new FileStream(_fileName, FileMode.Create, FileAccess.Write);
        _sw = new StreamWriter(_fs);
    }

    public void Dispose()
    {
        _sw?.Dispose();
        _fs?.Dispose();
    }

    void IWriteToFile.WriteToFile(string text)
    {
        _sw.WriteLine(text);
    }
}
