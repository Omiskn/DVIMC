using System.IO;
using FluxConvert.Core.Infrastructure;
using FluxConvert.Core.Process;
using FluxConvert.Core.Settings;
using FluxConvert.Core.Shell;
using FluxConvert.Core.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluxConvert.App.Composition;

/// <summary>
/// The application's composition root.
/// </summary>
/// <remarks>
/// Every long-lived service is registered here so that the rest of the application never creates
/// its own dependencies. The container is created once during startup and disposed during
/// shutdown, which also flushes and closes the file logger.
/// </remarks>
public static class AppServices
{
    /// <summary>
    /// Builds the application's service container.
    /// </summary>
    /// <param name="paths">
    /// The application data paths. When <see langword="null"/> the default location is used.
    /// </param>
    /// <returns>A configured and validated service provider.</returns>
    public static ServiceProvider Build(AppPaths? paths = null)
    {
        paths ??= new AppPaths();
        TryEnsureCreated(paths);

        var services = new ServiceCollection();

        services.AddSingleton(paths);
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IProcessRunner, ProcessRunner>();
        services.AddSingleton<IConversionActivity, ConversionActivityTracker>();

        // The search context is built from the settings at the moment it is first resolved, so a
        // path the user configured is always taken into account.
        services.AddSingleton(provider => ToolSearchContext.CreateDefault(
            provider.GetRequiredService<ISettingsService>().Current,
            AppContext.BaseDirectory));
        services.AddSingleton<IToolLocator>(provider => new ToolLocator(
            provider.GetRequiredService<ToolSearchContext>(),
            provider.GetRequiredService<IProcessRunner>()));

        var fileLogger = new FileLoggerProvider(paths.LogDirectory, LogLevel.Information);
        services.AddSingleton(fileLogger);
        services.AddLogging(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information);
            builder.AddProvider(fileLogger);
        });

        return services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });
    }

    private static void TryEnsureCreated(AppPaths paths)
    {
        try
        {
            paths.EnsureCreated();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // A read-only or unavailable data directory must not stop the application from
            // starting; the failure is reported again when a write is actually attempted.
        }
    }
}
