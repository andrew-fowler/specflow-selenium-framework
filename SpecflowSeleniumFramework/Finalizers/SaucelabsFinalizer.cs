
using System;
using OpenQA.Selenium;
using SaucelabsApiDotNet;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework.Finalizers
{
    class SaucelabsFinalizer
    {
        internal static void SauceLabsTearDown(IWebDriver driver)
        {
            var jobId = WebDriverFactory.SaucelabsJobId;
            var client = new Client(Configuration.Configuration.SauceLabsUsername,
                Configuration.Configuration.SauceLabsAccessKey);

            // We need to quit the driver before finalising the sauce job, 
            // otherwise Sauce won't consider the session complete.
            if (driver != null)
                driver.Quit();

            WaitUntilSauceJobComplete(client, jobId);

            client.SetJobPublic(jobId);

            if (TestFinalizer.GetCurrentTestStatus().Equals(TestFinalizer.TestStatus.Failed))
            {
                client.SetJobPassStatus(jobId, false);
                var job = client.GetJob(jobId);
                if (String.IsNullOrEmpty(job.Error))
                {
                    client.SetError(jobId, ScenarioContext.Current.TestError.Message);
                }

                WebDriverEventLog.Add(job.ToDebugInfo());
            }
            else
            {
                client.SetJobPassStatus(jobId, true);

                WebDriverEventLog.Add(String.Format("Report: {0}{1}", "https://saucelabs.com/jobs/", jobId));
            }
        }

        private static void WaitUntilSauceJobComplete(Client client, string jobId)
        {
            try
            {
                TestFinalizer.WaitUntil(d => client.IsJobComplete(jobId));
            }
            catch (TimeoutException)
            {
                const string jobTimeoutMessage = "Timed out waiting for the Saucelabs job to finish. " +
                                                 "This normally means that the connection has been lost.";

                WebDriverEventLog.Add(jobTimeoutMessage);
            }
        }

    }
}
