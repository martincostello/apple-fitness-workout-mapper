// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.IO;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MartinCostello.AppleFitnessWorkerMapper.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace MartinCostello.AppleFitnessWorkerMapper
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
        public async Task Can_Get_All_Track_Points()
        {
            // Arrange
            using var fixture = new WebApplicationFactory();
            using var client = fixture.CreateClient();

            // Act
            IList<Track>? actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks");

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(2);
        }

        [Fact]
        public async Task Can_Get_Filtered_Track_Points()
        {
            // Arrange
            using var fixture = new WebApplicationFactory();
            using var client = fixture.CreateClient();

            // Act
            IList<Track>? actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks?notBefore=2021-05-05T00:00:00Z");

            // Assert
            actual.ShouldNotBeNull();
            actual.ShouldNotBeEmpty();
            actual.Count.ShouldBe(1);
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
