using System;
using System.Drawing.Imaging;
using System.IO;
using OpenQA.Selenium;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework.Other
{
    public class ScreenshotCreator
    {
        // TODO: Pull to a general test constants file (and/or load from config)
        public const string ErrorScreenshotDirName = @"Screenshots\";

        /// <summary>
        /// This will take a full page screenshot of the current browser window and annotate the image with the
        /// error information.
        /// </summary>
        public static void CreateErrorScreenshot(IWebDriver driver)
        {
            var filename = MakeErrorScreenshotFilename();
            try
            {
                TakeScreenshot(driver, filename);
                filename = AppDomain.CurrentDomain.BaseDirectory + "\\" + ErrorScreenshotDirName + filename;

                if (String.IsNullOrEmpty(filename))
                {
                    return;
                }

                Console.WriteLine("{0}Error screenshot created: {0}{0}{1}{0}", Environment.NewLine, filename);
            }
            catch (InvalidOperationException)
            {
                Console.WriteLine("{0}Unfortunately we were unable to take a failure screenshot.  This is likely because the remote browser or node were disconnected.{0}", Environment.NewLine);
            }
        }

        /// <summary>
        /// This will take a screenshot of the currently rendered page and save it to the path set in the script.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="strFilename">The inputFilename to save the screenshot to, without extension.</param>
        public static string TakeScreenshot(IWebDriver driver, string strFilename)
        {
            if (!Directory.Exists(ErrorScreenshotDirName))
            {
                Directory.CreateDirectory(ErrorScreenshotDirName);
            }

            // TODO: Deal with .format expectedText
            //var filePath = ErrorScreenshotDirName + strFilename + "raw.png";
            string filePath = ErrorScreenshotDirName + strFilename;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            // Bug: This could fail due to System.IO.PathTooLongException. Need a more intelligent mechanism.
            ((ITakesScreenshot)driver).GetScreenshot().SaveAsFile(filePath, ImageFormat.Png);
            return filePath;
        }

        private static string MakeErrorScreenshotFilename()
        {
            // Reducing name length to avoid path length problems
            return DateTime.Now.ToString("hh-mm-ss-ffff") + ".png";
        }

        private static string GetScenarioErrorSummary(IWebDriver driver)
        {
            return "Scenario: " + ScenarioContext.Current.ScenarioInfo.Title + Environment.NewLine +
                   "Page: " + driver.Title + "( " + driver.Url + " )" + Environment.NewLine +
                   ScenarioContext.Current.TestError + Environment.NewLine;
        }
    }
}
