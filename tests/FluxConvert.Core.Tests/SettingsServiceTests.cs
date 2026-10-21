using System.Text.Json;
using FluxConvert.Core.Infrastructure;
using FluxConvert.Core.Settings;
using Microsoft.Extensions.Logging.Abstractions;

namespace FluxConvert.Core.Tests;

public class SettingsServiceTests : IDisposable
{
    private readonly string _root;
    private readonly AppPaths _paths;

    public SettingsServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "fluxconvert-settings-" + Guid.NewGuid().ToString("N"));
        _paths = new AppPaths(_root);
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    private SettingsService CreateService() => new(_paths, NullLogger<SettingsService>.Instance);

    [Fact]
    public void Load_WithoutAFile_KeepsTheDefaults()
    {
        var service = CreateService();

        service.Load();

        Assert.Equal(AppSettings.DefaultQualityPresetName, service.Current.QualityPreset);
        Assert.Equal(2, service.Current.MaxConcurrentConversions);
        Assert.True(service.Current.StripMetadata);
        Assert.False(service.Current.ReducedMotion);
    }

    [Fact]
    public async Task SaveAsync_ThenLoad_RoundTripsTheValues()
    {
        var service = CreateService();
        service.Current.DefaultOutputDirectory = @"D:\Conversions";
        service.Current.QualityPreset = "HighQuality";
        service.Current.MaxConcurrentConversions = 4;
        service.Current.ReducedMotion = true;
        service.Current.FavoriteFormats = new[] { "webp", "mp3" };

        await service.SaveAsync();

        var reloaded = CreateService();
        reloaded.Load();

        Assert.Equal(@"D:\Conversions", reloaded.Current.DefaultOutputDirectory);
        Assert.Equal("HighQuality", reloaded.Current.QualityPreset);
        Assert.Equal(4, reloaded.Current.MaxConcurrentConversions);
        Assert.True(reloaded.Current.ReducedMotion);
        Assert.Equal(new[] { "webp", "mp3" }, reloaded.Current.FavoriteFormats);
    }

    [Fact]
    public async Task SaveAsync_WritesCamelCaseJson()
    {
        var service = CreateService();

        await service.SaveAsync();

        var json = File.ReadAllText(_paths.SettingsFile);
        using var document = JsonDocument.Parse(json);
        Assert.True(document.RootElement.TryGetProperty("defaultOutputDirectory", out _));
        Assert.True(document.RootElement.TryGetProperty("maxConcurrentConversions", out _));
    }

    [Fact]
    public void Load_WithUnknownMembers_StillSucceeds()
    {
        File.WriteAllText(
            _paths.SettingsFile,
            """{ "maxConcurrentConversions": 3, "somethingFromTheFuture": { "nested": true } }""");

        var service = CreateService();

        service.Load();

        Assert.Equal(3, service.Current.MaxConcurrentConversions);
    }

    [Fact]
    public void Load_WithCorruptJson_KeepsDefaultsAndMovesTheFileAside()
    {
        File.WriteAllText(_paths.SettingsFile, "{ this is not json ");

        var service = CreateService();

        service.Load();

        Assert.Equal(AppSettings.DefaultQualityPresetName, service.Current.QualityPreset);
        Assert.False(File.Exists(_paths.SettingsFile));
        Assert.Single(Directory.GetFiles(_root, "settings.corrupt-*.json"));
    }

    [Fact]
    public async Task SaveAsync_IsAtomic_AndLeavesNoTemporaryFile()
    {
        var service = CreateService();

        await service.SaveAsync();

        Assert.True(File.Exists(_paths.SettingsFile));
        Assert.False(File.Exists(_paths.SettingsFile + ".tmp"));
    }

    [Fact]
    public async Task SaveAsync_RaisesTheChangedEvent()
    {
        var service = CreateService();
        var raised = 0;
        service.Changed += (_, _) => raised++;

        await service.SaveAsync();

        Assert.Equal(1, raised);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-5, 1)]
    [InlineData(1, 1)]
    [InlineData(4, 4)]
    [InlineData(99, 8)]
    public void MaxConcurrentConversions_IsClamped(int requested, int expected)
    {
        var settings = new AppSettings { MaxConcurrentConversions = requested };

        Assert.Equal(expected, settings.MaxConcurrentConversions);
    }

    [Fact]
    public void Clone_ProducesAnIndependentCopy()
    {
        var original = new AppSettings { FavoriteFormats = new[] { "webp" } };

        var copy = original.Clone();
        copy.FavoriteFormats[0] = "mp3";
        copy.MaxConcurrentConversions = 7;

        Assert.Equal("webp", original.FavoriteFormats[0]);
        Assert.Equal(2, original.MaxConcurrentConversions);
    }
}
