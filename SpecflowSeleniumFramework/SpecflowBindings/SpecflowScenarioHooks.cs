using System;
using SpecflowSeleniumFramework.Contexts;
using SpecflowSeleniumFramework.Finalizers;
using SpecflowSeleniumFramework.Other;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework.SpecflowBindings
{
    class SpecflowScenarioHooks : BaseStepDefinition
    {

        private const string DriverDiedMessage = "The driver had died before the test ended. No debug information could be extracted.";

        [BeforeTestRun]
        private static void BeforeTestRun()
        {

        }

        [BeforeScenario]
        private void BeforeScenario()
        {
            // Initialise our contexts to share information between step implementations, and hooks
            ScenarioContext.Current[SharedContextName] = new SharedContext();

            Driver = WebDriverFactory.Get();
            SharedContext.InitialWindowHandle = Driver.CurrentWindowHandle;
        }

        [AfterScenario]
        public void AfterScenario()
        {
            try
            {
                TestFinalizer.TearDown();
            }
            finally
            {
                Console.WriteLine(WebDriverEventLog.ToString());
            }
        }
    }
}
