﻿// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.Extensions.Options;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services;

public sealed partial class TrackParser(
    IOptions<ApplicationOptions> options,
    ILogger<TrackParser> logger)
{
    private static readonly XNamespace XS = "http://www.topografix.com/GPX/1/1";

    private readonly ApplicationOptions _options = options.Value;

    public async Task<IList<Track>> GetTracksAsync(CancellationToken cancellationToken = default)
    {
        string path = _options.DataDirectory;

        Log.ParsingTrackData(logger, path);

        var result = new List<Track>();

        foreach (string fileName in Directory.EnumerateFiles(path, "*.gpx"))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var track = await TryLoadTrackAsync(fileName, cancellationToken);

            if (track is not null)
            {
                result.Add(track);
            }
        }

        result.Sort((x, y) => x.Timestamp.CompareTo(y.Timestamp));

        Log.ParsedTrackData(logger, result.Count, path);

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
        using var stream = File.OpenRead(fileName);

        XElement root;

        try
        {
            root = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.TrackXmlLoadFailed(logger, ex, fileName);
            return null;
        }

        var result = new Track();

        foreach (var track in root.Descendants(XS + "trk"))
        {
            result.Name = track.Descendants(XS + "name").FirstOrDefault()?.Value ?? string.Empty;

            foreach (var segmentNodes in track.Descendants(XS + "trkseg"))
            {
                foreach (var pointNode in segmentNodes.Descendants(XS + "trkpt"))
                {
                    string? longitudeString = pointNode.Attribute("lon")?.Value;
                    string? latitudeString = pointNode.Attribute("lat")?.Value;

                    if (!double.TryParse(longitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out double longitude))
                    {
                        Log.IgnoringInvalidLongitude(logger, longitudeString, fileName);
                        continue;
                    }

                    if (!double.TryParse(latitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out double latitude))
                    {
                        Log.IgnoringInvalidLatitude(logger, latitudeString, fileName);
                        continue;
                    }

                    if (!TryParseTimestamp(pointNode.Descendants(XS + "time").FirstOrDefault(), out var timestamp) ||
                        timestamp is null)
                    {
                        Log.IgnoringInvalidTimestamp(logger, fileName);
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

        result.Timestamp = result.Points[0].Timestamp;

        Log.AddedTrackPointsFromFile(
            logger,
            result.Points.Count,
            result.Name,
            result.Timestamp,
            fileName);

        return result;
    }

    private static partial class Log
    {
        [LoggerMessage(1, LogLevel.Information, "Parsing track data from {Path}.")]
        public static partial void ParsingTrackData(ILogger logger, string path);

        [LoggerMessage(2, LogLevel.Information, "Parsed {Count} tracks from {Path}.")]
        public static partial void ParsedTrackData(ILogger logger, int count, string path);

        [LoggerMessage(3, LogLevel.Error, "Failed to load track XML from {FileName}.")]
        public static partial void TrackXmlLoadFailed(ILogger logger, Exception exception, string fileName);

        [LoggerMessage(4, LogLevel.Warning, "Ignoring longitude value {Longitude} from segment point in {FileName}.")]
        public static partial void IgnoringInvalidLongitude(ILogger logger, string? longitude, string fileName);

        [LoggerMessage(5, LogLevel.Warning, "Ignoring longitude value {Latitude} from segment point in {FileName}.")]
        public static partial void IgnoringInvalidLatitude(ILogger logger, string? latitude, string fileName);

        [LoggerMessage(6, LogLevel.Warning, "Ignoring invalid timestamp value from segment point in {FileName}.")]
        public static partial void IgnoringInvalidTimestamp(ILogger logger, string fileName);

        [LoggerMessage(7, LogLevel.Debug, "Added {Count} point(s) for track {Name} with timestamp {Timestamp:u} from {FileName}.")]
        public static partial void AddedTrackPointsFromFile(ILogger logger, int count, string name, DateTimeOffset timestamp, string fileName);
    }
}
