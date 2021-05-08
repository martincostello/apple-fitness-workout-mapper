// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using MartinCostello.AppleFitnessWorkoutMapper.Data;
using MartinCostello.AppleFitnessWorkoutMapper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public class Startup
    {
        public Startup(IWebHostEnvironment environment)
        {
            Environment = environment;
        }

        private IWebHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            string databaseFileName = Path.Combine(Environment.ContentRootPath, "App_Data", "tracks.db");

            services.AddDbContext<TracksContext>((p) => p.UseSqlite("Data Source=" + databaseFileName));
            services.AddScoped<TrackImporter>();
            services.AddScoped<TrackService>();
            services.AddSingleton<TrackParser>();
            services.AddRazorPages();

            services.Configure<GzipCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);
            services.Configure<BrotliCompressionProviderOptions>((p) => p.Level = CompressionLevel.Fastest);

            services.AddResponseCompression((p) =>
            {
                p.EnableForHttps = true;
                p.Providers.Add<BrotliCompressionProvider>();
                p.Providers.Add<GzipCompressionProvider>();
            });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseResponseCompression();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints((endpoints) =>
            {
                endpoints.MapRazorPages();

                endpoints.MapGet("/api/tracks", async (context) =>
                {
                    DateTimeOffset? since = null;
                    DateTimeOffset? until = null;

                    StringValues noteBefore = context.Request.Query["notBefore"];

                    if (!StringValues.IsNullOrEmpty(noteBefore) &&
                        DateTimeOffset.TryParse(noteBefore, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var value))
                    {
                        since = value;
                    }

                    StringValues notAfter = context.Request.Query["notAfter"];

                    if (!StringValues.IsNullOrEmpty(notAfter) &&
                        DateTimeOffset.TryParse(notAfter, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out value))
                    {
                        until = value;
                    }

                    var service = context.RequestServices.GetRequiredService<TrackService>();

                    var tracks = await service.GetTracksAsync(since, until, context.RequestAborted);

                    await context.Response.WriteAsJsonAsync(tracks, context.RequestAborted);
                });

                endpoints.MapGet("/api/tracks/count", async (context) =>
                {
                    var service = context.RequestServices.GetRequiredService<TrackService>();

                    int count = await service.GetTrackCountAsync(context.RequestAborted);

                    var result = new
                    {
                        count,
                    };

                    await context.Response.WriteAsJsonAsync(result, context.RequestAborted);
                });

                endpoints.MapPost("/api/tracks/import", async (context) =>
                {
                    var importer = context.RequestServices.GetRequiredService<TrackImporter>();
                    int count = await importer.ImportTracksAsync(context.RequestAborted);

                    var result = new
                    {
                        count,
                    };

                    context.Response.StatusCode = StatusCodes.Status201Created;

                    await context.Response.WriteAsJsonAsync(result, context.RequestAborted);
                });
            });
        }
    }
}
