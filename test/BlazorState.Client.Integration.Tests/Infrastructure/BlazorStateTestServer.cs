﻿namespace BlazorState.Client.Integration.Tests.Infrastructure
{
  using BlazorState.Server;
  using Microsoft.AspNetCore;
  using Microsoft.AspNetCore.Hosting;
  using Microsoft.AspNetCore.TestHost;

  class BlazorStateTestServer : TestServer
  {
    public BlazorStateTestServer() : base(WebHostBuilder()) { }

    private static IWebHostBuilder WebHostBuilder() =>
      WebHost.CreateDefaultBuilder()
      .UseStartup<Startup>()
      .UseEnvironment("Local");
  }
}
