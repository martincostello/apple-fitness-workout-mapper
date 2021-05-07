// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;

namespace MartinCostello.AppleFitnessWorkoutMapper.Models
{
    public sealed class TrackPoint
    {
        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
