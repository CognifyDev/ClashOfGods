using System.Text.Json.Serialization;

namespace COG.Cosmetics;

public class PackageInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "Unknown Package";

    [JsonPropertyName("author")]
    public string Author { get; set; } = "Unknown";

    [JsonPropertyName("description")]
    public string Description { get; set; } = "";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0.0";
}
