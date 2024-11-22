namespace SortTextFile.Interfaces;

interface IFileSorting : IDisposable
{
    /// <summary>
    /// read line by position
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    string ReadLine(long position);

    /// <summary>
    /// 
    /// </summary>
    IReadOnlyList<long> SortedPositions { get; }
}