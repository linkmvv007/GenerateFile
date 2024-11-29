using SortTextFile;
using SortTextFile.Configuration;
using SortTextFile.Interfaces;
using System.Diagnostics;

if (args.Length < 1)
{
    Console.WriteLine("File for sorting is not specified");
    return;
}

if (!File.Exists(args[0]))
{
    Console.WriteLine("File for sorting was not found");
    return;
}

var srcFile = args[0].TrimEnd();

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

var settings = Configuration.ReadConfig();
IFoldersHelper folderHelper = new FoldersHelper(settings.TempDirectory);

try
{
    //step 1:  split
    IFileSplitterLexicon splitter = new FileSplitterLexicon(srcFile, folderHelper, settings.LengthBookIndex);
    splitter.SplitWithInfo();


    //step 2: sorting & merge blocks
    IParallelSorting processor = new ParallelSorting(
        splitter.GetIndexs
        , folderHelper
        , settings.MaxNumberThreads
        , settings.IsDeleteFile
        );
    processor.Process();


    // mergeresults:
    Console.WriteLine("Finish Merging ....");

    Utils.MergeFiles(
        FoldersHelper.GetResultSortedNameFile(srcFile)
        , splitter.GetIndexs.OrderBy(x => x)
        , folderHelper
        , settings.IsDeleteFile);

    //chunks removes
    if (settings.IsDeleteFile)
    {
        Utils.ClearFolder(folderHelper.ChunksFolder);
    }

    Console.WriteLine("Finish Merging .... Ok");


    Console.WriteLine($"The number of rows is not in the format: {splitter.ErrorsCount}");
    Console.WriteLine($"See file : {folderHelper.GetBadFormatLinesNameFile(srcFile)}");
}
catch (Exception ex)
{
    Console.WriteLine(ex.ToString());
}

stopwatch.Stop();

TimeSpan ts = stopwatch.Elapsed;
Console.WriteLine($"Execution time: {ts.ToString(@"hh\:mm\:ss\.fffffff")}");


Console.WriteLine("Press any key ...");
Console.ReadKey();
