using System.Text.Json.Serialization;

public class ServerSettings
{
    [JsonPropertyName("prefixes")]
    public string[] Prefixes { get; set; } = new[] { "http://127.0.0.1:8888/" };
    
    [JsonPropertyName("staticFilesPath")]
    public string StaticFilesPath { get; set; } = "static";
    
    [JsonPropertyName("defaultFile")]
    public string DefaultFile { get; set; } = "index.html";
}