// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public sealed class TrackLoader
    {
        private static readonly XNamespace XS = "http://www.topografix.com/GPX/1/1";

        private readonly IMemoryCache _cache;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger _logger;

        public TrackLoader(
            IMemoryCache cache,
            IWebHostEnvironment environment,
            ILogger<TrackLoader> logger)
        {
            _cache = cache;
            _environment = environment;
            _logger = logger;
        }

        public async Task<IList<Track>> GetTracksAsync(
            DateTimeOffset? since = null,
            CancellationToken cancellationToken = default)
        {
            DateTimeOffset notBefore = since ?? DateTimeOffset.MinValue;

            if (!_cache.TryGetValue<IList<Track>>(notBefore, out var tracks))
            {
                tracks = _cache.Set(notBefore, await GetTracksCachedAsync(since, cancellationToken));
            }

            return tracks;
        }

        private static bool TryParseTimestamp(XElement? element, [NotNullWhen(true)] out DateTimeOffset? value)
        {
            value = null;

            if (element is null)
            {
                return false;
            }

            if (!DateTimeOffset.TryParse(element.Value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var timestamp))
            {
                return false;
            }

            value = timestamp;
            return true;
        }

        private async Task<IList<Track>> GetTracksCachedAsync(
            DateTimeOffset? since = null,
            CancellationToken cancellationToken = default)
        {
            DateTimeOffset notBefore = since ?? DateTimeOffset.MinValue;

            string path = Path.Combine(_environment.ContentRootPath, "App_Data");

            var result = new List<Track>();

            foreach (string fileName in Directory.EnumerateFiles(path, "*.gpx"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                Track? track = await TryLoadTrackAsync(fileName, notBefore, cancellationToken);

                if (track is not null)
                {
                    result.Add(track);
                }
            }

            result.Sort((x, y) => Comparer<DateTimeOffset>.Default.Compare(x.Timestamp, y.Timestamp));

            return result;
        }

        private async Task<Track?> TryLoadTrackAsync(
            string fileName,
            DateTimeOffset notBefore,
            CancellationToken cancellationToken)
        {
            using Stream stream = File.OpenRead(fileName);

            XElement root;

            try
            {
                root = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load track XML from {FileName}.", fileName);
                return null;
            }

            var result = new Track();

            foreach (XElement track in root.Descendants(XS + "trk"))
            {
                result.Name = track.Descendants(XS + "name").FirstOrDefault()?.Value ?? string.Empty;

                foreach (XElement segmentNodes in track.Descendants(XS + "trkseg"))
                {
                    var segment = new List<TrackPoint>();

                    foreach (XElement pointNode in segmentNodes.Descendants(XS + "trkpt"))
                    {
                        string? longitudeString = pointNode.Attribute("lon")?.Value;
                        string? latitudeString = pointNode.Attribute("lat")?.Value;

                        if (!double.TryParse(longitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out var longitude))
                        {
                            _logger.LogWarning("Ignoring longitude value {Longitude} from segment point in {FileName}.", longitudeString, fileName);
                            continue;
                        }

                        if (!double.TryParse(latitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out var latitude))
                        {
                            _logger.LogWarning("Ignoring latitude value {Latitude} from segment point in {FileName}.", latitudeString, fileName);
                            continue;
                        }

                        if (!TryParseTimestamp(pointNode.Descendants(XS + "time").FirstOrDefault(), out DateTimeOffset? timestamp))
                        {
                            _logger.LogWarning("Ignoring invalid timestamp value from segment point in {FileName}.", fileName);
                            continue;
                        }

                        if (timestamp.Value < notBefore)
                        {
                            return null;
                        }

                        var point = new TrackPoint()
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Timestamp = timestamp.Value,
                        };

                        segment.Add(point);
                    }

                    if (segment.Count > 0)
                    {
                        result.Segments.Add(segment);

                        _logger.LogDebug(
                            "Added {PointCount} point(s) to segment {SegmentIndex} in {FileName}.",
                            result.Segments[^1].Count,
                            result.Segments.Count - 1,
                            fileName);
                    }
                }
            }

            if (result.Segments.Count < 1)
            {
                return null;
            }

            result.Timestamp = result.Segments.First().First().Timestamp;

            _logger.LogDebug(
                "Added {SegmentCount} segment(s) for track with timestamp {Timestamp:u} from {FileName}.",
                result.Segments.Count,
                result.Timestamp,
                fileName);

            return result;
        }
    }
}
