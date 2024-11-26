namespace SortTextFile;

internal class Utils
{
    internal static void DeleteFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    internal static void DeleteFiles(IReadOnlyCollection<string> paths)
    {
        foreach (var item in paths)
        {
            DeleteFile(item);
        }
    }

    internal static void DeleteFiles(string folderPath)
    {
        DeleteFiles(Directory.GetFiles(folderPath));
    }

    void MergeFiles(string destFileName, List<string> srcFileNames)
    {
        using (var destStream = File.OpenWrite(destFileName))
        {
            foreach (string srcFileName in srcFileNames)
            {
                using (var srcStream = File.OpenRead(srcFileName))
                {
                    srcStream.CopyTo(destStream);
                }
            }
        }
    }
}
