// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    internal static class TaskExtensions
    {
        public static async Task<T> ThenAsync<T>(this Task<T> value, Func<T, Task<T>> continuation)
        {
            return await continuation(await value);
        }

        public static async Task ThenAsync<T>(this Task<T> value, Func<T, Task> continuation)
        {
            await continuation(await value);
        }
    }
}
