using System.Text.Json;

namespace UserApi.Infrastructures.Extensions;

public static class StringExtensions
{
    public static string ToSnakeCase(this string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return JsonNamingPolicy.SnakeCaseLower.ConvertName(input);
    }
}