using System.Net;

namespace StudentCardsServer;

public static class MiniTemplate
{
    public static string Render(string template, IReadOnlyDictionary<string, string> values)
    {
        var result = template;

        foreach (var kv in values)
            result = result.Replace("{{" + kv.Key + "}}", kv.Value);

        return result;
    }

    public static string Escape(string value)
        => WebUtility.HtmlEncode(value);
}
