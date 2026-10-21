using FluxConvert.Core;
using FluxConvert.Core.Infrastructure;

namespace FluxConvert.Core.Tests;

/// <summary>
/// Groups the tests that change process-wide environment variables so they never run in
/// parallel with other tests.
/// </summary>
[CollectionDefinition(nameof(EnvironmentVariableTestGroup), DisableParallelization = true)]
public sealed class EnvironmentVariableTestGroup
{
}

[Collection(nameof(EnvironmentVariableTestGroup))]
public class AppPathsTests
{
    [Fact]
    public void DefaultRoot_LivesUnderLocalApplicationData()
    {
        var paths = new AppPaths();

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Assert.False(string.IsNullOrWhiteSpace(localAppData));
        Assert.Equal(Path.Combine(localAppData, ProductInfo.ProductName), paths.Root);
    }

    [Fact]
    public void EnvironmentVariable_OverridesTheRoot()
    {
        var overridePath = Path.Combine(Path.GetTempPath(), "fluxconvert-env-" + Guid.NewGuid().ToString("N"));
        var previous = Environment.GetEnvironmentVariable(AppPaths.RootEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(AppPaths.RootEnvironmentVariable, overridePath);

            var paths = new AppPaths();

            Assert.Equal(Path.GetFullPath(overridePath), paths.Root);
        }
        finally
        {
            Environment.SetEnvironmentVariable(AppPaths.RootEnvironmentVariable, previous);
        }
    }

    [Fact]
    public void ExplicitRoot_IsUsedVerbatim()
    {
        var root = Path.Combine(Path.GetTempPath(), "fluxconvert-explicit-" + Guid.NewGuid().ToString("N"));

        var paths = new AppPaths(root);

        Assert.Equal(Path.GetFullPath(root), paths.Root);
    }

    [Fact]
    public void KnownFiles_AreAllInsideTheRoot()
    {
        var paths = new AppPaths(Path.Combine(Path.GetTempPath(), "fluxconvert-files-" + Guid.NewGuid().ToString("N")));

        Assert.Equal(paths.Root, Path.GetDirectoryName(paths.SettingsFile));
        Assert.Equal(paths.Root, Path.GetDirectoryName(paths.HistoryFile));
        Assert.Equal(paths.Root, Path.GetDirectoryName(paths.LogDirectory));
        Assert.Equal(paths.Root, Path.GetDirectoryName(paths.TempDirectory));
        Assert.Equal("settings.json", Path.GetFileName(paths.SettingsFile));
        Assert.Equal("history.json", Path.GetFileName(paths.HistoryFile));
        Assert.Equal("logs", Path.GetFileName(paths.LogDirectory));
    }

    [Fact]
    public void EnsureCreated_CreatesTheDirectories()
    {
        var root = Path.Combine(Path.GetTempPath(), "fluxconvert-create-" + Guid.NewGuid().ToString("N"));
        var paths = new AppPaths(root);

        try
        {
            paths.EnsureCreated();

            Assert.True(Directory.Exists(paths.Root));
            Assert.True(Directory.Exists(paths.LogDirectory));
            Assert.True(Directory.Exists(paths.TempDirectory));
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, recursive: true);
            }
        }
    }
}
