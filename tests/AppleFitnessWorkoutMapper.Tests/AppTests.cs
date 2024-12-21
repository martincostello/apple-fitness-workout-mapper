// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Net.Http.Json;
using MartinCostello.AppleFitnessWorkoutMapper.Models;

namespace MartinCostello.AppleFitnessWorkoutMapper;

public class AppTests(ITestOutputHelper outputHelper)
{
    [Fact]
    public async Task Can_Load_Homepage()
    {
        // Arrange
        using var fixture = new WebApplicationFactory(outputHelper);
        using var client = fixture.CreateClient();

        // Act
        string actual = await client.GetStringAsync("/", TestContext.Current.CancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldNotBeEmpty();
        actual.ShouldContain("<html");
    }

    [Fact]
    public async Task Can_Import_Tracks_And_Query()
    {
        // Arrange
        var cancellationToken = TestContext.Current.CancellationToken;
        using var fixture = new WebApplicationFactory(outputHelper);

        using var client = fixture.CreateClient();

        // Act
        var result = await client.GetFromJsonAsync<CountResponse>("/api/tracks/count", cancellationToken);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);

        // Act
        using var response = await client.PostAsJsonAsync("/api/tracks/import", new { }, cancellationToken);

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        result = await response.Content.ReadFromJsonAsync<CountResponse>(cancellationToken);

        result.ShouldNotBeNull();
        result.Count.ShouldBe(2);

        // Act
        var actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks", cancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldNotBeEmpty();
        actual.Count.ShouldBe(2);

        var item = actual[0];
        item.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 04, 11, 25, 35, TimeSpan.Zero));

        // Act
        actual = await client.GetFromJsonAsync<IList<Track>>("api/tracks?notBefore=2021-05-05T00:00:00Z", cancellationToken);

        // Assert
        actual.ShouldNotBeNull();
        actual.ShouldNotBeEmpty();
        actual.Count.ShouldBe(1);

        item = actual.ShouldHaveSingleItem();
        item.Timestamp.ShouldBe(new DateTimeOffset(2021, 05, 05, 11, 25, 35, TimeSpan.Zero));
    }

    private sealed class CountResponse
    {
        public int Count { get; set; }
    }
}
