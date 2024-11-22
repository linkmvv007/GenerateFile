using SortTextFile.Interfaces;

namespace SortTextFile;

/// <summary>
/// remembers the beginning of line positions in the file
/// </summary>
internal sealed class TextFileLinePositions : IFileSorting
{
    private readonly FileStream _fs;
    private readonly StreamReader _sr;
    private List<long> _sortedPositions;


    internal TextFileLinePositions(string fileName)
    {
        _fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
        _sr = new StreamReader(_fs);
    }

    #region Public
    IReadOnlyList<long> IFileSorting.SortedPositions
    {
        get
        {
            if (_sortedPositions is null)
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
        _sr.Close();
        _fs.Dispose();
    }

    #endregion

    /// <summary>
    /// Sorting indexes based on row contents
    /// </summary>
    private void Sort()
    {
        _sortedPositions = GetLinePositions();

        _sortedPositions.Sort((index1, index2) => SortMethod(index1, index2));
    }

    static long _progress = 0;
    private int SortMethod(long index1, long index2)
    {
        _progress++;

        string x = ReadLineAtPosition(index1);
        string y = ReadLineAtPosition(index2);

        if (_progress++ > 1000000L)
        {
            Console.WriteLine($"{index1}: {x} \n {index2}: {y}");
            _progress = 0L;
        }

        var xSpan = x.AsSpan();
        var ySpan = y.AsSpan();

        var xDotIndex = xSpan.IndexOf('.');
        var yDotIndex = ySpan.IndexOf('.');

        var xTextPart = xSpan.Slice(xDotIndex + 1);
        var yTextPart = ySpan.Slice(yDotIndex + 1);

        int textComparison = xTextPart.CompareTo(yTextPart, StringComparison.Ordinal);
        if (textComparison != 0)
        {
            return textComparison;
        }

        var xNumberPart = xSpan.Slice(0, xDotIndex);
        var yNumberPart = ySpan.Slice(0, yDotIndex);

        var xNumber = long.Parse(xNumberPart);
        var yNumber = long.Parse(yNumberPart);

        return (xNumber == yNumber) ? 0 : (xNumber > yNumber ? 1 : -1);
    }

    private void GoToPosition(long position)
    {
        _fs.Seek(position, SeekOrigin.Begin);

        _sr.DiscardBufferedData();
    }
    private string ReadLineAtPosition(long position)
    {
        //position = 104850748L;
        GoToPosition(position);

        return _sr.ReadLine();

        //var debug = _sr.ReadLine();

        //Console.WriteLine($"{position}: {debug}");
        //return debug;
    }

    //private List<long> GetLinePositions()
    //{
    //    var positions = new List<long>(capacity: 1000000);
    //    long position = 0;
    //    _fs.Seek(0, SeekOrigin.Begin);
    //    _sr.DiscardBufferedData();

    //    string line;
    //    while ((line = _sr.ReadLine()) != null)
    //    {
    //        positions.Add(position);
    //        // position = _fs.Position;
    //        position += _sr.CurrentEncoding.GetByteCount(line);
    //    }

    //    return positions;
    //}

    public List<long> GetLinePositions()
    {
        var positions = new List<long>(capacity: 1000000);

        long position = 0;
        int byteRead;
        bool isNewLine = true;

        while ((byteRead = _fs.ReadByte()) != -1)
        {
            if (isNewLine)
            {
                positions.Add(position);
                isNewLine = false;
            }

            if (byteRead == '\n')
            {
                isNewLine = true;
            }

            position++;
        }

        return positions;
    }
}



