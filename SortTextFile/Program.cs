// See https://aka.ms/new-console-template for more information
using SortTextFile;
using System.Diagnostics;

Console.WriteLine("Hello, World!");


//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output2Gb.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output20mb.txt";
const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\test.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output1.txt";

/*List<long> lineStartPositions = new List<long>();

using (FileStream fs = new FileStream(srcFile, FileMode.Open, FileAccess.Read))
using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, true, 10 * 1024 * 1024)) // Чтение блоками по 1 МБ
{
    long position = 0;
    string line;
    while ((line = sr.ReadLine()) != null)
    {
        lineStartPositions.Add(position);
        position += line.Length + Environment.NewLine.Length;

        // Анализ строки
        AnalyzeLine(line);
    }
}

// Вывод позиций начала строк
foreach (var pos in lineStartPositions)
{
    //Console.WriteLine(pos);
}


static void AnalyzeLine(string line)
{
    // Ваш код для анализа строки
    // Console.WriteLine($"Анализ строки: {line}");
}

*/




Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
//using (IFileSorting file = new TextFileLinePositions(srcFile))
//{
//    file.MakeDictionary();
//}


var settings = Configuration.ReadConfig();

/*
using (IFileSorting file = new TextFileLinePositions(srcFile))  // src file
using (var writer = new WriteToFile("sorted.txt"))              // output file
{

    IOutputWriter processor = new WriterProcessor(file);
    processor.SortingAndWriteToOutput(writer);
}*/



IFileSplitter splitter = new FileSplitter(srcFile, settings);
splitter.Split();

ISortAndMergeFiles mergeAndSort = new SortAndMergeFiles(splitter, settings);
mergeAndSort.Sort();


Console.WriteLine($"Max lines {splitter.MaxLinesCount}");

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
