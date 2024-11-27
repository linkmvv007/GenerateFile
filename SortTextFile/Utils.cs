using SortTextFile.Interfaces;

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

    internal static void MergeFiles(string destFileName, IOrderedEnumerable<string> srcFileNames, IFoldersHelper helper, bool isDeleteFiles = false)
    {
        using (var destStream = File.OpenWrite(destFileName))
        {
            foreach (string name in srcFileNames)
            {
                var srcFileName = helper.GetSortedChunkFullNameFile(name);

                using (var srcStream = File.OpenRead(srcFileName))
                {
                    srcStream.CopyTo(destStream);
                }

                if (isDeleteFiles)
                    Utils.DeleteFile(srcFileName);
            }
        }
    }

    internal static void ClearFolder(string path, bool recursive = true)
    {
        if (Directory.Exists(path))
        {
            var directory = new DirectoryInfo(path);

            foreach (var file in directory.GetFiles())
            {
                file.Delete();
            }

            foreach (var dir in directory.GetDirectories())
            {
                dir.Delete(recursive);
            }
        }
    }

}
