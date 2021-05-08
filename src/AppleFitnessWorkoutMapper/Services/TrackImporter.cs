// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkoutMapper.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services
{
    public sealed class TrackImporter
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

            _logger.LogInformation("Deleting existing database.");

            await _context.Database.EnsureDeletedAsync(cancellationToken);

            _logger.LogInformation("Existing database deleted.");
            _logger.LogInformation("Creating new database.");

            await _context.Database.EnsureCreatedAsync(cancellationToken);
            await _context.Database.MigrateAsync(cancellationToken);

            _logger.LogInformation("Created new database.");
            _logger.LogInformation("Importing {Count} track(s) to database.", tracks.Count);

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

            _logger.LogInformation("Imported {Count} track(s) to database in {Elapsed}.", tracks.Count, stopwatch.Elapsed);

            return tracks.Count;
        }
    }
}
