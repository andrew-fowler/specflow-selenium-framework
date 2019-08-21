
using System;
using OpenQA.Selenium;
using SpecflowSeleniumFramework.Other;

namespace SpecflowSeleniumFramework.Finalizers
{
    class GridFinalizer
    {
        internal static void GridTearDown(IWebDriver driver)
        {
            try
            {
                if (TestFinalizer.GetCurrentTestStatus().Equals(TestFinalizer.TestStatus.Failed))
                {
                    TakeScreenshot(driver);
                }
            }
            finally
            {
                if (driver != null)
                    driver.Quit();
            }
        }

        private static void TakeScreenshot(IWebDriver driver)
        {
            try
            {
                ScreenshotCreator.CreateErrorScreenshot(driver);
            }
            catch (Exception ex)
            {
                WebDriverEventLog.Add(ex.Message);
            }
        }
    }
}
