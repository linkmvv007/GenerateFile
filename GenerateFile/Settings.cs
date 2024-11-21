namespace GenerateFile;

internal sealed class Settings
{
    public string DictionaryFileName { get; init; }
    public OutputParams OutputFileOptions { get; init; }

    internal sealed class OutputParams
    {
        public string OutputFileName { get; init; }
        public int TargetSizeInMBytes { get; init; }
        public int RandomMax { get; init; }
        public int Multiplier { get; init; }
    }
}