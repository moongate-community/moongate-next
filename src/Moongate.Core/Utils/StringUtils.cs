using Moongate.Core.Buffers;
using Moongate.Core.Types;

namespace Moongate.Core.Utils;

/// <summary>
///     Provides utility methods for string operations, including various case conversion methods.
/// </summary>
public static class StringUtils
{
    /// <summary>
    ///     Converts a string to camelCase.
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "helloWorld"
    ///     "API_RESPONSE" becomes "apiResponse"
    ///     "user-id" becomes "userId"
    /// </example>
    public static string ToCamelCase(string text)
    {
        return ConvertCase(text, '\0', StringCasingType.Lower, StringCasingType.Title, true);
    }

    /// <summary>
    ///     Converts a string to dot.case.
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "hello.world"
    ///     "API_RESPONSE" becomes "api.response"
    /// </example>
    public static string ToDotCase(string text)
    {
        return ConvertCase(text, '.', StringCasingType.Lower, StringCasingType.Lower, true);
    }

    /// <summary>
    ///     Converts a string to kebab-case.
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "hello-world"
    ///     "API_RESPONSE" becomes "api-response"
    ///     "userId" becomes "user-id"
    /// </example>
    public static string ToKebabCase(string text)
    {
        return ConvertCase(text, '-', StringCasingType.Lower, StringCasingType.Lower, true);
    }

    /// <summary>
    ///     Converts a string to PascalCase.
    /// </summary>
    /// <example>
    ///     "hello_world" becomes "HelloWorld"
    ///     "api-response" becomes "ApiResponse"
    ///     "userId" becomes "UserId"
    /// </example>
    public static string ToPascalCase(string text)
    {
        return ConvertCase(text, '\0', StringCasingType.Title, StringCasingType.Title, true);
    }

    /// <summary>
    ///     Converts a string to path/case.
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "hello/world"
    ///     "API_RESPONSE" becomes "api/response"
    /// </example>
    public static string ToPathCase(string text)
    {
        return ConvertCase(text, '/', StringCasingType.Lower, StringCasingType.Lower, true);
    }

    /// <summary>
    ///     Converts a string to Sentence case.
    ///     Camel-case humps are NOT split: only whitespace, underscores and hyphens are word separators.
    /// </summary>
    /// <example>
    ///     "hello world" becomes "Hello world"
    ///     "API_RESPONSE" becomes "Api response"
    /// </example>
    public static string ToSentenceCase(string text)
    {
        return ConvertCase(text, ' ', StringCasingType.Title, StringCasingType.Lower, false);
    }

    /// <summary>
    ///     Converts a string to snake_case.
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "hello_world"
    ///     "APIResponse" becomes "api_response"
    ///     "userId" becomes "user_id"
    /// </example>
    public static string ToSnakeCase(string text)
    {
        return ConvertCase(text, '_', StringCasingType.Lower, StringCasingType.Lower, true);
    }

    /// <summary>
    ///     Converts a string to Title Case.
    /// </summary>
    /// <example>
    ///     "hello_world" becomes "Hello World"
    ///     "API_RESPONSE" becomes "Api Response"
    ///     "user-id" becomes "User Id"
    /// </example>
    public static string ToTitleCase(string text)
    {
        return ConvertCase(text, ' ', StringCasingType.Title, StringCasingType.Title, true);
    }

    /// <summary>
    ///     Converts a string to Train-Case.
    /// </summary>
    /// <example>
    ///     "hello_world" becomes "Hello-World"
    ///     "apiResponse" becomes "Api-Response"
    /// </example>
    public static string ToTrainCase(string text)
    {
        return ConvertCase(text, '-', StringCasingType.Title, StringCasingType.Title, true);
    }

    /// <summary>
    ///     Converts a string to UPPER_SNAKE_CASE (screaming snake case).
    /// </summary>
    /// <example>
    ///     "HelloWorld" becomes "HELLO_WORLD"
    ///     "apiResponse" becomes "API_RESPONSE"
    ///     "user-id" becomes "USER_ID"
    /// </example>
    public static string ToUpperSnakeCase(string text)
    {
        return ConvertCase(text, '_', StringCasingType.Upper, StringCasingType.Upper, true);
    }

    private static void AppendWord(ref ValueStringBuilder sb, ReadOnlySpan<char> word, StringCasingType casing)
    {
        if (word.IsEmpty)
        {
            return;
        }

        switch (casing)
        {
            case StringCasingType.Lower:
                for (var i = 0; i < word.Length; i++)
                {
                    sb.Append(char.ToLowerInvariant(word[i]));
                }

                break;

            case StringCasingType.Upper:
                for (var i = 0; i < word.Length; i++)
                {
                    sb.Append(char.ToUpperInvariant(word[i]));
                }

                break;

            case StringCasingType.Title:
                sb.Append(char.ToUpperInvariant(word[0]));

                for (var i = 1; i < word.Length; i++)
                {
                    sb.Append(char.ToLowerInvariant(word[i]));
                }

                break;
        }
    }

    private static string ConvertCase(
        string text,
        char separator,
        StringCasingType firstWordCasing,
        StringCasingType otherWordCasing,
        bool splitCamel
    )
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var span = text.AsSpan();
        var sb = ValueStringBuilder.Create(span.Length);

        try
        {
            var wordIndex = 0;
            var i = 0;

            while (i < span.Length)
            {
                while (i < span.Length && IsExplicitSeparator(span[i]))
                {
                    i++;
                }

                if (i >= span.Length)
                {
                    break;
                }

                var wordStart = i;
                i++;

                while (i < span.Length && !IsExplicitSeparator(span[i]))
                {
                    if (splitCamel)
                    {
                        // lowercase → uppercase: "fooBar" splits between o and B
                        if (char.IsLower(span[i - 1]) && char.IsUpper(span[i]))
                        {
                            break;
                        }

                        // acronym → word: "APIResponse" splits between I and R
                        if (i + 1 < span.Length &&
                            char.IsUpper(span[i - 1]) &&
                            char.IsUpper(span[i]) &&
                            char.IsLower(span[i + 1]))
                        {
                            break;
                        }
                    }

                    i++;
                }

                if (wordIndex > 0 && separator != '\0')
                {
                    sb.Append(separator);
                }

                var casing = wordIndex == 0 ? firstWordCasing : otherWordCasing;
                AppendWord(ref sb, span[wordStart..i], casing);

                wordIndex++;
            }

            return sb.ToString();
        }
        finally
        {
            sb.Dispose();
        }
    }

    private static bool IsExplicitSeparator(char c)
    {
        return c == ' ' || c == '\t' || c == '\n' || c == '\r' || c == '_' || c == '-';
    }
}
