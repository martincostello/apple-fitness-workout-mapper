// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;

namespace MartinCostello.AppleFitnessWorkoutMapper.Models
{
    public sealed class Track
    {
        public DateTimeOffset Timestamp { get; set; }

        public IList<IList<TrackPoint>> Segments { get; set; } = new List<IList<TrackPoint>>();
    }
}
