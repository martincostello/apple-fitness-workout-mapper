// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace MartinCostello.AppleFitnessWorkoutMapper;

internal sealed class HttpWebApplicationFactory : WebApplicationFactory
{
    public HttpWebApplicationFactory(ITestOutputHelper outputHelper)
        : base(outputHelper)
    {
        UseKestrel(0);
    }

    public string ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }
}
