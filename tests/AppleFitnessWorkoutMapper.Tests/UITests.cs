// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.AppleFitnessWorkoutMapper.Pages;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper;

public class UITests(ITestOutputHelper outputHelper) : IAsyncLifetime
{
    public ITestOutputHelper OutputHelper { get; set; } = outputHelper;

    public static TheoryData<string, string?> Browsers()
    {
        var browsers = new TheoryData<string, string?>()
        {
            { BrowserType.Chromium, null },
            { BrowserType.Firefox, null },
        };

        if (!OperatingSystem.IsWindows())
        {
            browsers.Add(BrowserType.Chromium, "chrome");
        }

        // HACK Skip on macOS. See https://github.com/microsoft/playwright-dotnet/issues/2920.
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Chromium, "msedge");
        }

        if (OperatingSystem.IsMacOS())
        {
            browsers.Add(BrowserType.Webkit, null);
        }

        return browsers;
    }

    [Theory]
    [MemberData(nameof(Browsers))]
    public async Task Can_View_Route_Info_Window(string browserType, string? browserChannel)
    {
        // Arrange
        var options = new BrowserFixtureOptions()
        {
            BrowserType = browserType,
            BrowserChannel = browserChannel,
        };

        using var fixture = new HttpWebApplicationFactory(OutputHelper);

        var browser = new BrowserFixture(options, OutputHelper);
        await browser.WithPageAsync(async (page) =>
        {
            await page.GotoAsync(fixture.ServerAddress);
            await page.WaitForLoadStateAsync();

            var app = new ApplicationPage(page);

            // Act - import data so routes are rendered on the real Google Maps.
            await app.ImportDataAsync();

            // Assert - wait for tracks and the map to be visible.
            await app.WaitForTracksAsync();
            await app.WaitForMapAsync();

            // Act - hover over a route to trigger the mouseover and show the info window.
            var infoWindowText = await app.RouteInfoWindowAsync();

            // Assert - the info window should display a non-empty duration and distance.
            // Before the fix, createInfoWindowContent was called at route creation time,
            // before this.displayDuration and this.displayDistance were set in the
            // TrackPath constructor, so the info window would show "undefined" for both.
            infoWindowText.ShouldNotBeNullOrEmpty();
            infoWindowText.ShouldNotContain("undefined");
            (infoWindowText.Contains("second", StringComparison.Ordinal) || infoWindowText.Contains("minute", StringComparison.Ordinal) || infoWindowText.Contains("hour", StringComparison.Ordinal)).ShouldBeTrue("Expected a duration in the info window text");
            (infoWindowText.Contains("km", StringComparison.Ordinal) || infoWindowText.Contains("miles", StringComparison.Ordinal)).ShouldBeTrue("Expected a distance in the info window text");
        });
    }

    [Theory]
    [MemberData(nameof(Browsers))]
    public async Task Can_Import_Data_And_View_Workouts(string browserType, string? browserChannel)
    {
        // Arrange
        var options = new BrowserFixtureOptions()
        {
            BrowserType = browserType,
            BrowserChannel = browserChannel,
        };

        using var fixture = new HttpWebApplicationFactory(OutputHelper);

        var browser = new BrowserFixture(options, OutputHelper);
        await browser.WithPageAsync(async (page) =>
        {
            await page.GotoAsync(fixture.ServerAddress);
            await page.WaitForLoadStateAsync();

            var app = new ApplicationPage(page);

            // Act
            await app.ImportDataAsync();

            // Assert
            await app.WaitForTracksAsync();
            await app.WaitForMapAsync();

            await app.IsMapDisplayedAsync().ShouldBeTrue();

            var tracks = await app.TracksAsync();

            tracks.Count.ShouldBe(2);

            await tracks[0].TitleAsync().ShouldBe("Route 1");
            await tracks[0].NameAsync().ShouldBe("Route 1");

            await tracks[1].TitleAsync().ShouldBe("Route 2 (Evening)");
            await tracks[1].NameAsync().ShouldBe("Route 2 (Evening)");

            var track = tracks[0];

            // Act
            await track.ExpandAsync();
            await app.TogglePolygonAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.StartedAtAsync().ShouldBeOneOf("May 4, 2021 11:25 AM", "May 4, 2021 12:25 PM");
            await track.EndedAtAsync().ShouldBeOneOf("May 4, 2021 11:45 AM", "May 4, 2021 12:45 PM");
            await track.DurationAsync().ShouldBe("20 minutes");

            await track.DistanceAsync().ShouldBe("1.31 km");
            await track.AveragePaceAsync().ShouldBe(@"14'59""/km");

            await app.TotalDistanceAsync().ShouldBe("3 km");
            await app.EmissionsAsync().ShouldBe("1");

            // Act
            await app.TogglePolygonAsync();
            await track.CollapseAsync();

            // Assert
            track = tracks[1];

            // Act
            await track.ExpandAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.StartedAtAsync().ShouldBeOneOf("May 5, 2021 11:25 AM", "May 5, 2021 12:25 PM");
            await track.EndedAtAsync().ShouldBeOneOf("May 5, 2021 11:45 AM", "May 5, 2021 12:45 PM");
            await track.DurationAsync().ShouldBe("20 minutes");

            await track.DistanceAsync().ShouldBe("1.31 km");
            await track.AveragePaceAsync().ShouldBe("14'59\"/km");

            // Act
            await track.CollapseAsync();

            // Act
            await app.ToggleUnitsAsync();

            // Assert
            tracks = await app.TracksAsync();
            tracks.ShouldNotBeEmpty();

            track = tracks[0];

            await track.ExpandAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.DistanceAsync().ShouldBe("0.81 miles");
            await track.AveragePaceAsync().ShouldBe("24'8\"/mile");

            await app.TotalDistanceAsync().ShouldBe("2 miles");
            await app.EmissionsAsync().ShouldBe("1");

            // Act
            await app.ToggleUnitsAsync();

            // Assert
            await app.WaitForTracksAsync();

            tracks = await app.TracksAsync();
            tracks.ShouldNotBeEmpty();

            track = tracks[0];

            await track.ExpandAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.DistanceAsync().ShouldBe("1.31 km");
            await track.AveragePaceAsync().ShouldBe("14'59\"/km");

            await app.TotalDistanceAsync().ShouldBe("3 km");
            await app.EmissionsAsync().ShouldBe("1");

            // Act
            await app.NotBeforeAsync(new(2021, 05, 05))
                     .ThenAsync((p) => p.NotAfterAsync(new(2021, 05, 05)))
                     .ThenAsync((p) => p.FilterAsync());

            // Assert
            await app.WaitForTracksAsync();

            tracks = await app.TracksAsync();
            track = tracks.ShouldHaveSingleItem();

            await track.TitleAsync().ShouldBe("Route 2 (Evening)");
            await track.NameAsync().ShouldBe("Route 2 (Evening)");
        });
    }

    public ValueTask InitializeAsync()
    {
        InstallPlaywright();
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(["install"]);

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
