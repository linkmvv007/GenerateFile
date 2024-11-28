using SortTextFile.Interfaces;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace SortTextFile;

/// <summary>
/// Generate file index
/// </summary>
internal sealed class FileSplitterLexicon : IFileSplitterLexicon
{
    private static int _maxBookIndexLength = 4;

    const long BufferSize = 10_000_000L; // 10млн строк

    private readonly string _fileName;
    private readonly Dictionary<string, List<string>> _indexFileNames = new(capacity: 26 * 33 + 2); // letters of the Russian and English alphabets
    private readonly IFoldersHelper _folderHelper;
    private long _errors = 0L;


    internal FileSplitterLexicon(string fileName, IFoldersHelper folderHelper, int bookIndexLength)
    {
        _fileName = fileName;
        _folderHelper = folderHelper;

        _maxBookIndexLength = bookIndexLength < 1 ? 1 : bookIndexLength;
    }

    HashSet<string> IFileSplitterLexicon.GetIndexs => _indexFileNames.Keys.ToHashSet();
    long IFileSplitterLexicon.ErrorsCount => _errors;

    void IFileSplitterLexicon.SplitWithInfo()
    {
        Console.WriteLine("Validation and index creation ...");

        var progress = 0L;

        var counter = 0L;
        bool errors;

        using (IWriteToFile badLinesFile = new WriteToFile(_folderHelper.GetBadFormatLinesNameFile(_fileName)))
        using (var mmf = MemoryMappedFile.CreateFromFile(_fileName, FileMode.Open, "MMF"))  // Creating a memory-mapped file
        using (var stream = mmf.CreateViewStream(0, 0, MemoryMappedFileAccess.Read))        // Creating a view for reading a file
        using (var sr = new StreamReader(stream, Encoding.UTF8))
        //using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
        //using (var sr = new StreamReader(fs, Encoding.UTF8, true, 100 * 1024 * 1024)) // Reading in blocks of 100 MB (44sec - 2Gb)
        {
            string? line;
            string name;

            while ((line = sr.ReadLine()) != null && line[0] != '\0')
            //while ((line = sr.ReadLine()) != null)
            {
                name = GetNameIndexFile(line, out errors);
                if (errors)
                {
                    _errors++;
                    badLinesFile.WriteToFile(line);
                    continue;
                }

                if (!_indexFileNames.ContainsKey(name))
                {
                    _indexFileNames.Add(name, new List<string>(capacity: 39 * 1024) { line }); //todo
                }
                else
                {
                    _indexFileNames[name].Add(line);
                }

                counter++;
                if (counter > BufferSize)
                {
                    AppendBufferTextToFile();
                    counter = 0;

                    Console.Write($"\r{(++progress * BufferSize).ToString("N0")} lines ");
                }
            }
        }

        if (counter > 0L)
        {
            AppendBufferTextToFile();
        }

        Console.WriteLine("Validation and index creation... Ok");
    }


    private static string GetNameIndexFile(string line, out bool errors)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;

        errors = GetStringKey(line, out xSpan, out xDotIndex, out xTextPart);
        if (!errors)
        {
            int lengthToCheck = Math.Min(_maxBookIndexLength, xTextPart.Length);
            for (int i = 0; i < lengthToCheck; i++)
            {
                if (!Char.IsLetterOrDigit(xTextPart[i]))
                {
                    return ReplaceCharacters(xTextPart[..(lengthToCheck)].ToArray(), lengthToCheck);
                }
            }

            return xTextPart[..lengthToCheck].ToString();
        }

        return string.Empty;
    }

    public static string ReplaceCharacters(char[] span, int length)
    {
        for (int i = 0; i < length; i++)
        {
            span[i] = GetLetter(span[i]);
        }

        return new string(span, 0, length);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="x"></param>
    /// <param name="xSpan"></param>
    /// <param name="xDotIndex"></param>
    /// <param name="xTextPart"></param>
    /// <returns>true - error, bad line</returns>
    private static bool GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();

        xDotIndex = xSpan.IndexOf('.');

        if (xDotIndex == -1 || xSpan.Length <= (xDotIndex + 1))
        {
            xTextPart = null;
            return true;        // error
        }

        xTextPart = xSpan[(xDotIndex + 1)..];

        // check long;
        long xNumber;
        return !long.TryParse(xSpan[..xDotIndex], out xNumber);
    }
    private static char GetLetter(char ch)
    {
        return ch switch
        {
            char c when Char.IsLetterOrDigit(c) => ch,
            char c when c < '0' => '!',
            _ => '~'
        };
    }

    private void AppendBufferTextToFile()
    {
        foreach (var item in _indexFileNames)
        {
            if (item.Value?.Count > 0)
            {
                var filePath = Path.Combine(_folderHelper.BookIndexFolder, item.Key);
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