// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex) when (ex.InnerException is Microsoft.AspNetCore.Connections.AddressInUseException)
            {
                var oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkRed;

                Console.Error.WriteLine();
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.InnerException.Message);
                Console.Error.WriteLine();
                Console.Error.WriteLine("Close any other copies of AppleFitnessWorkoutMapper that might be running and try again.");
                Console.Error.WriteLine();

                Console.ForegroundColor = oldColor;

                return -1;
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults((builder) => builder.UseStartup<Startup>());
    }
}
