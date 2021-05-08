// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.AspNetCore.Hosting;
using Moq;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services
{
    public class TrackParserTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public TrackParserTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Can_Parse_Track_Points_From_Disk()
        {
            // Arrange
            TrackParser target = CreateTarget();

            // Act
            IList<Track>? actual = await target.GetTracksAsync();

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(2);

            Track track = actual[0];

            track.ShouldNotBeNull();
            track.Name.ShouldBe("Route 1");
            track.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 25, 35, TimeSpan.Zero));

            track.Segments.ShouldNotBeNull();
            track.Segments.ShouldNotBeEmpty();
            track.Segments.Count.ShouldBe(1);

            IList<TrackPoint> segment = track.Segments[0];

            segment.ShouldNotBeNull();
            segment.ShouldNotBeEmpty();
            segment.Count.ShouldBe(2);

            TrackPoint point = segment[0];

            point.ShouldNotBeNull();
            point.Latitude.ShouldBe(51.5080900);
            point.Longitude.ShouldBe(-0.1285907);
            point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 25, 35, TimeSpan.Zero));

            point = segment[1];

            point.ShouldNotBeNull();
            point.Latitude.ShouldBe(51.5013640);
            point.Longitude.ShouldBe(-0.1440787);
            point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 45, 12, TimeSpan.Zero));

            track = actual[1];

            track.ShouldNotBeNull();
            track.Name.ShouldBe("Route 2");
            track.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));

            track.Segments.ShouldNotBeNull();
            track.Segments.ShouldNotBeEmpty();
            track.Segments.Count.ShouldBe(1);

            segment = track.Segments[0];

            segment.ShouldNotBeNull();
            segment.ShouldNotBeEmpty();
            segment.Count.ShouldBe(1);

            point = segment[0];

            point.ShouldNotBeNull();
            point.Latitude.ShouldBe(51.5080900);
            point.Longitude.ShouldBe(-0.1285907);
            point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));
        }

        private TrackParser CreateTarget()
        {
            string? thisDirectory = Path.GetDirectoryName(GetType().Assembly.Location);

            var mock = new Mock<IWebHostEnvironment>();

            mock.Setup((p) => p.ContentRootPath)
                .Returns(thisDirectory!);

            var environment = mock.Object;
            var logger = _outputHelper.ToLogger<TrackParser>();

            return new TrackParser(environment, logger);
        }
    }
}
