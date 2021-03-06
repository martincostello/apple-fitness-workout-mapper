// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace MartinCostello.AppleFitnessWorkoutMapper;

internal class WebApplicationFactory : WebApplicationFactory<ApplicationOptions>, ITestOutputHelperAccessor
{
    public WebApplicationFactory(ITestOutputHelper outputHelper)
    {
        OutputHelper = outputHelper;
        AppDataDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location)!, "App_Data");
    }

    public string AppDataDirectory { get; }

    public ITestOutputHelper? OutputHelper { get; set; }

    private string DatabaseFileName { get; } = Guid.NewGuid().ToString() + ".db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var config = new[]
        {
            KeyValuePair.Create("DatabaseFileName", DatabaseFileName),
            KeyValuePair.Create("DataDirectory", AppDataDirectory),
        };

        var utcNow = Instant.FromUtc(2021, 06, 01, 12, 34, 56);

        builder.ConfigureAppConfiguration((p) => p.AddInMemoryCollection(config))
               .ConfigureLogging((p) => p.AddXUnit(this))
               .ConfigureServices((p) => p.AddSingleton<IClock>((_) => new NodaTime.Testing.FakeClock(utcNow)))
               .UseSolutionRelativeContentRoot(Path.Combine("src", "AppleFitnessWorkoutMapper"));
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
