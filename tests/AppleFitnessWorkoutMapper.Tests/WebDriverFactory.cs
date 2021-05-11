// Copyright (c) Martin Costello, 2021. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System;
using System.IO;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace MartinCostello.AppleFitnessWorkoutMapper
{
    internal static class WebDriverFactory
    {
        public static IWebDriver CreateWebDriver()
        {
            string chromeDriverDirectory = Path.GetDirectoryName(typeof(WebDriverFactory).Assembly.Location) ?? ".";

            var options = new ChromeOptions()
            {
                AcceptInsecureCertificates = true,
            };

            if (!System.Diagnostics.Debugger.IsAttached)
            {
                options.AddArgument("--headless");
            }
            else
            {
                options.AddArgument("--auto-open-devtools-for-tabs");
            }

            options.AddArgument("--lang=en-GB");
            options.AddUserProfilePreference("intl.accept_languages", "en-GB");

            options.SetLoggingPreference(LogType.Browser, LogLevel.All);

#if DEBUG
            options.SetLoggingPreference(LogType.Client, LogLevel.All);
            options.SetLoggingPreference(LogType.Driver, LogLevel.All);
            options.SetLoggingPreference(LogType.Profiler, LogLevel.All);
            options.SetLoggingPreference(LogType.Server, LogLevel.All);
#endif

            var driver = new ChromeDriver(chromeDriverDirectory, options, TimeSpan.FromSeconds(20));

            try
            {
                var manage = driver.Manage();

                var timeout = TimeSpan.FromSeconds(15);
                var timeouts = manage.Timeouts();

                timeouts.AsynchronousJavaScript = timeout;
                timeouts.ImplicitWait = timeout;
                timeouts.PageLoad = timeout;

                manage.Window.Maximize();
            }
            catch (Exception)
            {
                driver.Dispose();
                throw;
            }

            return driver;
        }
    }
}
