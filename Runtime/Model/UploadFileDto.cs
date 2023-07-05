using Newtonsoft.Json;
using System.IO;

public class UploadFileDto
{
    public UploadFileDto(string fileDirectory, string token)
    {
        FileName = Path.GetFileName(fileDirectory);
        File = new MemoryStream(System.IO.File.ReadAllBytes(fileDirectory));
        Token = token;
    }

    public UploadFileDto(string fileName, Stream stream, string token)
    {
        FileName = fileName;
        File = stream;
        Token = token;
    }

    [JsonProperty("action")]
    public string Action => "upload";

    [JsonProperty("format")]
    public string Format => "json";

    [JsonProperty("bot")]
    public int bot => 1;

    [JsonProperty("ignorewarnings")]
    public int IgnoreWarnings => 1;

    [JsonProperty("filename")]
    public string FileName { get; }

    [JsonIgnore]
    public Stream File { get; }

    [JsonProperty("token")]
    public string Token { get; }
}
