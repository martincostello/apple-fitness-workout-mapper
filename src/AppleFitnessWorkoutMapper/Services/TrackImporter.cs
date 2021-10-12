﻿// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics;
using MartinCostello.AppleFitnessWorkoutMapper.Data;
using Microsoft.EntityFrameworkCore;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services;

public sealed partial class TrackImporter
{
    private readonly TracksContext _context;
    private readonly ILogger _logger;
    private readonly TrackParser _parser;

    public TrackImporter(
        TrackParser parser,
        TracksContext context,
        ILogger<TrackImporter> logger)
    {
        _parser = parser;
        _context = context;
        _logger = logger;
    }

    public async Task<int> ImportTracksAsync(CancellationToken cancellationToken = default)
    {
        var tracks = await _parser.GetTracksAsync(cancellationToken);

        Log.DeletingDatabase(_logger);

        await _context.Database.EnsureDeletedAsync(cancellationToken);

        Log.DeletedDatabase(_logger);
        Log.CreatingDatabase(_logger);

        await _context.Database.EnsureCreatedAsync(cancellationToken);
        await _context.Database.MigrateAsync(cancellationToken);

        Log.CreatedDatabase(_logger);
        Log.ImportingTracks(_logger, tracks.Count);

        var stopwatch = Stopwatch.StartNew();

        foreach (var track in tracks)
        {
            var trackDB = new Data.Track()
            {
                Name = track.Name,
                Timestamp = track.Timestamp.UtcDateTime,
            };

            trackDB = (await _context.Tracks.AddAsync(trackDB, cancellationToken)).Entity;

            var points = new List<Data.TrackPoint>(track.Points.Count);

            foreach (var point in track.Points)
            {
                var pointDB = new Data.TrackPoint()
                {
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    Timestamp = point.Timestamp.UtcDateTime,
                    TrackId = trackDB.Id,
                };

                points.Add(pointDB);
            }

            await _context.TrackPoints.AddRangeAsync(points, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        Log.ImportedTracks(_logger, tracks.Count, stopwatch.Elapsed);

        return tracks.Count;
    }

    private static partial class Log
    {
        [LoggerMessage(8, LogLevel.Information, "Deleting existing database.")]
        public static partial void DeletingDatabase(ILogger logger);

        [LoggerMessage(9, LogLevel.Information, "Existing database deleted.")]
        public static partial void DeletedDatabase(ILogger logger);

        [LoggerMessage(10, LogLevel.Information, "Creating new database.")]
        public static partial void CreatingDatabase(ILogger logger);

        [LoggerMessage(11, LogLevel.Information, "Created new database.")]
        public static partial void CreatedDatabase(ILogger logger);

        [LoggerMessage(12, LogLevel.Information, "Importing {Count} track(s) to database.")]
        public static partial void ImportingTracks(ILogger logger, int count);

        [LoggerMessage(13, LogLevel.Information, "Imported {Count} track(s) to database in {Elapsed}.")]
        public static partial void ImportedTracks(ILogger logger, int count, TimeSpan elapsed);
    }
}
