﻿using SortTextFile.Interfaces;

internal sealed class ParallelSorting : IParallelSorting
{
    private readonly IFoldersHelper _folderHelper;
    private readonly HashSet<string> _indexesHash;
    private readonly int _threadsCount;

    internal ParallelSorting(HashSet<string> indexes, IFoldersHelper folderHelper, int threadsCount = 4)
    {
        _indexesHash = indexes;
        _folderHelper = folderHelper;
        _threadsCount = threadsCount;
    }

    void IParallelSorting.Process()
    {
        var count = _indexesHash.Count;
        //var it = 0L;
        Console.WriteLine("Merging ....");

        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = _threadsCount
        };


        var results = Parallel.ForEach(_indexesHash, parallelOptions, fileIndex =>
        {
            // Console.Write($"\r{it * 100 / count}%");
            Console.WriteLine($"'{fileIndex}' \tmerging ....");
            ISortAndMergeTextBlocks sort = new SortAndMergeTextBlocks(_folderHelper);
            sort.GetBlocksAndSort(fileIndex);
            //it++;

            //Console.WriteLine();
            Console.WriteLine($"'{fileIndex}' \tmerging ....Ok");
        });

    }
}