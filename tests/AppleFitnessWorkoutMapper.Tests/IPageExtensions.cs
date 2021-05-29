// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    internal static class IPageExtensions
    {
        public static async Task ClearTextAsync(this IPage page, string selector)
        {
            await page.FocusAsync(selector);
            await page.Keyboard.PressAsync(OperatingSystem.IsMacOS() ? "Meta+A" : "Control+A");
            await page.Keyboard.PressAsync("Delete");
        }

        public static async Task<string> InnerTextTrimmedAsync(this IPage page, string selector)
        {
            string text = await page.InnerTextAsync(selector);
            return text.Trim();
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
