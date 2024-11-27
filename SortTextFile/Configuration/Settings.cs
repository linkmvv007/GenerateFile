namespace SortTextFile.Configuration;

internal sealed class SplitterOptions
{
    public string? TempDirectory { get; init; }
    public int FileSizeInMb { get; init; }
    public int MaxNumberThreads { get; init; }


    public bool IsDeleteFile { get; init; }
    public int LengthBookIndex { get; init; }
}