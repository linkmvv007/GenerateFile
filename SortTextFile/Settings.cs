﻿namespace SortTextFile;

internal sealed class SplitterOptions
{
    public string? TempDirectory { get; init; }
    public int FileSizeInMb { get; init; }
    public int MaxNumberThreads { get; init; }
}