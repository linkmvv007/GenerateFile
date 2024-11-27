namespace SortTextFile.Interfaces;

interface IFileSplitterLexicon
{
    void SplitWithInfo();

    HashSet<string> GetIndexs { get; }

    long ErrorsCount { get; }
}