// See https://aka.ms/new-console-template for more information
using SortTextFile;
using SortTextFile.Interfaces;
using System.Diagnostics;

Console.WriteLine("Hello, World!");


const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\test.txt";
//const string srcFile = "c:\\Users\\Dell\\source\\repos\\3deye\\GenerateFile\\GenerateFile\\bin\\Debug\\net8.0\\output1.txt";

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

using (IFileSorting file = new TextFileLinePositions(srcFile))
{

    IFileWriter processor = new WriterProcessor(file);

    processor.WriteToFile(new WriteToFile("sorted.txt"));
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