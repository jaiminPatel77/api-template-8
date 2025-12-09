//-----------------------------------------------------------------------------
// <copyright file="LoggingStartupExtensions.cs" company="Neudesic">
// Copyright 2016 Neudesic, LLC
// </copyright>
//-----------------------------------------------------------------------------

using Microsoft.Extensions.Logging;
//using Neuronms.Data.Services.Log4Net;

namespace CSCoreEFTemplate8.StartupExtensions
{
    internal static class LoggingStartupExtensions
    {
        internal static void ConfigureLogging(this Startup startup, ILoggerFactory loggerFactory)
        {
            //loggerFactory.AddConsole(startup.Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();
            //loggerFactory.AddLog4Net();
        }
    }
}
