﻿// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.AppleFitnessWorkoutMapper.Pages;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper;

public class UITests : IAsyncLifetime
{
    public UITests(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
    }

    public ITestOutputHelper OutputHelper { get; set; }

    public static IEnumerable<object?[]> Browsers()
    {
        yield return new[] { BrowserType.Chromium, null };

        if (!OperatingSystem.IsWindows())
        {
            yield return new[] { BrowserType.Chromium, "chrome" };
        }

        if (!OperatingSystem.IsLinux())
        {
            yield return new[] { BrowserType.Chromium, "msedge" };
        }

        yield return new[] { BrowserType.Firefox, null };

        if (OperatingSystem.IsMacOS())
        {
            yield return new[] { BrowserType.Webkit, null };
        }
    }

    [Theory]
    [MemberData(nameof(Browsers))]
    public async Task Can_Import_Data_And_View_Workouts(string browserType, string browserChannel)
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

            await app.IsMapDisplayedAsync().ShouldBeTrue();

            IReadOnlyList<TrackItem> tracks = await app.TracksAsync();

            tracks.Count.ShouldBe(2);

            await tracks[0].LinkTextAsync().ShouldBe("Route 1");
            await tracks[0].NameAsync().ShouldBe("Route 1");

            await tracks[1].LinkTextAsync().ShouldBe("Route 2");
            await tracks[1].NameAsync().ShouldBe("Route 2");

            TrackItem track = tracks[0];

            // Act
            await track.ExpandAsync();
            await app.ShowPolygonAsync();

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
            await app.HidePolygonAsync();
            await track.CollapseAsync();

            // Act
            await app.UseMilesAsync();

            // Assert
            tracks = await app.TracksAsync();
            tracks.ShouldNotBeEmpty();

            track = tracks[0];

            await track.ExpandAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.DistanceAsync().ShouldBe("0.81 miles");
            await track.AveragePaceAsync().ShouldBe(@"24'8""/mile");

            await app.TotalDistanceAsync().ShouldBe("2 miles");
            await app.EmissionsAsync().ShouldBe("1");

            // Act
            await app.UseKilometersAsync();

            // Assert
            await app.WaitForTracksAsync();

            tracks = await app.TracksAsync();
            tracks.ShouldNotBeEmpty();

            track = tracks[0];

            await track.ExpandAsync();

            // Assert
            await app.WaitForTracksAsync();

            await track.DistanceAsync().ShouldBe("1.31 km");
            await track.AveragePaceAsync().ShouldBe(@"14'59""/km");

            await app.TotalDistanceAsync().ShouldBe("3 km");
            await app.EmissionsAsync().ShouldBe("1");

            // Act
            await app.NotBeforeAsync("2021-05-05")
                     .ThenAsync((p) => p.NotAfterAsync("2021-05-05"))
                     .ThenAsync((p) => p.FilterAsync());

            // Assert
            await app.WaitForTracksAsync();

            tracks = await app.TracksAsync();
            track = tracks.ShouldHaveSingleItem();

            await track.LinkTextAsync().ShouldBe("Route 2");
            await track.NameAsync().ShouldBe("Route 2");
        });
    }

    public Task InitializeAsync()
    {
        InstallPlaywright();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static void InstallPlaywright()
    {
        int exitCode = Microsoft.Playwright.Program.Main(new[] { "install" });

        if (exitCode != 0)
        {
            throw new InvalidOperationException($"Playwright exited with code {exitCode}");
        }
    }
}
