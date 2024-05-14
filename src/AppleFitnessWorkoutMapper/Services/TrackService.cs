// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.AppleFitnessWorkoutMapper.Data;
using Microsoft.EntityFrameworkCore;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services;

public class TrackService(TracksContext context)
{
    public async Task<DateTime?> GetLatestTrackAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);
        return await context.Tracks.MaxAsync((p) => (DateTime?)p.Timestamp, cancellationToken);
    }

    public async Task<int> GetTrackCountAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        return await context.Tracks.CountAsync(cancellationToken);
    }

    public async Task<IList<Models.Track>> GetTracksAsync(
        DateTimeOffset? since = null,
        DateTimeOffset? until = null,
        CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseAsync(cancellationToken);

        var notBefore = (since ?? DateTimeOffset.MinValue).UtcDateTime;
        var notAfter = (until ?? DateTimeOffset.MaxValue).UtcDateTime;

        var tracks = await context.Tracks
            .Where((p) => p.Timestamp > notBefore)
            .Where((p) => p.Timestamp < notAfter)
            .OrderBy((p) => p.Timestamp)
            .ToListAsync(cancellationToken);

        var result = new List<Models.Track>();

        foreach (var track in tracks)
        {
            var trackModel = new Models.Track()
            {
                Name = track.Name,
                Timestamp = new DateTimeOffset(track.Timestamp, TimeSpan.Zero),
            };

            var points = context.TrackPoints
                .Where((p) => p.TrackId == track.Id)
                .OrderBy((p) => p.Timestamp)
                .AsAsyncEnumerable()
                .WithCancellation(cancellationToken);

            await foreach (var point in points)
            {
                var pointModel = new Models.TrackPoint()
                {
                    Latitude = point.Latitude,
                    Longitude = point.Longitude,
                    Timestamp = new DateTimeOffset(point.Timestamp, TimeSpan.Zero),
                };

                trackModel.Points.Add(pointModel);
            }

            result.Add(trackModel);
        }

        return result;
    }

    private async Task EnsureDatabaseAsync(CancellationToken cancellationToken)
        => await context.Database.EnsureCreatedAsync(cancellationToken);
}
