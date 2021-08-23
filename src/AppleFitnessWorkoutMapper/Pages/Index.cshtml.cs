// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using NodaTime;

namespace MartinCostello.AppleFitnessWorkoutMapper.Pages;

public class IndexModel : PageModel
{
    public IndexModel(IClock clock, IOptions<ApplicationOptions> options)
    {
        Clock = clock;
        GoogleMapsApiKey = options.Value.GoogleMapsApiKey;
    }

    public IClock Clock { get; }

    public string GoogleMapsApiKey { get; }

    public void OnGet()
    {
    }
}
