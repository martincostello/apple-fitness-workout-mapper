// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkoutMapper.Pages;
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

        [Fact]
        public async Task Can_Import_Data_And_View_Workouts()
        {
            // Arrange
            using var fixture = new HttpWebApplicationFactory(OutputHelper);
            await fixture.InitializeAsync();

            using var driver = WebDriverFactory.CreateWebDriver();
            driver.Navigate().GoToUrl(fixture.ServerAddress);

            var page = new ApplicationPage(driver);

            // Act
            page.ImportData();

            // Assert
            page.IsMapDisplayed().ShouldBeTrue();

            IReadOnlyList<TrackItem> tracks = page.Tracks();

            tracks.Count.ShouldBe(2);

            tracks[0].LinkText().ShouldBe("Route 1");
            tracks[0].Name().ShouldBe("Route 1");

            tracks[1].LinkText().ShouldBe("Route 2");
            tracks[1].Name().ShouldBe("Route 2");

            TrackItem track = tracks[0];

            // Act
            track.Expand();
            page.ShowPolygon();

            // Assert
            track.StartedAt().ShouldBe("May 4, 2021 12:25 PM");
            track.EndedAt().ShouldBe("May 4, 2021 12:45 PM");
            track.Duration().ShouldBe("20 minutes");

            track.Distance().ShouldBe("1.31 km");
            track.AveragePace().ShouldBe(@"14'59""/km");

            page.TotalDistance().ShouldBe("3 km");

            // Act
            page.HidePolygon();
            track.Collapse();

            // Act
            page.UseMiles();

            // Assert
            page.WaitForReload();

            tracks = page.Tracks();
            track = tracks[0];

            track.Expand();

            // Assert
            track.Distance().ShouldBe("0.81 miles");
            track.AveragePace().ShouldBe(@"24'8""/mile");
            page.TotalDistance().ShouldBe("2 miles");

            // Act
            page.UseKilometers();

            // Assert
            page.WaitForReload();

            tracks = page.Tracks();
            track = tracks[0];

            track.Expand();

            // Assert
            track.Distance().ShouldBe("1.31 km");
            track.AveragePace().ShouldBe(@"14'59""/km");
            page.TotalDistance().ShouldBe("3 km");

            // Act
            page.NotBefore("2021-05-05")
                .NotAfter("2021-05-05")
                .Filter();

            // Assert
            tracks = page.Tracks();
            track = tracks.ShouldHaveSingleItem();

            track.LinkText().ShouldBe("Route 2");
            track.Name().ShouldBe("Route 2");
        }
    }
}
