// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public static class GitMetadata
    {
        public static string Branch { get; } = GetMetadataValue("CommitBranch", "Unknown");

        public static string Commit { get; } = GetMetadataValue("CommitHash", "HEAD");

        public static string BuildId { get; } = GetMetadataValue("BuildId", "Unknown");

        public static DateTime Timestamp { get; } = DateTime.Parse(GetMetadataValue("BuildTimestamp", DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture)), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);

        private static string GetMetadataValue(string name, string defaultValue)
        {
            return typeof(GitMetadata).Assembly
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .Where((p) => string.Equals(p.Key, name, StringComparison.Ordinal))
                .Select((p) => p.Value)
                .FirstOrDefault() ?? defaultValue;
        }
    }
}
