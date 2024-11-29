using SortTextFile.Interfaces;

internal sealed class ParallelSorting : IParallelSorting
{
    private readonly IFoldersHelper _folderHelper;
    private readonly HashSet<string> _indexesHash;
    private readonly int _threadsCount;
    private readonly bool _isDeleteFiles;

    internal ParallelSorting(HashSet<string> indexes, IFoldersHelper folderHelper, int threadsCount = 4, bool isDeleteFiles = true)
    {
        _indexesHash = indexes;
        _folderHelper = folderHelper;
        _threadsCount = threadsCount;
        _isDeleteFiles = isDeleteFiles;
    }

    void IParallelSorting.Process()
    {
        var count = _indexesHash.Count;

        Console.WriteLine("Sorting & merging ....");

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _threadsCount
        };


        var results = Parallel.ForEach(_indexesHash, parallelOptions, fileIndex =>
        {
            Console.WriteLine($"'{fileIndex}' \tsorting ....");
            ISortAndMergeTextBlocks sort = new SortAndMergeTextBlocks(_folderHelper, _isDeleteFiles);
            sort.GetBlocksAndSort(fileIndex);

            Console.WriteLine($"'{fileIndex}' \tsorting ....Ok");
        });

    }
}