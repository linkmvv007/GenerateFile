using SortTextFile.Interfaces;

namespace SortTextFile;

internal class FoldersHelper : IFoldersHelper
{
    private const string BookIndexFolderName = "BookIndex";
    private const string ChunksFolderName = "Chunks";
    private const string SotedFolderName = "SortedResults";
    private const string TempFolder = "Temp";

    private readonly string _chunksFolder;
    private readonly string _sortedFolder;
    private readonly string _bookIndexFolder;
    private readonly string _tempFolder;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="tempDir"></param>
    internal FoldersHelper(string? tempDir)
    {
        _tempFolder = string.IsNullOrWhiteSpace(tempDir)
            ? _tempFolder = Path.Combine(Directory.GetCurrentDirectory(), TempFolder)
            : tempDir;
        Directory.CreateDirectory(_tempFolder);

        Utils.ClearFolder(_tempFolder);


        _bookIndexFolder = Path.Combine(_tempFolder, BookIndexFolderName);
        Directory.CreateDirectory(_bookIndexFolder);

        _chunksFolder = Path.Combine(_tempFolder, ChunksFolderName);
        Directory.CreateDirectory(_chunksFolder);

        _sortedFolder = Path.Combine(_chunksFolder, SotedFolderName);
        Directory.CreateDirectory(_sortedFolder);

    }

    #region IFolderHelper

    string IFoldersHelper.ChunksFolder => _chunksFolder;
    string IFoldersHelper.SortedFolder => _sortedFolder;
    string IFoldersHelper.BookIndexFolder => _bookIndexFolder;

    string IFoldersHelper.GetChunkFullNameFile(string srcFileName, long fileIndex) =>
        Path.Combine(_chunksFolder, $"{GetChunkNameFile(srcFileName, fileIndex)}.ind");

    string IFoldersHelper.GetSortedChunkFullNameFile(string srcFileName) =>
       $"{(this as IFoldersHelper).GetSortedFileFolder(srcFileName)}.sorted";

    string IFoldersHelper.GetSortedFileFolder(string fileName)
    {
        var path = Path.Combine(_sortedFolder, fileName[0..1]);
        Directory.CreateDirectory(path);

        return Path.Combine(path, fileName);
    }

    string IFoldersHelper.GetBookIndexFile(string fileName) => Path.Combine(_bookIndexFolder, fileName);

    #endregion

    internal static string GetResultSortedNameFile(string srcFileName) =>
        $"{srcFileName}.sorted";

    private static string GetChunkNameFile(string srcFileName, long fileIndex) =>
        $"{srcFileName}_chunk_{fileIndex}";

}
