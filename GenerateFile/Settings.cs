namespace GenerateFile;

internal sealed class Settings
{
    public string DictionaryFileName { get; set; }
    public OutputParams OutputFileOptions { get; set; }

    internal sealed class OutputParams
    {
        public string OutputFileName { get; set; }
        public int TargetSizeInMBytes { get; set; }
        public int RandomMax { get; set; }
    }
}