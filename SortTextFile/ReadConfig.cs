using Microsoft.Extensions.Configuration;

namespace SortTextFile;

internal class Configuration
{
    internal static SplitterOptions ReadConfig()
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .Build();

        // reading the original settings:
        var settings = new SplitterOptions();
        config.GetSection("SplitterOptions").Bind(settings);

        if (settings is null)
        {
            throw new ArgumentNullException($"Error readind configuration in {nameof(settings)}. Check optionn in the appsetting.json");
        }

        if (string.IsNullOrWhiteSpace(settings.TempDirectory))
        {
            settings.TempDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Temp");
            Directory.CreateDirectory(settings.TempDirectory);
        }

        return settings;
    }
}
