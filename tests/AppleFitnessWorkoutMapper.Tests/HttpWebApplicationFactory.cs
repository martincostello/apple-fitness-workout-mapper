﻿// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.AppleFitnessWorkoutMapper;

internal sealed class HttpWebApplicationFactory(ITestOutputHelper outputHelper) : WebApplicationFactory(outputHelper)
{
    private IHost? _host;
    private bool _disposed;

    public string ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }

    public override IServiceProvider Services
    {
        get
        {
            StartServer();
            return _host!.Services!;
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        // Configure the server address for the server to
        // listen on for HTTP requests on a dynamic port.
        builder.UseUrls("http://127.0.0.1:0");
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var testHost = builder.Build();

        builder.ConfigureWebHost((p) => p.UseKestrel());

        _host = builder.Build();
        _host.Start();

        var server = _host.Services.GetRequiredService<IServer>();
        var addresses = server.Features.Get<IServerAddressesFeature>();

        ClientOptions.BaseAddress = addresses!.Addresses
            .Select((p) => new Uri(p))
            .Last();

        return testHost;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (!_disposed)
        {
            if (disposing)
            {
                _host?.Dispose();
            }

            _disposed = true;
        }
    }

    private void StartServer()
    {
        if (_host is null)
        {
            using (CreateDefaultClient())
            {
            }
        }
    }
}
