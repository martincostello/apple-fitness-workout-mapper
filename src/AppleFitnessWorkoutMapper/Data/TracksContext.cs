// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.EntityFrameworkCore;

namespace MartinCostello.AppleFitnessWorkoutMapper.Data
{
    public sealed class TracksContext : DbContext
    {
        public TracksContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Track> Tracks { get; set; } = null!;

        public DbSet<TrackPoint> TrackPoints { get; set; } = null!;
    }
}
