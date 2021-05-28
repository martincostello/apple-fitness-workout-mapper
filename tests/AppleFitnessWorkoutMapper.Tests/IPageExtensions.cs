// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    internal static class IPageExtensions
    {
        public static async Task EnterTextAsync(this IPage page, string selector, string value)
        {
            // Clear any existing value
            await page.FocusAsync(selector);
            await page.Keyboard.PressAsync(OperatingSystem.IsMacOS() ? "Meta+A" : "Control+A");
            await page.Keyboard.PressAsync("Delete");

            // Enter the new vaue
            await page.TypeAsync(selector, value);
        }

        public static async Task<IElementHandle> WaitUntilEnabledAsync(this IPage page, string selector)
        {
            IElementHandle element = await page.QuerySelectorAsync(selector);

            await element.WaitForElementStateAsync(ElementState.Enabled);

            return element;
        }

        public static async Task<IElementHandle> WaitUntilHiddenAsync(this IPage page, string selector)
        {
            IElementHandle element = await page.QuerySelectorAsync(selector);

            await element.WaitForElementStateAsync(ElementState.Hidden);

            return element;
        }
    }
}
