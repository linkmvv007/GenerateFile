namespace SortTextFile.Interfaces;

internal interface IFoldersHelper
{
    string ChunksFolder { get; }
    string SortedFolder { get; }
    string BookIndexFolder { get; }

    string GetChunkFullNameFile(string srcFileName, long fileIndex);
    string GetSortedChunkFullNameFile(string srcFileName);
    string GetSortedFileFolder(string fileName);
    string GetBookIndexFile(string fileName);
    string GetBadFormatLinesNameFile(string fileName);
}
