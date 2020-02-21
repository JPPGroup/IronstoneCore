using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;

namespace IronstoneCore.UI.UiTests
{
    [SetUpFixture]
    public class Setup
    {
        public static WindowsDriver<WindowsElement> CurrentDriver { get; set; }

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            AppiumOptions options = new AppiumOptions();
            options.AddAdditionalCapability("app", @"C:\Program Files\Autodesk\AutoCAD 2019\acad.exe");
            options.AddAdditionalCapability("appArguments", "/product ACAD /language \"en - US\"");
            options.AddAdditionalCapability("platformName", "Windows");
            options.AddAdditionalCapability("deviceName", "WindowsPC");

            CurrentDriver = new WindowsDriver<WindowsElement>(options);
            CurrentDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(5000);

            bool loading = true;
            while (loading)
            {
                try
                {
                    var newElement = CurrentDriver.FindElementByName("Start Drawing");
                    Actions a = new Actions(CurrentDriver);
                    a.MoveToElement(newElement);
                    a.DoubleClick();
                    a.Perform();
                    
                    loading = false;
                }
                catch (NoSuchElementException)
                {
                    Thread.Sleep(250);
                }
            }

            CurrentDriver.FindElementByName("Proxy Information").FindElementByName("OK").Click();
            var drawingWindow = CurrentDriver.FindElementByName("Drawing1.dwg");
            drawingWindow.SendKeys("fildia ");
            Thread.Sleep(250);
            drawingWindow.SendKeys("0 ");
            Thread.Sleep(250);
            drawingWindow.SendKeys("netload ");

            string backslash = Keys.Alt + Keys.NumberPad9 + Keys.NumberPad2 + Keys.Alt;

            string coreDllPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "IronstoneCore.dll ");

            coreDllPath = coreDllPath.Replace("\\", backslash);
            drawingWindow.SendKeys(coreDllPath);
            drawingWindow.SendKeys(Keys.Enter);
            Thread.Sleep(2000);
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            // TODO: Consider sending /forcequite when launching the driver
            // Return all window handles associated with this process/application.
            var allWindowHandles = CurrentDriver.WindowHandles;
            
            CurrentDriver.SwitchTo().Window(allWindowHandles.Last());
            CurrentDriver.Quit();
        }
	}
}
