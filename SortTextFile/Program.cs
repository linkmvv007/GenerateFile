// See https://aka.ms/new-console-template for more information
using SortTextFile;
using SortTextFile.Configuration;
using SortTextFile.Interfaces;
using System.Diagnostics;

Console.WriteLine("Hello, World!");


//100L * 1024 * 1024 * 1024; // 100 ГБ в байтах  => 107374182400

const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output2Gb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output!.txt";

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output.txt";

//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output20mb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\test.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output1.txt";

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();


var settings = Configuration.ReadConfig();
IFoldersHelper folderHelper = new FoldersHelper(settings.TempDirectory);


//step 1:  split
IFileSplitterLexicon splitter = new FileSplitterLexicon(srcFile, folderHelper, settings.LengthBookIndex);
splitter.SplitWithInfo();
/*

//step 2: sorting & merge blocks
ISortAndMergeTextBlocks processor = new SortAndMergeTextBlocks(splitter, folderHelper);
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
*/

Console.WriteLine($"The number of rows is not in the format: {splitter.ErrorsCount}");
Console.WriteLine($"See file : {folderHelper.GetBadFormatLinesNameFile(srcFile)}");


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
