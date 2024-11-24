using SortTextFile.Interfaces;
using System.Text;

namespace SortTextFile;

/// <summary>
/// Generate book index
/// </summary>
internal sealed class FileSplitterLexicon : IFileSplitterLexicon
{
    private readonly string _fileName;

    private readonly HashSet<string> _indexFileNames = new(capacity: 26 * 33 + 2);
    private readonly SplitterOptions _options;
    private readonly string _tempFolder;

    internal FileSplitterLexicon(string fileName, SplitterOptions options)
    {
        _fileName = fileName;
        _options = options;

        _tempFolder = Path.Combine(_options.TempDirectory, "BookIndex");
        Directory.CreateDirectory(_tempFolder);
    }

    void IFileSplitterLexicon.SplitWithInfo()
    {
        Console.WriteLine("Сreating files...");

        using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
        using (var sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Reading in blocks of 10 MB
        {
            string? line;
            string name;

            while ((line = sr.ReadLine()) != null)
            {
                name = GetNameIndexFile(line);

                if (!_indexFileNames.Contains(name))
                {
                    _indexFileNames.Add(name);
                }

                AppendTextToFile(name, line);
            }
        }


        Console.WriteLine("Сreating files... Ok");
    }

    private static string GetNameIndexFile(string line)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(line, out xSpan, out xDotIndex, out xTextPart);

        var result = GetLetter(xTextPart.Slice(0, 1));

        return xTextPart.Length switch
        {
            >= 2 => result + GetLetter(xTextPart.Slice(1, 1)),
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

    private static void AppendTextToFile(string filePath, string text)
    {
        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(text);
        }
    }
    /*
        private void GetLinePositions(bool analys = false)
        {
            Console.WriteLine("Analysis...");

            using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 10 МБ
            {
                long position = 0;
                string line;
                while ((line = sr.ReadLine()) != null)
                {

                    //var name = AnalyzeLine(line);

                    position += line.Length + Environment.NewLine.Length;
                }
            }

            Console.WriteLine("Analysis... Ok");

        }


        static ReadOnlySpan<char> ExtractSubstringFromPosition(ReadOnlySpan<char> span, int position, int length)
        {
            if (position < 0 || position >= span.Length)
                throw new ArgumentOutOfRangeException(nameof(position));

            int end = position + length;
            if (end > span.Length)
                end = span.Length;

            return span.Slice(position, end - position);
        }*/
}