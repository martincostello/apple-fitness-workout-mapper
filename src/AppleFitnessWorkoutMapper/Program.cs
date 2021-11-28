﻿// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.IO.Compression;
using MartinCostello.AppleFitnessWorkoutMapper;
using MartinCostello.AppleFitnessWorkoutMapper.Data;
using MartinCostello.AppleFitnessWorkoutMapper.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using NodaTime;

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

    builder.Services.AddDbContext<TracksContext>((serviceProvider, builder) =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<ApplicationOptions>>();
        builder.UseSqlite("Data Source=" + options.Value.DatabaseFile);
    });

    builder.Services.TryAddSingleton<IClock>((_) => SystemClock.Instance);
    builder.Services.AddSingleton<TrackParser>();
    builder.Services.AddScoped<TrackImporter>();
    builder.Services.AddScoped<TrackService>();

    builder.Services.AddRazorPages();

    builder.Services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
    builder.Services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

    builder.Services.AddResponseCompression((p) =>
    {
        p.EnableForHttps = true;
        p.Providers.Add<BrotliCompressionProvider>();
        p.Providers.Add<GzipCompressionProvider>();
    });

    var app = builder.Build();

    app.UseResponseCompression();
    app.UseStaticFiles();
    app.UseRouting();
    app.MapRazorPages();

    var options = app.Services.GetRequiredService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>().Value;
    var context = new ApplicationJsonSerializerContext(options.SerializerOptions);

    app.MapGet("/api/tracks", async (
        TrackService service,
        DateTimeOffset? notBefore,
        DateTimeOffset? notAfter,
        CancellationToken cancellationToken) =>
    {
        var tracks = await service.GetTracksAsync(notBefore, notAfter, cancellationToken);
        return Results.Extensions.Json(tracks, context.IListTrack);
    });

    app.MapGet("/api/tracks/count", async (TrackService service, CancellationToken cancellationToken) =>
    {
        int count = await service.GetTrackCountAsync(cancellationToken);
        return Results.Extensions.Json(new() { Count = count }, context.TrackCount);
    });

    app.MapPost("/api/tracks/import", async (TrackImporter importer, CancellationToken cancellationToken) =>
    {
        int count = await importer.ImportTracksAsync(cancellationToken);
        return Results.Extensions.Json(new() { Count = count }, context.TrackCount, statusCode: StatusCodes.Status201Created);
    });

    app.Run();
}
