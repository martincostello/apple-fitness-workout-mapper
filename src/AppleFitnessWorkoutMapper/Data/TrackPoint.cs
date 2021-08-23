// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace MartinCostello.AppleFitnessWorkoutMapper.Data
{
    [Index(nameof(Timestamp))]
    [Index(nameof(TrackId))]
    public class TrackPoint
    {
        [Key]
        public Guid Id { get; set; }

        public double Longitude { get; set; }

        public double Latitude { get; set; }

        public DateTime Timestamp { get; set; }

        public Guid TrackId { get; set; }
    }
}
