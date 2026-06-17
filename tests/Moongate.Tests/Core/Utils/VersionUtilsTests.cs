using System.Reflection;
using Moongate.Core.Utils;

namespace Moongate.Tests.Core.Utils;

public class VersionUtilsTests
{
    [Fact]
    public void GetVersion_AssemblyWithBuildMetadata_StripsHashSuffix()
    {
        // Synthetic assembly carrying an informational version with +build-metadata.
        var assembly = new FakeInformationalAssembly("1.2.3-preview.5+abc1234");

        var result = VersionUtils.GetVersion(assembly);

        Assert.Equal("1.2.3-preview.5", result);
    }

    [Fact]
    public void GetVersion_AssemblyWithoutBuildMetadata_ReturnsAsIs()
    {
        var assembly = new FakeInformationalAssembly("1.2.3");

        var result = VersionUtils.GetVersion(assembly);

        Assert.Equal("1.2.3", result);
    }

    [Fact]
    public void GetVersion_AssemblyWithoutInformationalVersion_FallsBackToAssemblyVersion()
    {
        var assembly = new FakeInformationalAssembly(null, new Version(4, 5, 6, 7));

        var result = VersionUtils.GetVersion(assembly);

        Assert.Equal("4.5.6.7", result);
    }

    [Fact]
    public void GetVersion_AssemblyWithWhitespaceInformationalVersion_FallsBackToAssemblyVersion()
    {
        var assembly = new FakeInformationalAssembly("   ", new Version(2, 0));

        var result = VersionUtils.GetVersion(assembly);

        Assert.Equal("2.0", result);
    }

    [Fact]
    public void GetVersion_CoreAssembly_ReturnsDeclaredVersion()
    {
        var result = VersionUtils.GetVersion();

        // Directory.Build.props declares Version = 0.1.0; informational version mirrors it.
        Assert.False(string.IsNullOrWhiteSpace(result));
        Assert.StartsWith("0.", result);
    }

    [Fact]
    public void GetVersion_NullAssembly_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => VersionUtils.GetVersion(null!));
    }

    [Fact]
    public void GetVersion_TargetAssembly_ReturnsInformationalVersion()
    {
        var assembly = typeof(VersionUtilsTests).Assembly;

        var result = VersionUtils.GetVersion(assembly);

        Assert.False(string.IsNullOrWhiteSpace(result));
    }

    [Fact]
    public void IsRunningFromDocker_UsesMoongateEnvironmentVariable()
    {
        var oldDockerValue = Environment.GetEnvironmentVariable("MOONGATE_IS_DOCKER");
        Environment.SetEnvironmentVariable("MOONGATE_IS_DOCKER", "true");

        try
        {
            Assert.True(PlatformUtils.IsRunningFromDocker());
        }
        finally
        {
            Environment.SetEnvironmentVariable("MOONGATE_IS_DOCKER", oldDockerValue);
        }
    }

    private sealed class FakeInformationalAssembly : Assembly
    {
        private readonly Version? _assemblyVersion;
        private readonly string? _informationalVersion;

        public FakeInformationalAssembly(string? informationalVersion, Version? assemblyVersion = null)
        {
            _informationalVersion = informationalVersion;
            _assemblyVersion = assemblyVersion;
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == typeof(AssemblyInformationalVersionAttribute) && _informationalVersion is not null)
            {
                return new Attribute[] { new AssemblyInformationalVersionAttribute(_informationalVersion) };
            }

            return Array.Empty<Attribute>();
        }

        public override AssemblyName GetName()
        {
            return new AssemblyName("Fake") { Version = _assemblyVersion };
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return attributeType == typeof(AssemblyInformationalVersionAttribute) && _informationalVersion is not null;
        }
    }
}
