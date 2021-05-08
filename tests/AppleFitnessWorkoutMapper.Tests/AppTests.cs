// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public class AppTests
    {
        private readonly ITestOutputHelper _outputHelper;

        public AppTests(ITestOutputHelper outputHelper)
        {
            _outputHelper = outputHelper;
        }

        [Fact]
        public async Task Can_Load_Homepage()
        {
            // Arrange
            using var fixture = new WebApplicationFactory();
            using var client = fixture.CreateClient();

            // Act
            string actual = await client.GetStringAsync("/");

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.ShouldContain("<html");
        }

        [Fact]
        public async Task Can_Import_Tracks_And_Query()
        {
            // Arrange
            using var fixture = new WebApplicationFactory();
            using var client = fixture.CreateClient();

            // Act
            using var response = await client.PostAsJsonAsync("/api/tracks/import", new { });

            // Assert
            response.StatusCode.ShouldBe(HttpStatusCode.Created);

            using var importResponse = await response.Content.ReadFromJsonAsync<JsonDocument>();

            importResponse.ShouldNotBeNull();
            importResponse.RootElement.TryGetProperty("count", out var element).ShouldBeTrue();
            element.TryGetInt64(out long count).ShouldBeTrue();
            count.ShouldBe(2);

            // Act
            IList<Track>? actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks");

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(2);

            // Act
            actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks?notBefore=2021-05-05T00:00:00Z");

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(1);

            Track item = actual.ShouldHaveSingleItem();
            item.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));
        }

        private sealed class WebApplicationFactory : WebApplicationFactory<Startup>
        {
            protected override void ConfigureWebHost(IWebHostBuilder builder)
            {
                string? thisDirectory = Path.GetDirectoryName(GetType().Assembly.Location);
                builder.UseContentRoot(thisDirectory!);
            }
        }
    }
}
