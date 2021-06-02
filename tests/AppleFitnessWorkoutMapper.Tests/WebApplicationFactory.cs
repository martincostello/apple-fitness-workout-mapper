// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NodaTime;
using Xunit.Abstractions;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    internal class WebApplicationFactory : WebApplicationFactory<Startup>, ITestOutputHelperAccessor
    {
        public WebApplicationFactory(ITestOutputHelper outputHelper)
        {
            OutputHelper = outputHelper;
            AppDataDirectory = Path.Combine(Path.GetDirectoryName(GetType().Assembly.Location) !, "App_Data");
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
                   .UseContentRoot(GetApplicationContentRootPath());
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

        private string GetApplicationContentRootPath()
        {
            var attribute = GetTestAssemblies()
                .SelectMany((p) => p.GetCustomAttributes<WebApplicationFactoryContentRootAttribute>())
                .Where((p) => string.Equals(p.Key, "AppleFitnessWorkoutMapper", StringComparison.OrdinalIgnoreCase))
                .OrderBy((p) => p.Priority)
                .First();

            return attribute.ContentRootPath;
        }
    }
}
