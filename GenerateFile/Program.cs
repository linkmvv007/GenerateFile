using GenerateFile;
using Microsoft.Extensions.Configuration;
using System.Text;
using static GenerateFile.Settings;

class Program
{
    static void Main(string[] args)
    {
        const string Options = "Options";

        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        Settings settings = new();
        config.GetSection(Options).Bind(settings);


        List<string> words = LoadEnglishDictionaryFromFile(settings.DictionaryFileName);

        GenerateTestFile(settings.OutputFileOptions, words);
    }

    static List<string> LoadEnglishDictionaryFromFile(string filePath)
    {
        var words = new List<string>();

        using (var reader = new StreamReader(filePath))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                words.Add(line);
            }
        }

        return words;
    }

    static void GenerateTestFile(OutputParams parameters, List<string> words)
    {
        var currentSize = 0;
        long targetSizeInBytes = parameters.TargetSizeInMBytes * 1024 * 1024L;

        var newLineLength = Encoding.UTF8.GetByteCount(Environment.NewLine);
        var count = words.Count;

        var random = new Random();

        using (var sw = new StreamWriter(parameters.OutputFileName))
        {
            var k = 0;

            while (currentSize < targetSizeInBytes)
            {
                string line;
                var number = random.Next(1, parameters.RandomMax);
                int fract = k % 5;

                if (fract > 0)
                {
                    var sb = new StringBuilder(words[random.Next(count)]);
                    for (int i = 0; i < fract; i++)
                    {
                        sb.Append(' ');
                        sb.Append(words[random.Next(count)]);
                    }

                    line = $"{number}.{sb.ToString()}";
                }
                else
                {
                    line = $"{number}.{words[random.Next(count)]}";

                }

                sw.WriteLine(line);

                currentSize += Encoding.UTF8.GetByteCount(line) + newLineLength;

                k++;
            }
        }

        Console.WriteLine($"The file '{parameters.OutputFileName}' has been created and has a size of {targetSizeInBytes} bytes.");
    }
}