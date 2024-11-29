using GenerateFile;
using Microsoft.Extensions.Configuration;
using System.Text;

const string Options = "Options";


Settings settings = ReadConfig();

var wordsEngl = LoadDictionaryFromFile(
    Path.Combine("Dictionary"
    , settings.DictionaryEnglishFileName
    ));

var allWords = LoadDictionaryFromFile(
    Path.Combine("Dictionary",
    settings.DictionaryRussianFileName
    ));

allWords.AddRange(wordsEngl);

GenerateTestFile(args[0], settings.OutputFileOptions, allWords.AsReadOnly());


static Settings ReadConfig()
{
    IConfiguration config = new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build();

    // reading the original settings:
    var settings = new Settings();
    config.GetSection(Options).Bind(settings);
    return settings;
}

static List<string> LoadDictionaryFromFile(string filePath)
{
    var words = new List<string>(capacity: 500_000);

    using (var reader = new StreamReader(filePath))
    {
        string line;
        while ((line = reader.ReadLine()) != null)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                words.Add(line);
            }
        }
    }

    return words;
}

static void Progress(ref long progress, ref long targetSizeInBytes, ref long currentSize)
{
    var tmp = currentSize * 100 / targetSizeInBytes;
    if (tmp != progress)
    {
        Console.Write($"\r.... {tmp}%");
        progress = tmp;
    }
}
void GenerateTestFile(string outputFile, OutputParams parameters, IReadOnlyList<string> words)
{
    Random random = new();
    int newLineLength = Encoding.UTF8.GetByteCount(Environment.NewLine);

    var countLines = 0L;
    var currentSize = 0L;
    long targetSizeInBytes = parameters.TargetSizeInMBytes * 1024 * 1024L;

    var progress = 0L;

    var outputFileName = string.IsNullOrWhiteSpace(outputFile) ? parameters.OutputFileName : outputFile;
    using (var sw = new StreamWriter(outputFileName))
    {
        while (currentSize < targetSizeInBytes)
        {
            Progress(ref progress, ref targetSizeInBytes, ref currentSize);

            string line;
            var number = random.Next(1, words.Count);
            var fract = countLines % parameters.Multiplier;

            line = $"{number}.{GenerateNewWord(words, ref random)}";
            if (fract > 0)
            {
                for (int i = 0; i < fract; i++)
                {
                    WriteToFile(sw, line, ref currentSize);
                    line = $" {GenerateNewWord(words, ref random)}";
                }
            }

            WriteLnToFile(sw, line, ref currentSize, newLineLength);

            countLines++;
        }
    }

    Progress(ref progress, ref targetSizeInBytes, ref currentSize);
    Console.WriteLine();
    Console.WriteLine($"The file '{parameters.OutputFileName}' has been created and has a size of {targetSizeInBytes}, real size: {currentSize} bytes, count lines = {countLines}.");
}

static void WriteToFile(StreamWriter sw, string line, ref long currentSize)
{
    sw.Write(line);
    currentSize += Encoding.UTF8.GetByteCount(line);
}
static void WriteLnToFile(StreamWriter sw, string line, ref long currentSize, int newLineLength)
{
    WriteToFile(sw, line, ref currentSize);

    sw.WriteLine();
    currentSize += newLineLength;
}

static string GenerateNewWord(IReadOnlyList<string> words, ref Random random) =>
    words[random.Next(words.Count)];

