using Moongate.Core.Extensions.Strings;
using Moongate.Core.Utils;

namespace Moongate.Tests.Core.Utils;

public class StringUtilsTests
{
    [Fact]
    public void Extension_ToCamelCase_DelegatesToUtils()
        => Assert.Equal("helloWorld", "HelloWorld".ToCamelCase());

    [Fact]
    public void Extension_ToPascalCase_DelegatesToUtils()
        => Assert.Equal("HelloWorld", "hello_world".ToPascalCase());

    [Fact]
    public void Extension_ToSnakeCase_DelegatesToUtils()
        => Assert.Equal("hello_world", "HelloWorld".ToSnakeCase());

    [Theory, InlineData("HelloWorld", "helloWorld"), InlineData("hello_world", "helloWorld"),
     InlineData("hello-world", "helloWorld"), InlineData("API_RESPONSE", "apiResponse"), InlineData("user-id", "userId"),
     InlineData("APIResponse", "apiResponse"), InlineData("a", "a"), InlineData("", "")]
    public void ToCamelCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToCamelCase(input));

    [Theory, InlineData("HelloWorld", "hello.world"), InlineData("API_RESPONSE", "api.response"), InlineData("", "")]
    public void ToDotCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToDotCase(input));

    [Theory, InlineData("HelloWorld", "hello-world"), InlineData("API_RESPONSE", "api-response"),
     InlineData("userId", "user-id"), InlineData("", "")]
    public void ToKebabCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToKebabCase(input));

    [Theory, InlineData("hello_world", "HelloWorld"), InlineData("api-response", "ApiResponse"),
     InlineData("userId", "UserId"), InlineData("HelloWorld", "HelloWorld"), InlineData("APIResponse", "ApiResponse"),
     InlineData("a", "A"), InlineData("", "")]
    public void ToPascalCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToPascalCase(input));

    [Theory, InlineData("HelloWorld", "hello/world"), InlineData("API_RESPONSE", "api/response"), InlineData("", "")]
    public void ToPathCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToPathCase(input));

    [Theory, InlineData("hello world", "Hello world"), InlineData("API_RESPONSE", "Api response"),
     InlineData("hello", "Hello"), InlineData("HELLO", "Hello"), InlineData("", "")]

    // Sentence case does NOT split camelCase humps - only whitespace, _, -.
    public void ToSentenceCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToSentenceCase(input));

    [Fact]
    public void ToSnakeCase_OnlySeparators_ReturnsEmpty()
    {
        Assert.Equal("", StringUtils.ToSnakeCase("___"));
        Assert.Equal("", StringUtils.ToSnakeCase("---"));
        Assert.Equal("", StringUtils.ToSnakeCase("   "));
    }

    [Theory, InlineData("HelloWorld", "hello_world"), InlineData("APIResponse", "api_response"),
     InlineData("userId", "user_id"), InlineData("hello world", "hello_world"), InlineData("hello-world", "hello_world"),
     InlineData("a", "a"), InlineData("", "")]
    public void ToSnakeCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToSnakeCase(input));

    [Theory, InlineData("hello_world", "Hello World"), InlineData("API_RESPONSE", "Api Response"),
     InlineData("user-id", "User Id"), InlineData("HelloWorld", "Hello World"), InlineData("", "")]
    public void ToTitleCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToTitleCase(input));

    [Theory, InlineData("hello_world", "Hello-World"), InlineData("apiResponse", "Api-Response"), InlineData("", "")]
    public void ToTrainCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToTrainCase(input));

    [Theory, InlineData("HelloWorld", "HELLO_WORLD"), InlineData("apiResponse", "API_RESPONSE"),
     InlineData("user-id", "USER_ID"), InlineData("", "")]
    public void ToUpperSnakeCase_VariousInputs(string input, string expected)
        => Assert.Equal(expected, StringUtils.ToUpperSnakeCase(input));
}
