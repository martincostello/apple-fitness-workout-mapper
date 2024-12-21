// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.AppleFitnessWorkoutMapper.Models;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services;

public class TrackParserTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Can_Parse_Track_Points_From_Disk()
    {
        // Arrange
        var target = CreateTarget();

        // Act
        var actual = await target.GetTracksAsync(TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldNotBeEmpty();
        actual.Count.ShouldBe(2);

        var track = actual[0];

        track.ShouldNotBeNull();
        track.Name.ShouldBe("Route 1");
        track.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 25, 35, TimeSpan.Zero));

        track.Points.ShouldNotBeNull();
        track.Points.ShouldNotBeEmpty();
        track.Points.Count.ShouldBe(2);

        var point = track.Points[0];

        point.ShouldNotBeNull();
        point.Latitude.ShouldBe(51.5080900);
        point.Longitude.ShouldBe(-0.1285907);
        point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 25, 35, TimeSpan.Zero));

        point = track.Points[1];

        point.ShouldNotBeNull();
        point.Latitude.ShouldBe(51.5013640);
        point.Longitude.ShouldBe(-0.1440787);
        point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 45, 12, TimeSpan.Zero));

        track = actual[1];

        track.ShouldNotBeNull();
        track.Name.ShouldBe("Route 2");
        track.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));

        track.Points.ShouldNotBeNull();
        track.Points.ShouldNotBeEmpty();
        track.Points.Count.ShouldBe(2);

        point = track.Points[0];

        point.ShouldNotBeNull();
        point.Latitude.ShouldBe(51.5013640);
        point.Longitude.ShouldBe(-0.1440787);
        point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));

        point = track.Points[1];

        point.ShouldNotBeNull();
        point.Latitude.ShouldBe(51.5080900);
        point.Longitude.ShouldBe(-0.1285907);
        point.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 45, 12, TimeSpan.Zero));
    }

    private TrackParser CreateTarget()
    {
        string? thisDirectory = Path.GetDirectoryName(GetType().Assembly.Location);

        var application = new ApplicationOptions()
        {
            DataDirectory = Path.Combine(thisDirectory!, "App_Data"),
        };

        var options = Microsoft.Extensions.Options.Options.Create(application);
        var logger = outputHelper.ToLogger<TrackParser>();

        return new TrackParser(options, logger);
    }
}
