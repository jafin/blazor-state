// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using BlazorState.EndToEnd.Tests.Infrastructure;
using BlazorState.EndToEnd.Tests.Infrastructure.ServerFixtures;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Xunit;
using Xunit.Abstractions;

namespace BlazorState.EndToEnd.Tests.ServerExecutionTests
{
  public class ServerSideAppTest : ServerTestBase<AspNetSiteServerFixture>
  {
    public ServerSideAppTest(
        BrowserFixture browserFixture,
        AspNetSiteServerFixture serverFixture,
        ITestOutputHelper output)
        : base(browserFixture, serverFixture, output)
    {
      _serverFixture.Environment = AspNetEnvironment.Development;
      _serverFixture.BuildWebHostMethod = BlazorState.Server.Program.BuildWebHost;

      Navigate("/", noReload: false);
      WaitUntilLoaded();
    }

    [Fact]
    public void HasCounterPage()
    {
      // Navigate to "Counter"
      Browser.FindElement(By.LinkText("Counter")).Click();
      WaitAssert.Equal("Counter", () => Browser.FindElement(By.TagName("h1")).Text);

      // Observe the initial value is zero
      IWebElement countDisplayElement = Browser.FindElement(By.CssSelector("h1 + p"));
      Assert.Equal("Current count: 3", countDisplayElement.Text);

      // Click the button; see it counts
      IWebElement button = Browser.FindElement(By.CssSelector(".main button"));
      button.Click();
      WaitAssert.Equal("Current count: 8", () => countDisplayElement.Text);
      button.Click();
      WaitAssert.Equal("Current count: 13", () => countDisplayElement.Text);
      button.Click();
      WaitAssert.Equal("Current count: 18", () => countDisplayElement.Text);
    }

    [Fact]
    public void HasFetchDataPage()
    {
      // Navigate to "Fetch Data"
      Browser.FindElement(By.LinkText("Fetch data")).Click();
      WaitAssert.Equal("Weather forecast", () => Browser.FindElement(By.TagName("h1")).Text);

      // Wait until loaded
      var tableSelector = By.CssSelector("table.table");
      new WebDriverWait(Browser, TimeSpan.FromSeconds(10)).Until(
          driver => driver.FindElement(tableSelector) != null);

      // Check the table is displayed correctly
      System.Collections.ObjectModel.ReadOnlyCollection<IWebElement> rows = Browser.FindElements(By.CssSelector("table.table tbody tr"));
      Assert.Equal(5, rows.Count);
      System.Collections.Generic.IEnumerable<IWebElement> cells = rows.SelectMany(row => row.FindElements(By.TagName("td")));
      foreach (IWebElement cell in cells)
      {
        Assert.True(!string.IsNullOrEmpty(cell.Text));
      }
    }

    [Fact]
    public void HasHeading()
    {
      Assert.Equal("Hello, world!", Browser.FindElement(By.TagName("h1")).Text);
    }

    [Fact]
    public void HasTitle()
    {
      Assert.Equal("Server Side Blazor", Browser.Title);
    }

    [Fact]
    public void NavMenuHighlightsCurrentLocation()
    {
      var activeNavLinksSelector = By.CssSelector(".sidebar a.active");
      var mainHeaderSelector = By.TagName("h1");

      // Verify we start at home, with the home link highlighted
      Assert.Equal("Hello, world!", Browser.FindElement(mainHeaderSelector).Text);
      Assert.Collection(Browser.FindElements(activeNavLinksSelector),
          item => Assert.Equal("Home", item.Text));

      // Click on the "counter" link
      Browser.FindElement(By.LinkText("Counter")).Click();

      // Verify we're now on the counter page, with that nav link (only) highlighted
      WaitAssert.Equal("Counter", () => Browser.FindElement(mainHeaderSelector).Text);
      Assert.Collection(Browser.FindElements(activeNavLinksSelector),
          item => Assert.Equal("Counter", item.Text));

      // Verify we can navigate back to home too
      Browser.FindElement(By.LinkText("Home")).Click();
      WaitAssert.Equal("Hello, world!", () => Browser.FindElement(mainHeaderSelector).Text);
      Assert.Collection(Browser.FindElements(activeNavLinksSelector),
          item => Assert.Equal("Home", item.Text));
    }

    private void WaitUntilLoaded()
    {
      new WebDriverWait(Browser, TimeSpan.FromSeconds(30)).Until(
          driver => driver.FindElement(By.TagName("app")).Text != "Loading...");
    }
  }
}