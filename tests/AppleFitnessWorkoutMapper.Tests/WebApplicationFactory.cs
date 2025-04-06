// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;

namespace MartinCostello.AppleFitnessWorkoutMapper;

internal class WebApplicationFactory(ITestOutputHelper outputHelper) : WebApplicationFactory<ApplicationOptions>, ITestOutputHelperAccessor
{
    public string AppDataDirectory { get; } = Path.Combine(Path.GetDirectoryName(typeof(WebApplicationFactory).Assembly.Location)!, "App_Data");

    public ITestOutputHelper? OutputHelper { get; set; } = outputHelper;

    private string DatabaseFileName { get; } = $"{Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var config = new[]
        {
            KeyValuePair.Create<string, string?>("DatabaseFileName", DatabaseFileName),
            KeyValuePair.Create<string, string?>("DataDirectory", AppDataDirectory),
        };

        var utcNow = new DateTimeOffset(2021, 06, 01, 12, 34, 56, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(utcNow);

        builder.ConfigureAppConfiguration((p) => p.AddInMemoryCollection(config))
               .ConfigureLogging((p) => p.AddXUnit(this))
               .ConfigureServices((p) => p.AddSingleton<TimeProvider>(timeProvider))
               .UseSolutionRelativeContentRoot(Path.Combine("src", "AppleFitnessWorkoutMapper"), "*.slnx");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        try
        {
            File.Delete(Path.Combine(AppDataDirectory, DatabaseFileName));
        }
        catch (Exception)
        {
            // Ignore
        }
    }
}
