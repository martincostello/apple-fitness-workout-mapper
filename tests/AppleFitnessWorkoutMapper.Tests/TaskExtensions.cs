// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AppleFitnessWorkoutMapper;

internal static class TaskExtensions
{
    public static async Task<T> ThenAsync<T>(this Task<T> value, Func<T, Task<T>> continuation)
        => await continuation(await value);

    public static async Task ThenAsync<T>(this Task<T> value, Func<T, Task> continuation)
        => await continuation(await value);

    public static async Task ShouldBe(this Task<string> task, string expected)
    {
        string actual = await task;
        actual.ShouldBe(expected);
    }

    public static async Task ShouldBeOneOf<T>(this Task<T> task, params T[] expected)
    {
        var actual = await task;
        actual.ShouldBeOneOf(expected);
    }

    public static async Task ShouldBeTrue(this Task<bool> task)
    {
        bool actual = await task;
        actual.ShouldBeTrue();
    }
}
