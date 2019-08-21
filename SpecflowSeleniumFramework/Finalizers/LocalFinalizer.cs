
using OpenQA.Selenium;
using SpecflowSeleniumFramework.Other;

namespace SpecflowSeleniumFramework.Finalizers
{
    class LocalFinalizer
    {
        internal static void LocalTearDown(IWebDriver driver)
        {
            if (TestFinalizer.GetCurrentTestStatus().Equals(TestFinalizer.TestStatus.Failed))
            {
                ScreenshotCreator.CreateErrorScreenshot(driver);
            }

            if (driver != null)
                driver.Quit();
        }
    }
}
