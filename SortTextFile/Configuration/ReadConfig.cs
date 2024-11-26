using Microsoft.Extensions.Configuration;

namespace SortTextFile.Configuration;

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

        return settings;
    }
}
