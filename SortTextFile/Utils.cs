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

    internal static void MergeFiles(string destFileName, IOrderedEnumerable<string> srcFileNames, FolderHelper helper)
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

    internal static string FixFileName(string str)
    {
        return str;
        //if (str[str.Length - 1] != ' ')
        //    return str;

        //Span<char> span = str.ToCharArray();
        //int len = span.Length;
        //while (len > 0 && span[len - 1] == ' ')
        //{
        //    len--;
        //}
        //return new string(span.Slice(0, len)) + new string('!', span.Length - len);
    }
}
