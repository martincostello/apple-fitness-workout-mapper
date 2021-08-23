// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AppleFitnessWorkoutMapper;

public sealed class ApplicationOptions
{
    public string DatabaseFileName { get; set; } = "tracks.db";

    public string DataDirectory { get; set; } = string.Empty;

    public string GoogleMapsApiKey { get; set; } = string.Empty;

    internal string DatabaseFile { get; set; } = string.Empty;
}
