using SortTextFile.Interfaces;
using System.Text;

namespace SortTextFile;
//internal class Info
//{
//    internal Info(long p, long c)
//    {
//        pos = new() { p };
//        count = c;
//    }

//    /// <summary>
//    /// Line positions in file
//    /// </summary>
//    internal List<long> pos { get; set; }
//    internal long count { get; set; }
//};

/// <summary>
/// remembers the beginning of line positions in the file
/// </summary>
internal sealed class TextFileLinePositions : IFileSorting
{
    private readonly FileStream _fs;
    private readonly StreamReader _sr;
    private List<long> _sortedPositions;
    //private Dictionary<string, Info> _dict;
    private bool _isSorted = false;
    private readonly int _capacity;

    private readonly string _fileName;

    internal TextFileLinePositions(string fileName, int capacity = 100000000)
    {
        _fileName = fileName;
        _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        _sr = new StreamReader(_fs);

        _capacity = capacity;
    }

    #region IFileSorting
    IReadOnlyList<long> IFileSorting.SortedPositions
    {
        get
        {
            if (!_isSorted)
            {
                Sort();
            }

            if (_sortedPositions is null)
            {
                throw new ArgumentNullException("File is bad format. Not found string positions");
            }

            return _sortedPositions.AsReadOnly();
        }
    }

    string IFileSorting.ReadLine(long position) =>
    ReadLineAtPosition(position);

    public void Dispose()
    {
        _sr?.Dispose();
        _fs?.Dispose();
    }

    #endregion //IFileSorting

    /// <summary>
    /// Sorting indexes based on row contents
    /// </summary>
    private void Sort()
    {
        if (_sortedPositions is null)
            _sortedPositions = GetLinePositions();

        Console.WriteLine($"Sorting {_fileName} file ....");

        _sortedPositions.Sort((index1, index2) => SortMethod(index1, index2));

        _isSorted = true;

        Console.WriteLine($"Sorting {_fileName} file .... OK");
    }

    //static long _progress = 0;
    private int SortMethod(long index1, long index2)
    {
        //_progress++;

        string x = ReadLineAtPosition(index1);
        string y = ReadLineAtPosition(index2);

        //if (_progress++ > 1000000L)
        //{
        //    Console.WriteLine($"{index1}: {x} \n {index2}: {y}");
        //    _progress = 0L;
        //}

        return CompareFunc(x, y);
    }

    internal static int CompareFunc(string? x, string? y)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(x, out xSpan, out xDotIndex, out xTextPart);

        ReadOnlySpan<char> ySpan, yTextPart;
        int yDotIndex;
        GetStringKey(y, out ySpan, out yDotIndex, out yTextPart);

        int textComparison = xTextPart.CompareTo(yTextPart, StringComparison.Ordinal);
        if (textComparison != 0)
        {
            return textComparison;
        }

        long xNumber = GetNumber(xSpan, xDotIndex);
        long yNumber = GetNumber(ySpan, yDotIndex);

        return (xNumber == yNumber) ? 0 : (xNumber > yNumber ? 1 : -1);
    }

    private static long GetNumber(ReadOnlySpan<char> xSpan, int xDotIndex) =>
     long.Parse(xSpan.Slice(0, xDotIndex));


    private static void GetStringKey(string x, out ReadOnlySpan<char> xSpan, out int xDotIndex, out ReadOnlySpan<char> xTextPart)
    {
        xSpan = x.AsSpan();
        xDotIndex = xSpan.IndexOf('.');
        xTextPart = xSpan.Slice(xDotIndex + 1);
    }

    private void GoToPosition(long position)
    {
        _fs.Seek(position, SeekOrigin.Begin);

        _sr.DiscardBufferedData();
    }
    private string ReadLineAtPosition(long position)
    {
        GoToPosition(position);

        return _sr.ReadLine();
    }

    private List<long> GetLinePositions()
    {
        Console.WriteLine($"Reading file {_fileName} ...");

        var positions = new List<long>(capacity: _capacity);
        using (var fs = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
        using (var sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 10 МБ
        {
            long position = 0;
            string? line;
            while ((line = sr.ReadLine()) != null)
            {
                positions.Add(position);
                position += line.Length + Environment.NewLine.Length;
            }
        }

        Console.WriteLine($"Reading file {_fileName} ... Ok");
        return positions;
    }
    /*
    void AnalyzeLine(long position, string line)
    {
        ReadOnlySpan<char> xSpan, xTextPart;
        int xDotIndex;
        GetStringKey(line, out xSpan, out xDotIndex, out xTextPart);

        var ch = xTextPart.Slice(0, 1);

        string key = ch[0] switch
        {
            char c when Char.IsLetterOrDigit(c) => ch.ToString(),
            char c when c > 'A' => "!",
            _ => "~"
        };

        if (_dict.ContainsKey(key) == false)
            _dict.Add(key, new Info(position, 0L));
        else
        {
            var info = _dict[key];
            info.count++;
            info.pos.Add(position);
        }
    }*/
    /* void IFileSorting.Analysis()
     {
         _dict = new();

         if (_sortedPositions is null)
             _sortedPositions = GetLinePositions(true);

         // 
         Console.WriteLine("Сreating files...");
         foreach (var item in _dict)
         {
             Console.WriteLine($" запись файла '{item.Key}' ...");
             using (IWriteToFile file = new WriteToFile(item.Key))
             {
                 foreach (var pos in item.Value.pos)
                     file.WriteToFile(ReadLineAtPosition(pos));
             }
         }

         Console.WriteLine("Сreating files... Ok");
     }*/

    //public List<long> GetLinePositions()
    //{
    //    var positions = new List<long>(capacity: 1000000);

    //    long position = 0;
    //    int byteRead;
    //    bool isNewLine = true;

    //    while ((byteRead = _fs.ReadByte()) != -1)
    //    {
    //        if (isNewLine)
    //        {
    //            positions.Add(position);
    //            isNewLine = false;
    //        }

    //        if (byteRead == '\n')
    //        {
    //            isNewLine = true;
    //        }

    //        position++;
    //    }

    //    return positions;
    //}
}



