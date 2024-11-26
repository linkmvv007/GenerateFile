using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SortTextFile;

/// <summary>
/// Generate book index
/// </summary>
internal sealed class FileSplitterLexicon : IFileSplitterLexicon
{
    private readonly string _fileName;
    private readonly Dictionary<string, List<string>> _indexFileNames = new(capacity: 26 * 33 + 2);
    private readonly SplitterOptions _options;
    private readonly string _indexFolder;

    internal FileSplitterLexicon(string fileName, SplitterOptions options)
    {
        _fileName = fileName;
        _options = options;

        _indexFolder = Path.Combine(_options.TempDirectory, "BookIndex");
        Directory.CreateDirectory(_indexFolder);

        //todo:  Utils.DeleteFile(_indexFolder);
    }

    string IFileSplitterLexicon.IndexFolder => _indexFolder;
    HashSet<string> IFileSplitterLexicon.GetIndexs => _indexFileNames.Keys.ToHashSet();

    void IFileSplitterLexicon.SplitWithInfo()
    {
        Console.WriteLine("Сreating index files...");
        var counter = 0L;
        //long maxCount = 0;

        using (var mmf = MemoryMappedFile.CreateFromFile(_fileName, FileMode.Open, "MMF")) // Создание отображенного в память файла
        using (var stream = mmf.CreateViewStream())// Создание представления для чтения файла
        using (var sr = new StreamReader(stream, Encoding.UTF8))
        //using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
        //using (var sr = new StreamReader(fs, Encoding.UTF8, true, 100 * 1024 * 1024)) // Reading in blocks of 100 MB (44sec - 2Gb)
        {
            string? line;
            string name;

            while ((line = sr.ReadLine()) != null && line[0] != '\0')
            //while ((line = sr.ReadLine()) != null)
            {
                name = GetNameIndexFile(line);

                if (!_indexFileNames.ContainsKey(name))
                {
                    _indexFileNames.Add(name, new List<string>(capacity: 39 * 1024) { line });
                }
                else
                {
                    var item = _indexFileNames[name];
                    item.Add(line);
                    //if (maxCount < item.Count)
                    //    maxCount = item.Count;
                }

                counter++;
                if (counter > 1000000)
                {
                    AppendBufferTextToFile();
                    //Console.WriteLine($"maxCount: {maxCount}");
                    //maxCount = 
                    counter = 0;
                }
            }
        }

        if (counter > 0L)
        {
            AppendBufferTextToFile();

            counter = 0;
        }

        Console.WriteLine("Сreating index files... Ok");
    }

    private static string GetNameIndexFile(string line)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(line, out xSpan, out xDotIndex, out xTextPart);

        var result = GetLetter(xTextPart.Slice(0, 1));

        return xTextPart.Length switch
        {
            > 3 => result + GetLetter(xTextPart.Slice(1, 3)),
            > 2 => result + GetLetter(xTextPart.Slice(1, 2)),
            > 1 => result + GetLetter(xTextPart.Slice(1, 1)),
            _ => result
        };
    }

    private static string GetLetter(ReadOnlySpan<char> ch)
    {
        return ch[0] switch
        {
            char c when Char.IsLetterOrDigit(c) => ch.ToString(),
            char c when c < 'A' => "!",
            _ => "~"
        };
    }


    private static void GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();
        xDotIndex = xSpan.IndexOf('.');

        if (xDotIndex == -1)
        {
            throw new ApplicationException("Bad file format: Dot not found");
        }

        if (xSpan.Length <= (xDotIndex + 1))
        {
            throw new ApplicationException("Bad file format. Name not found");
        }

        xTextPart = xSpan.Slice(xDotIndex + 1);
    }

    //private static void AppendTextToFile(string filePath, string text)
    //{
    //    using (StreamWriter sw = File.AppendText(filePath))
    //    {
    //        sw.WriteLine(text);
    //    }
    //}
    private void AppendBufferTextToFile()
    {
        foreach (var item in _indexFileNames)
        {
            if (item.Value?.Count > 0)
            {
                var filePath = Path.Combine(_indexFolder, item.Key);
                using (StreamWriter sw = File.AppendText(filePath))
                {
                    foreach (var line in item.Value)
                    {
                        sw.WriteLine(line);
                    }
                }

                item.Value.Clear();
            }
        }
    }
}