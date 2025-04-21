// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AppleFitnessWorkoutMapper;

/// <summary>
/// A class representing the model for the application. This class cannot be inherited.
/// </summary>
public sealed class AppModel
{
    /// <summary>
    /// Gets the Google Maps API key to use.
    /// </summary>
    public required string GoogleMapsApiKey { get; init; }

    /// <summary>
    /// Gets the start date for the workouts to display.
    /// </summary>
    public required string StartDate { get; init; }

    /// <summary>
    /// Gets the end date for the workouts to display.
    /// </summary>
    public required string EndDate { get; init; }

    /// <summary>
    /// Gets the date for today.
    /// </summary>
    public required string TodayDate { get; init; }
}
