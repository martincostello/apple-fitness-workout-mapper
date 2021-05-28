// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkoutMapper.Pages;
using Microsoft.Playwright;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public class UITests
    {
        public UITests(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
        }

        public ITestOutputHelper OutputHelper { get; set; }

        [Theory]
        [InlineData("chromium")]
        [InlineData("firefox")]
        public async Task Can_Import_Data_And_View_Workouts(string browserType)
        {
            // Arrange
            using var fixture = new HttpWebApplicationFactory(OutputHelper);
            await fixture.InitializeAsync();

            using IPlaywright playwright = await Playwright.CreateAsync();

            await using IBrowser browser = await CreateBrowserAsync(playwright, browserType);

            var options = new BrowserNewPageOptions()
            {
                IgnoreHTTPSErrors = true,
                Locale = "en-GB",
                TimezoneId = "Europe/London",
            };

            IPage page = await browser.NewPageAsync(options);

            page.Console += (_, e) => OutputHelper.WriteLine(e.Text);
            page.PageError += (_, e) => OutputHelper.WriteLine(e);

            await page.GotoAsync(fixture.ServerAddress.ToString());

            var app = new ApplicationPage(page);

            try
            {
                // Act
                await app.ImportDataAsync();

                // Assert
                (await app.IsMapDisplayedAsync()).ShouldBeTrue();

                IReadOnlyList<TrackItem> tracks = await app.TracksAsync();

                tracks.Count.ShouldBe(2);

                (await tracks[0].LinkTextAsync()).ShouldBe("Route 1");
                (await tracks[0].NameAsync()).ShouldBe("Route 1");

                (await tracks[1].LinkTextAsync()).ShouldBe("Route 2");
                (await tracks[1].NameAsync()).ShouldBe("Route 2");

                TrackItem track = tracks[0];

                // Act
                await track.ExpandAsync();
                await app.ShowPolygonAsync();

                // Assert
                (await track.StartedAtAsync()).ShouldBeOneOf("May 4, 2021 11:25 AM", "May 4, 2021 12:25 PM");
                (await track.EndedAtAsync()).ShouldBeOneOf("May 4, 2021 11:45 AM", "May 4, 2021 12:45 PM");
                (await track.DurationAsync()).ShouldBe("20 minutes");

                (await track.DistanceAsync()).ShouldBe("1.31 km");
                (await track.AveragePaceAsync()).ShouldBe(@"14'59""/km");

                (await app.TotalDistanceAsync()).ShouldBe("3 km");
                (await app.EmissionsAsync()).ShouldBe("1");

                // Act
                await app.HidePolygonAsync();
                await track.CollapseAsync();

                // Act
                await app.UseMilesAsync();

                // Assert
                tracks = await app.TracksAsync();
                track = tracks[0];

                await track.ExpandAsync();

                // Assert
                (await track.DistanceAsync()).ShouldBe("0.81 miles");
                (await track.AveragePaceAsync()).ShouldBe(@"24'8""/mile");

                (await app.TotalDistanceAsync()).ShouldBe("2 miles");
                (await app.EmissionsAsync()).ShouldBe("1");

                // Act
                await app.UseKilometersAsync();

                // Assert
                tracks = await app.TracksAsync();
                track = tracks[0];

                await track.ExpandAsync();

                // Assert
                (await track.DistanceAsync()).ShouldBe("1.31 km");
                (await track.AveragePaceAsync()).ShouldBe(@"14'59""/km");

                (await app.TotalDistanceAsync()).ShouldBe("3 km");
                (await app.EmissionsAsync()).ShouldBe("1");

                // Act
                await app.NotBeforeAsync("2021-05-05")
                         .ThenAsync((p) => p.NotAfterAsync("2021-05-05"))
                         .ThenAsync((p) => p.FilterAsync());

                // Assert
                tracks = await app.TracksAsync();
                track = tracks.ShouldHaveSingleItem();

                (await track.LinkTextAsync()).ShouldBe("Route 2");
                (await track.NameAsync()).ShouldBe("Route 2");
            }
            catch (Exception)
            {
                string os =
                    OperatingSystem.IsLinux() ? "linux" :
                    OperatingSystem.IsMacOS() ? "macos" :
                    OperatingSystem.IsWindows() ? "windows" :
                    "other";

                string name = nameof(Can_Import_Data_And_View_Workouts);
                string utcNow = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss", CultureInfo.InvariantCulture);
                string path = Path.Combine("screenshots", $"{name}_{browserType}_{os}_{utcNow}.png");

                await page.ScreenshotAsync(new PageScreenshotOptions()
                {
                    Path = path,
                });

                OutputHelper.WriteLine($"Screenshot saved to {path}.");

                throw;
            }
        }

        private static async Task<IBrowser> CreateBrowserAsync(IPlaywright playwright, string browserType)
        {
            var options = new BrowserTypeLaunchOptions();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                options.Devtools = true;
                options.Headless = false;
                options.SlowMo = 100;
            }

            return await playwright[browserType].LaunchAsync(options);
        }
    }
}
