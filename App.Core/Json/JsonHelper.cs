using System.IO;
using System.Text.Json;

namespace App.Core.Json;

public class JsonHelper : IJsonHelper
{
    public string? File_Path { get; set; }
    public string? Json_Content { get; set; }

    public JsonHelper(string? file_path = null)
    {
        File_Path = file_path;
    }

    public string LoadJsonContent()
    {
        if (string.IsNullOrWhiteSpace(File_Path) || !File.Exists(File_Path))
            throw new FileNotFoundException("The specified JSON file was not found.", File_Path);

        Json_Content = File.ReadAllText(File_Path);
        return Json_Content;
    }

    public T DeserializeJsonContent<T>(string? json_content = null)
    {
        if (string.IsNullOrWhiteSpace(json_content))
        {
            if (string.IsNullOrWhiteSpace(Json_Content))
                LoadJsonContent();
            json_content = Json_Content;
        }

        if (string.IsNullOrWhiteSpace(json_content))
            throw new ArgumentException("JSON content is empty.", nameof(json_content));

        return JsonSerializer.Deserialize<T>(json_content)!;
    }

    public object? UnwrapJsonValue(object? value)
    {
        if (value is not JsonElement element)
            return value;

        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString() ?? string.Empty,
            JsonValueKind.Number when element.TryGetInt32(out int number) => number,
            JsonValueKind.Number when element.TryGetDouble(out double doubleNumber) => doubleNumber,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new ArgumentException($"Unsupported JSON value: {element.ValueKind}")
        };
    }
}

public interface IJsonHelper
{
    string? File_Path { get; set; }
    string? Json_Content { get; set; }
    string LoadJsonContent();
    T DeserializeJsonContent<T>(string? json_content = null);
    object? UnwrapJsonValue(object? value);
}