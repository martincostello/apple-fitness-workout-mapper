// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace MartinCostello.AppleFitnessWorkerMapper
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<RouteLoader>();
            services.AddRazorPages();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment environment)
        {
            if (environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapRazorPages();
                endpoints.MapGet("/api/tracks", async (context) =>
                {
                    DateTimeOffset? since = null;

                    StringValues noteBefore = context.Request.Query["notBefore"];

                    if (!StringValues.IsNullOrEmpty(noteBefore) &&
                        DateTimeOffset.TryParse(noteBefore, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var value))
                    {
                        since = value;
                    }

                    var loader = context.RequestServices.GetRequiredService<RouteLoader>();

                    var tracks = await loader.GetTracksAsync(since, context.RequestAborted);

                    await context.Response.WriteAsJsonAsync(tracks, context.RequestAborted);
                });
            });
        }
    }
}
