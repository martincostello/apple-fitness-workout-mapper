// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using MartinCostello.AppleFitnessWorkoutMapper.Models;

namespace MartinCostello.AppleFitnessWorkoutMapper;

[ExcludeFromCodeCoverage]
[JsonSerializable(typeof(IList<Track>))]
[JsonSerializable(typeof(List<Track>))] // TODO A change in behaviour means that it now wants the concrete type
[JsonSerializable(typeof(TrackCount))]
internal sealed partial class ApplicationJsonSerializerContext : JsonSerializerContext
{
}
