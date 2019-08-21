using System;
using OpenQA.Selenium;
using SpecflowSeleniumFramework.SpecflowBindings;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework.Finalizers
{
    internal class TestFinalizer : BaseStepDefinition
    {
        private const string DriverDiedMessage = "The driver had died before the test ended. No debug information could be extracted.";

        public static void TearDown()
        {
            if (Driver == null)
            {
                WebDriverEventLog.Add(DriverDiedMessage);
                return;
            }

            WebDriverEventLog.Add(GetFinalUrlMessage());

            switch (WebDriverFactory.GetTestExecutionEnvironment())
            {
                case WebDriverFactory.TestExecutionEnvironment.Local:
                    LocalFinalizer.LocalTearDown(Driver);
                    break;
                case WebDriverFactory.TestExecutionEnvironment.Grid:
                    GridFinalizer.GridTearDown(Driver);
                    break;
                case WebDriverFactory.TestExecutionEnvironment.SauceLabs:
                    SaucelabsFinalizer.SauceLabsTearDown(Driver);
                    break;
                default:
                    throw new ArgumentException("Could not tear down the session for this execution environment");
            }
        }

        internal static TestStatus GetCurrentTestStatus()
        {
            return ScenarioContext.Current.TestError == null ? TestStatus.Passed : TestStatus.Failed;
        }

        internal enum TestStatus
        {
            Passed,
            Failed
        }

        internal static void WaitUntil(Func<object, bool> func, int timeoutSeconds = 60)
        {
            var startTime = DateTime.Now;

            while (!func.Invoke(null))
            {
                if (DateTime.Now.Subtract(startTime).TotalSeconds >= timeoutSeconds)
                {
                    throw new TimeoutException();
                }
                System.Threading.Thread.Sleep(250);
            }
        }

        private static string GetFinalUrlMessage()
        {
            try
            {
                return string.Format("Final URL at tear down: {0}", Driver.Url);
            }
            catch (UnhandledAlertException)
            {
                return "Could not obtain the final URL due to an unexpected Alert being present";
            }
            catch (InvalidOperationException)
            {
                return "Could not obtain the final URL as the driver has already been torn down.";
            }
            catch (Exception ex)
            {
                return String.Format("Could not obtain the final URL due to an unexpected exception: {0}", ex.Message);
            }
        }

        //private static void SauceLabsTearDown()
        //{
        //    var jobId = WebDriverFactory.SaucelabsJobId;
        //    var client = new Client(Configuration.Configuration.SauceLabsUsername, 
        //        Configuration.Configuration.SauceLabsAccessKey);

        //    // We need to quit the driver before finalising the sauce job, 
        //    // otherwise Sauce won't consider the session complete.
        //    QuitDriver();

        //    WaitUntilSauceJobComplete(client, jobId);

        //    client.SetJobPublic(jobId);

        //    if (GetTestStatus().Equals(TestStatus.Failed))
        //    {
        //        client.SetJobPassStatus(jobId, false);
        //        var job = client.GetJob(jobId);
        //        if (String.IsNullOrEmpty(job.Error))
        //        {
        //            client.SetError(jobId, ScenarioContext.Current.TestError.Message);
        //        }

        //        WebDriverEventLog.Add(job.ToDebugInfo());
        //    }
        //    else
        //    {
        //        client.SetJobPassStatus(jobId, true);

        //        WebDriverEventLog.Add(String.Format("Report: {0}{1}", "https://saucelabs.com/jobs/", jobId));
        //    }
        //}

        //private static void WaitUntilSauceJobComplete(Client client, string jobId)
        //{
        //    try
        //    {
        //        WaitUntil(d => client.IsJobComplete(jobId));
        //    }
        //    catch (TimeoutException)
        //    {
        //        WebDriverEventLog.Add(JobTimeoutMessage);
        //    }
        //}

        //private static void GridTearDown()
        //{
        //    try
        //    {
        //        if (GetTestStatus().Equals(TestStatus.Failed))
        //        {
        //            TakeScreenshot();
        //        }
        //    }
        //    finally
        //    {
        //        QuitDriver();
        //    }
        //}

        //private static void LocalTearDown()
        //{
        //    if (GetTestStatus().Equals(TestStatus.Failed))
        //    {
        //        TakeScreenshot();
        //    }

        //    QuitDriver();
        //}

        //private static void QuitDriver()
        //{
        //    if (Driver != null)
        //        Driver.Quit();
        //    Driver = null;
        //}

        //private static void TakeScreenshot()
        //{
        //    try
        //    {
        //        ScreenshotCreator.CreateErrorScreenshot(Driver);
        //    }
        //    catch (Exception ex)
        //    {
        //        WebDriverEventLog.Add(ex.Message);
        //    }
        //    try
        //    {
        //        Driver.Manage().Cookies.DeleteAllCookies();
        //    }
        //    catch (Exception)
        //    {
        //        //Suppressing. Uncontrollable race condition can cause a very rare but annoying failure here.
        //        WebDriverEventLog.Add("Could not delete cookies");
        //    }
        //}
    }
}
