// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Data.Common;
using System.Diagnostics;
using System.IO.Compression;
using MartinCostello.AppleFitnessWorkoutMapper;
using MartinCostello.AppleFitnessWorkoutMapper.Data;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using MartinCostello.AppleFitnessWorkoutMapper.Services;
using MartinCostello.AppleFitnessWorkoutMapper.Slices;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;

try
{
    RunApplication(args);
    return 0;
}
catch (Exception ex) when (ex.InnerException is Microsoft.AspNetCore.Connections.AddressInUseException)
{
    var oldColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkRed;

    Console.Error.WriteLine();
    Console.Error.WriteLine(ex.Message);
    Console.Error.WriteLine(ex.InnerException.Message);
    Console.Error.WriteLine();
    Console.Error.WriteLine("Close any other copies of AppleFitnessWorkoutMapper that might be running and try again.");
    Console.Error.WriteLine();

    Console.ForegroundColor = oldColor;

    return -1;
}

static void RunApplication(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Services
        .Configure<ApplicationOptions>(builder.Configuration)
        .PostConfigure<ApplicationOptions>((options) =>
        {
            if (!string.IsNullOrEmpty(options.DataDirectory) && !Path.IsPathRooted(options.DataDirectory))
            {
                options.DataDirectory = Path.Combine(builder.Environment.ContentRootPath, options.DataDirectory);
            }

            if (string.IsNullOrEmpty(options.DataDirectory))
            {
                options.DataDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
            }

            if (string.IsNullOrEmpty(options.DatabaseFileName))
            {
                options.DatabaseFileName = "tracks.db";
            }

            options.DatabaseFile = Path.Combine(options.DataDirectory, options.DatabaseFileName);

            // Ensure the configured data directory exists
            if (!Directory.Exists(options.DataDirectory))
            {
                Directory.CreateDirectory(options.DataDirectory);
            }
        });

    builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(
        (options) => options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApplicationJsonSerializerContext.Default));

    builder.Services.Configure<StaticFileOptions>((options) =>
    {
        options.OnPrepareResponse = (context) =>
        {
            var maxAge = TimeSpan.FromDays(7);

            if (context.File.Exists)
            {
                string? extension = Path.GetExtension(context.File.PhysicalPath);

                // These files are served with a content hash in the URL so can be cached for longer
                bool isScriptOrStyle =
                    string.Equals(extension, ".css", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".js", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(extension, ".svg", StringComparison.OrdinalIgnoreCase);

                if (isScriptOrStyle)
                {
                    maxAge = TimeSpan.FromDays(365);
                }
            }

            context.Context.Response.GetTypedHeaders().CacheControl = new() { MaxAge = maxAge };
        };
    });

    const string Key = "Database";

    builder.Services.AddResilienceEnricher();
    builder.Services.AddResiliencePipeline(Key, (builder) =>
    {
        builder.AddRetry(new()
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(
                (ex) => ex is DbException or InvalidOperationException or TimeoutException),
        });
    });

    builder.Services.AddDbContext<TracksContext>((serviceProvider, builder) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>();
        var provider = serviceProvider.GetRequiredService<ResiliencePipelineProvider<string>>();

        builder.UseSqlite("Data Source=" + options.Value.DatabaseFile, (builder) =>
        {
            builder.ExecutionStrategy(
                (dependencies) => new ResilientExecutionStrategy(dependencies, provider.GetPipeline(Key)));
        });
    });

    builder.Services.TryAddSingleton(TimeProvider.System);
    builder.Services.AddSingleton<TrackParser>();
    builder.Services.AddScoped<TrackImporter>();
    builder.Services.AddScoped<TrackService>();

    if (!Debugger.IsAttached)
    {
        builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
        builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

        builder.Services.AddResponseCompression((p) =>
        {
            p.EnableForHttps = true;
            p.Providers.Add<BrotliCompressionProvider>();
            p.Providers.Add<GzipCompressionProvider>();
        });
    }

    var app = builder.Build();

    if (!Debugger.IsAttached)
    {
        app.UseResponseCompression();
    }

    app.UseStaticFiles();

    app.MapGet("/", async (
        TrackService service,
        TimeProvider timeProvider,
        IOptions<ApplicationOptions> options,
        CancellationToken cancellationToken) =>
    {
        var today = timeProvider.GetUtcNow().UtcDateTime.Date;
        var timestamp = await service.GetLatestTrackAsync(cancellationToken);

        timestamp ??= today;

        var endDate = timestamp.GetValueOrDefault().Date;

        if (endDate < today)
        {
            endDate = endDate.AddDays(1);
        }

        var model = new AppModel()
        {
            GoogleMapsApiKey = options.Value.GoogleMapsApiKey,
            StartDate = endDate.AddDays(-28).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            EndDate = endDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            TodayDate = today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        };

        return Results.Extensions.RazorSlice<App, AppModel>(model);
    });

    var api = app.MapGroup("/api/tracks");

    api.MapGet(string.Empty, static async (
        TrackService service,
        DateTimeOffset? notBefore,
        DateTimeOffset? notAfter,
        CancellationToken cancellationToken) =>
    {
        var tracks = await service.GetTracksAsync(notBefore, notAfter, cancellationToken);
        return Results.Json(tracks, ApplicationJsonSerializerContext.Default.ListTrack);
    });

    api.MapGet("/count", static async (TrackService service, CancellationToken cancellationToken) =>
    {
        int count = await service.GetTrackCountAsync(cancellationToken);
        return Results.Json(new TrackCount() { Count = count }, ApplicationJsonSerializerContext.Default.TrackCount);
    });

    api.MapPost("/import", static async (TrackImporter importer, CancellationToken cancellationToken) =>
    {
        int count = await importer.ImportTracksAsync(cancellationToken);
        return Results.Json(
            new TrackCount() { Count = count },
            ApplicationJsonSerializerContext.Default.TrackCount,
            statusCode: StatusCodes.Status201Created);
    });

    app.Run();
}
