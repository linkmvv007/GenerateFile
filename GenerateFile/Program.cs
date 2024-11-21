using GenerateFile;
using Microsoft.Extensions.Configuration;
using System.Text;
using static GenerateFile.Settings;

class Program
{
    const string Options = "Options";
    private readonly static Random random = new();
    private readonly static int newLineLength = Encoding.UTF8.GetByteCount(Environment.NewLine);

    static void Main(string[] args)
    {
        Settings settings = ReadConfig();

        var words = LoadEnglishDictionaryFromFile(settings.DictionaryFileName);

        GenerateTestFile(settings.OutputFileOptions, words);
    }

    private static Settings ReadConfig()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // reading the original settings:
        var settings = new Settings();
        config.GetSection(Options).Bind(settings);
        return settings;
    }

    private static IReadOnlyList<string> LoadEnglishDictionaryFromFile(string filePath)
    {
        var words = new List<string>(capacity: 2000);

        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                { }
                words.Add(line);
            }
        }

        return words.AsReadOnly();
    }

    private static void GenerateTestFile(OutputParams parameters, IReadOnlyList<string> words)
    {
        var countLines = 0L;
        var currentSize = 0L;
        long targetSizeInBytes = parameters.TargetSizeInMBytes * 1024 * 1024L;

        using (var sw = new StreamWriter(parameters.OutputFileName))
        {
            while (currentSize < targetSizeInBytes)
            {
                string line;
                var number = random.Next(1, parameters.RandomMax);
                var fract = countLines % parameters.Multiplier;

                line = $"{number}.{GenerateNewWord(words)}";
                if (fract > 0)
                {
                    for (int i = 0; i < fract; i++)
                    {
                        WriteToFile(sw, line, ref currentSize);
                        line = $" {GenerateNewWord(words)}";
                    }
                }

                WriteLnToFile(sw, line, ref currentSize);

                countLines++;
            }
        }

        Console.WriteLine($"The file '{parameters.OutputFileName}' has been created and has a size of {targetSizeInBytes}, real size: {currentSize} bytes, count lines = {countLines}.");
    }

    private static void WriteToFile(StreamWriter sw, string line, ref long currentSize)
    {
        sw.Write(line);
        currentSize += Encoding.UTF8.GetByteCount(line);
    }
    private static void WriteLnToFile(StreamWriter sw, string line, ref long currentSize)
    {
        WriteToFile(sw, line, ref currentSize);

        sw.WriteLine();
        currentSize += newLineLength;
    }

    private static string GenerateNewWord(IReadOnlyList<string> words) =>
         words[random.Next(words.Count)];

}