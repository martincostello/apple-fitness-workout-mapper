// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.Extensions.Options;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services
{
    public sealed class TrackParser
    {
        private static readonly XNamespace XS = "http://www.topografix.com/GPX/1/1";

        private readonly ApplicationOptions _options;
        private readonly ILogger _logger;

        public TrackParser(
            IOptions<ApplicationOptions> options,
            ILogger<TrackParser> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task<IList<Track>> GetTracksAsync(CancellationToken cancellationToken = default)
        {
            string path = _options.DataDirectory;

            _logger.LogInformation("Parsing track data from {Path}.", path);

            var result = new List<Track>();

            foreach (string fileName in Directory.EnumerateFiles(path, "*.gpx"))
            {
                cancellationToken.ThrowIfCancellationRequested();

                Track? track = await TryLoadTrackAsync(fileName, cancellationToken);

                if (track is not null)
                {
                    result.Add(track);
                }
            }

            var comparer = Comparer<DateTimeOffset>.Default;

            result.Sort((x, y) => comparer.Compare(x.Timestamp, y.Timestamp));

            _logger.LogInformation("Parsed {Count} tracks from {Path}.", result.Count, path);

            return result;
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

        private async Task<Track?> TryLoadTrackAsync(
            string fileName,
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

                        var point = new TrackPoint()
                        {
                            Latitude = latitude,
                            Longitude = longitude,
                            Timestamp = timestamp.Value,
                        };

                        result.Points.Add(point);
                    }
                }
            }

            if (result.Points.Count < 1)
            {
                return null;
            }

            result.Timestamp = result.Points.First().Timestamp;

            _logger.LogDebug(
                "Added {Count} point(s) for track {Name} with timestamp {Timestamp:u} from {FileName}.",
                result.Points.Count,
                result.Name,
                result.Timestamp,
                fileName);

            return result;
        }
    }
}
