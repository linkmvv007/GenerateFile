using System.Collections.ObjectModel;

namespace SortTextFile.Interfaces;

interface IFileSplitter
{
    void Split();
    ReadOnlyDictionary<long, long> InfoFiles { get; }

    int MaxLinesCount { get; }

    string SourceFileName { get; }
}