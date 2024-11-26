namespace SortTextFile;

internal class FolderHelper
{
    private const string BookIndexFolderName = "BookIndex";
    private const string ChunksFolderName = "Chunks";
    private const string SotedFolderName = "SortedResults";
    private const string TempFolder = "Temp";

    private readonly string _chunksFolder;
    private readonly string _sortedFolder;
    private readonly string _bookIndexFolder;
    private readonly string _tempFolder;
    internal FolderHelper(string tempDir)
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

    internal string ChunksFolder => _chunksFolder;
    internal string SortedFolder => _sortedFolder;
    internal string BookIndexFolder => _bookIndexFolder;

    private static string GetChunkNameFile(string srcFileName, long fileIndex) =>
   $"{Utils.FixFileName(srcFileName)}_chunk_{fileIndex}";
    internal string GetChunkFullNameFile(string srcFileName, long fileIndex) =>
        Path.Combine(_chunksFolder, $"{GetChunkNameFile(srcFileName, fileIndex)}.ind");

    internal string GetSortedChunkFullNameFile(string srcFileName) =>
       $"{GetSortedFileFolder(Utils.FixFileName(srcFileName))}.sorted";

    internal string GetSortedFileFolder(string fileName)
    {
        var path = Path.Combine(_sortedFolder, fileName[0..1]);
        Directory.CreateDirectory(path);

        return Path.Combine(path, fileName);
    }

    internal string GetBookIndexFile(string fileName) => Path.Combine(_bookIndexFolder, fileName);

    internal static string GetResultSortedNameFile(string srcFileName) =>
        $"{Utils.FixFileName(srcFileName)}.sorted";

}
