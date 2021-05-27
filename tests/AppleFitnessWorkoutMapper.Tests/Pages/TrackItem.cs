// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Microsoft.Playwright;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages
{
    public sealed class TrackItem
    {
        private readonly IElementHandle _element;

        public TrackItem(IElementHandle element)
        {
            _element = element;
        }

        public async Task ExpandAsync()
        {
            await _element.ClickAsync();

            var child = await _element.QuerySelectorAsync("[data-js-visible]");
            await child.WaitForElementStateAsync(ElementState.Visible);
        }

        public async Task CollapseAsync()
            => await _element.ClickAsync();

        public async Task<string> LinkTextAsync()
            => await GetTextAsync("a");

        public async Task<string> NameAsync()
        {
            IElementHandle child = await _element.QuerySelectorAsync("[data-track-name]");
            return await child.GetAttributeAsync("data-track-name");
        }

        public async Task<string> StartedAtAsync()
            => await GetTextAsync("[data-js-start]");

        public async Task<string> EndedAtAsync()
            => await GetTextAsync("[data-js-end]");

        public async Task<string> DurationAsync()
            => await GetTextAsync("[data-js-duration]");

        public async Task<string> DistanceAsync()
            => await GetTextAsync("[data-js-distance]");

        public async Task<string> AveragePaceAsync()
            => await GetTextAsync("[data-js-pace]");

        private async Task<string> GetTextAsync(string selector)
        {
            IElementHandle child = await _element.QuerySelectorAsync(selector);

            string text = await child.InnerTextAsync();

            return text.Trim();
        }
    }
}
