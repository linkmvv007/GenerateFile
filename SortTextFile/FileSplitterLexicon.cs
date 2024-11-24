using System.Text;

namespace SortTextFile;

interface IFileSplitterLexicon
{
    void SplitWithInfo();

}
internal sealed class FileSplitterLexicon : IFileSplitterLexicon
{
    // private readonly Dictionary<string, Info> _dict = new();
    private readonly string _fileName;

    void IFileSplitterLexicon.SplitWithInfo()
    {
        Console.WriteLine("Сreating files...");

        string? line;
        //long linesCount = 0L;

        // var wr = NextNameFile();

        string name;
        using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
        using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Reading in blocks of 10 MB
        {
            while ((line = sr.ReadLine()) != null)
            {
                name = GetNameFile(line);

                WriteToFile(name, line);
            }
        }


        Console.WriteLine("Сreating files... Ok");
    }

    private static string GetNameFile(string line)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(line, out xSpan, out xDotIndex, out xTextPart);

        var ch = xTextPart.Slice(0, 1);

        return ch[0] switch
        {
            char c when Char.IsLetterOrDigit(c) => ch.ToString(),
            char c when c > 'A' => "!",
            _ => "~"
        };
    }

    private static void GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();
        xDotIndex = xSpan.IndexOf('.');
        xTextPart = xSpan.Slice(xDotIndex + 1);
    }

    private static void WriteToFile(string filePath, string text)
    {
        // Добавление текста в конец файла
        using (StreamWriter sw = File.AppendText(filePath))
        {
            sw.WriteLine(text);
        }
    }

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
    }
}