// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AppleFitnessWorkoutMapper.Models;

public sealed class Track
{
    public string Name { get; set; } = string.Empty;

    public DateTimeOffset Timestamp { get; set; }

    public IList<TrackPoint> Points { get; set; } = new List<TrackPoint>();
}
