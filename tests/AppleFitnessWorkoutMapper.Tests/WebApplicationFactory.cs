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
using Microsoft.Extensions.Logging;
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

        public void Reset()
        {
            try
            {
                File.Delete(Path.Combine(AppDataDirectory, "tracks.db"));
            }
            catch (Exception)
            {
                // Ignore
            }
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((p) => p.AddInMemoryCollection(new[] { KeyValuePair.Create("DataDirectory", AppDataDirectory) }))
                   .ConfigureLogging((p) => p.AddXUnit(this))
                   .UseContentRoot(GetApplicationContentRootPath());
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
