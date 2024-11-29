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

var srcFile = args[0];

//100L * 1024 * 1024 * 1024; // 100 ГБ в байтах  => 107374182400

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output2Gb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output!.txt";

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output.txt";

// russian:

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output2Gb.txt";
//const string srcFile = "d:\\TempDir\\output50Gb.txt";
//const string srcFile = "d:\\TempDir\\output100Gb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output10Mb.txt";

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output20mb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\test.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output1.txt";

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

Console.WriteLine($"Время выполнения: {stopwatch.ElapsedMilliseconds} миллисекунд");
TimeSpan ts = stopwatch.Elapsed;

// Форматирование времени в минутах с долями секунд
string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}",
ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

// Вывод времени выполнения
Console.WriteLine($"Время выполнения: {elapsedTime} минут");


Console.WriteLine("Нажмите любую клавишу для выхода...");
Console.ReadKey();
