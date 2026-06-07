using Moongate.Server.Services.Commands;

namespace Moongate.Tests.Server.Commands;

public sealed class ConsoleCommandServiceTests
{
    [Fact]
    public void Prompt_UsesMoongatePrefix()
    {
        Assert.Equal("MG> ", ConsoleCommandService.Prompt);
    }

    [Theory]
    [InlineData("exit")]
    [InlineData("exit now")]
    [InlineData(" stop")]
    [InlineData("QUIT")]
    public void IsLoopTerminatingCommand_ExitAliases_ReturnsTrue(string line)
    {
        Assert.True(ConsoleCommandService.IsLoopTerminatingCommand(line));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("help")]
    [InlineData("exitdoor")]
    [InlineData(".exit")]
    public void IsLoopTerminatingCommand_OtherInput_ReturnsFalse(string line)
    {
        Assert.False(ConsoleCommandService.IsLoopTerminatingCommand(line));
    }
}
