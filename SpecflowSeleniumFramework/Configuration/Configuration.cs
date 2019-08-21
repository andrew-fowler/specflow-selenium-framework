using System;
using System.Configuration;

namespace SpecflowSeleniumFramework.Configuration
{
    class Configuration
    {
        public static WebDriverFactory.TestExecutionEnvironment TestExecutionEnvironment
        {
            get
            {
                var executionEnvironmentValue =
                    Environment.GetEnvironmentVariable("SELENIUM_EXECUTION_ENVIRONMENT");

                try
                {
                    return (WebDriverFactory.TestExecutionEnvironment)
                           Enum.Parse(typeof (WebDriverFactory.TestExecutionEnvironment), executionEnvironmentValue);
                }
                catch (Exception)
                {
                    throw new ConfigurationErrorsException(String.Format(
                        "Selenium execution environment value not recognised. {0}", executionEnvironmentValue));
                }
            }
        }

        public static string GridHubUrl
        {
            get { return Environment.GetEnvironmentVariable("GRID_HUB_URL"); }
        }

        public static string SauceLabsUsername
        {
            get { return Environment.GetEnvironmentVariable("SAUCELABS_USERNAME"); }
        }

        public static string SauceLabsParentName
        {
            get { return Environment.GetEnvironmentVariable("SAUCELABS_PARENT_NAME"); }
        }

        public static bool EnableLogging
        {
            get { throw new NotImplementedException(); }
        }

        public static string SauceLabsAccessKey
        {
            get { return Environment.GetEnvironmentVariable("SAUCELABS_ACCESS_KEY"); }
        }

        public static string SauceLabsHubUrl
        {
            get { return Environment.GetEnvironmentVariable("SAUCELABS_HUB_URL"); }
        }

        public static string SauceLabsTunnelName
        {
            get { return Environment.GetEnvironmentVariable("SAUCELABS_TUNNEL_NAME"); }
        }

        public static string GridIdentifier
        {
            get { return Environment.GetEnvironmentVariable("SELENIUM_GRID_IDENTIFIER"); }
        }

        public static double CommandTimeoutSec
        {
            get { return Convert.ToDouble(Environment.GetEnvironmentVariable("SELENIUM_COMMAND_TIMEOUT_SEC")); }
        }

        public static double ImplicitWaitSec
        {
            get { return Convert.ToDouble(Environment.GetEnvironmentVariable("SELENIUM_IMPLICIT_WAIT_TIMEOUT_SEC")); }
        }

        public static string BrowserVersion
        {
            get { return Environment.GetEnvironmentVariable("SELENIUM_BROWSER_VERISON"); }
        }

        public static string BrowserName
        {
            get { return Environment.GetEnvironmentVariable("SELENIUM_BROWSER_NAME"); }
        }

        public static string LocalFirefoxBinaryPath
        {
            get { return Environment.GetEnvironmentVariable("LOCAL_FIREFOX_BINARY_PATH"); }
        }

        public static string LocalIeDriverServerPath
        {
            get { return Environment.GetEnvironmentVariable("LOCAL_IEDRIVERSERVER_PATH"); }
        }

        public static string LocalChromeDriverPath
        {
            get { return Environment.GetEnvironmentVariable("LOCAL_CHROMEDRIVERSERVER_PATH"); }
        }
    }
}
