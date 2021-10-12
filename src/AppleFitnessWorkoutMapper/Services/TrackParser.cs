// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;
using MartinCostello.AppleFitnessWorkoutMapper.Models;
using Microsoft.Extensions.Options;

namespace MartinCostello.AppleFitnessWorkoutMapper.Services;

public sealed partial class TrackParser
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

        Log.ParsingTrackData(_logger, path);

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

        Log.ParsedTrackData(_logger, result.Count, path);

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
            Log.TrackXmlLoadFailed(_logger, ex, fileName);
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

                    if (!double.TryParse(longitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out double longitude))
                    {
                        Log.IgnoringInvalidLongitude(_logger, longitudeString, fileName);
                        continue;
                    }

                    if (!double.TryParse(latitudeString, NumberStyles.Number, CultureInfo.InvariantCulture, out double latitude))
                    {
                        Log.IgnoringInvalidLatitude(_logger, latitudeString, fileName);
                        continue;
                    }

                    if (!TryParseTimestamp(pointNode.Descendants(XS + "time").FirstOrDefault(), out DateTimeOffset? timestamp))
                    {
                        Log.IgnoringInvalidTimestamp(_logger, fileName);
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

        Log.AddedTrackPointsFromFile(
            _logger,
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
