using System;
using System.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.Events;
using SpecflowSeleniumFramework.DriverWrappers;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework
{
    public static class WebDriverFactory
    {
        public static string SaucelabsJobId;
        public static BrowserType CurrentBrowserType;
        private const string EnvNotSetMessage = "Please specify an execution environment (local, grid or sauce) in App.config.";
        //private static readonly Configuration.Configuration Config = new Configuration.Configuration();

        internal static TestExecutionEnvironment GetTestExecutionEnvironment()
        {
            // If someone has specified a browser that only exists on Sauce, we assume they want a Sauce connection
            if (IsSauceLabsOnlyBrowser((BrowserType)Enum.Parse(typeof(BrowserType), Configuration.Configuration.BrowserName, true)))
                return TestExecutionEnvironment.SauceLabs;

            // If someone has specified a standard browser on local env, we assume they want a local instance
            if (Configuration.Configuration.TestExecutionEnvironment == TestExecutionEnvironment.Local)
                return TestExecutionEnvironment.Local;

            if (Configuration.Configuration.TestExecutionEnvironment == TestExecutionEnvironment.SauceLabs)
                return TestExecutionEnvironment.SauceLabs;

            if (Configuration.Configuration.TestExecutionEnvironment == TestExecutionEnvironment.Grid)
                return TestExecutionEnvironment.Grid;

            throw new ConfigurationErrorsException(String.Format("Unrecognised Execution Environment variable: {0}", 
                Configuration.Configuration.TestExecutionEnvironment));
        }

        // If one of these is set, there's no point trying to fire locally or across grid
        private static bool IsSauceLabsOnlyBrowser(BrowserType browserChoice)
        {
            switch (browserChoice)
            {
                case BrowserType.Android:
                case BrowserType.iPad:
                case BrowserType.iPhone:
                    return true;
                default:
                    return false;
            }
        }

        public static IWebDriver Get(string browser, string version)
        {
            CurrentBrowserType = (BrowserType)Enum.Parse(typeof(BrowserType), browser, true);
            var driver = GetDriverForSauceLabs(CurrentBrowserType, version);
            return Configuration.Configuration.EnableLogging ? AttachEventFiringWebDriver(driver) : driver;
        }

        /// <summary>
        /// Construct a driver appropriate for the current configuration.
        /// </summary>
        /// <returns>
        /// The relevantly configured IWebDriver implementation.
        /// </returns>
        public static IWebDriver Get()
        {
            IWebDriver driver;

            CurrentBrowserType = (BrowserType)Enum.Parse(typeof(BrowserType), Configuration.Configuration.BrowserName, true);
            var browserVersion = Configuration.Configuration.BrowserVersion;

            WebDriverEventLog.Add(String.Format("Specifying browser type: '{0}'", CurrentBrowserType));
            WebDriverEventLog.Add(String.Format("Specifying browser version: '{0}'", browserVersion));

            switch (GetTestExecutionEnvironment())
            {
                case TestExecutionEnvironment.Local:
                    driver = GetDriverForLocalEnvironment(CurrentBrowserType);
                    break;
                case TestExecutionEnvironment.SauceLabs:
                    driver = GetDriverForSauceLabs(CurrentBrowserType, browserVersion);
                    break;
                case TestExecutionEnvironment.Grid:
                    driver = GetDriverForGrid(CurrentBrowserType);
                    break;
                default:
                    throw new ArgumentException("Test execution environment not recognised.");
            }

            if (Configuration.Configuration.EnableLogging)
                driver = AttachEventFiringWebDriver(driver);

            if (GetTestExecutionEnvironment() != TestExecutionEnvironment.SauceLabs
                && BrowserSupportsResizing(CurrentBrowserType))
                driver.Manage().Window.Maximize();

            // If we're on Sauce, there's no need to delete cookies (and it causes iOS failures)
            if (GetTestExecutionEnvironment() != TestExecutionEnvironment.SauceLabs)
                try
                {
                    driver.Manage().Cookies.DeleteAllCookies();
                }
                catch (Exception)
                {
                    WebDriverEventLog.Add(
                        "Could not delete all cookies! Are you testing on iOS? If you see this message please contact TAS.");
                }

            driver.Manage().Timeouts().ImplicitlyWait(TimeSpan.FromSeconds(10));

            return driver;
        }

        private static IWebDriver GetDriverForLocalEnvironment(BrowserType browserToUse)
        {
            switch (browserToUse)
            {
                case BrowserType.Firefox:
                    FirefoxBinary binary = new FirefoxBinary(Configuration.Configuration.LocalFirefoxBinaryPath);
                    FirefoxProfile profile = new FirefoxProfile();
                    return new FirefoxDriver(binary, profile);

                case BrowserType.IE:
                    InternetExplorerOptions options = new InternetExplorerOptions
                    {
                        InitialBrowserUrl = "about:blank",
                        IntroduceInstabilityByIgnoringProtectedModeSettings = true,
                        IgnoreZoomLevel = true,
                        RequireWindowFocus = true
                    };

                    return new InternetExplorerDriver(Configuration.Configuration.LocalIeDriverServerPath, options);

                case BrowserType.Chrome:
                    return new ChromeDriver(Configuration.Configuration.LocalChromeDriverPath);
                default:
                    throw new ArgumentException("Unrecognised browser choice '" + browserToUse +
                                                "' when initialising driver for local environment.");
            }
        }

        private static IWebDriver GetDriverForGrid(BrowserType browserType)
        {
            DesiredCapabilities capabilities;

            switch (browserType)
            {
                case BrowserType.Firefox:
                    capabilities = DesiredCapabilities.Firefox();
                    break;
                case BrowserType.IE:
                    capabilities = DesiredCapabilities.InternetExplorer();
                    capabilities.SetCapability("ie.ensureCleanSession", "true");
                    break;
                case BrowserType.Chrome:
                    capabilities = DesiredCapabilities.Chrome();
                    break;
                case BrowserType.Safari:
                    capabilities = DesiredCapabilities.Safari();
                    break;
                default:
                    throw new ArgumentException("Unrecognised browser choice '" + browserType +
                                                "' when initialising driver for Grid.");
            }

            var platform = browserType.Equals(BrowserType.Safari) ? PlatformType.Mac : PlatformType.Vista;
            capabilities.SetCapability(CapabilityType.Platform, new Platform(platform));

            // So we know who's using it
            capabilities.SetCapability("environment",
                                        String.Format("{0} ({1})", Configuration.Configuration.GridIdentifier, Environment.MachineName));

            IWebDriver driver;
            try
            {
                driver = new SeleniumGridDriver(
                    new Uri(Configuration.Configuration.GridHubUrl), capabilities, TimeSpan.FromSeconds(900));
            }
            catch (Exception)
            {
                WebDriverEventLog.Add(String.Format("Failed when attempting to initialise a {0} browser on the Selenium Grid.{1}",
                    capabilities.BrowserName, Environment.NewLine));
                throw;
            }

            WebDriverEventLog.Add(String.Format("Grid Node: '{0}'{1}",
                   ((SeleniumGridDriver)driver).RemoteHost, Environment.NewLine));

            return driver;
        }

        private static IWebDriver GetDriverForSauceLabs(BrowserType browserType, string browserVersion)
        {
            const string seleniumVersion = "";
            DesiredCapabilities capabilities;

            var saucelabsUsername = Configuration.Configuration.SauceLabsUsername;
            var saucelabsAccessKey = Configuration.Configuration.SauceLabsAccessKey;
            var sauceLabsHubUrl = Configuration.Configuration.SauceLabsHubUrl;
            var nodeQueueingTimeout = TimeSpan.FromSeconds(Configuration.Configuration.CommandTimeoutSec);
            const string osPlatform = "Windows 7";

            switch (browserType)
            {
                case BrowserType.Firefox:
                    capabilities = DesiredCapabilities.Firefox();
                    if (browserVersion != "default")
                        capabilities.SetCapability(CapabilityType.Version, browserVersion);
                    capabilities.SetCapability("platform", osPlatform);
                    capabilities.SetCapability("screen-resolution", "1280x1024");
                    capabilities.SetCapability("selenium-version", seleniumVersion);
                    break;
                case BrowserType.IE:
                    capabilities = DesiredCapabilities.InternetExplorer();
                    if (browserVersion != "default")
                        capabilities.SetCapability(CapabilityType.Version, browserVersion);
                    capabilities.SetCapability("platform", osPlatform);
                    capabilities.SetCapability("screen-resolution", "1280x1024");
                    capabilities.SetCapability("selenium-version", seleniumVersion);
                    capabilities.SetCapability("ie.ensureCleanSession", true);

                    if (browserVersion == "8")
                        capabilities.SetCapability("iedriverVersion", "2.45.0");

                    break;
                case BrowserType.Chrome:
                    capabilities = DesiredCapabilities.Chrome();
                    if (browserVersion != "default")
                        capabilities.SetCapability(CapabilityType.Version, browserVersion);
                    capabilities.SetCapability("platform", osPlatform);
                    var screenResolution = "1280x1024";
                    // OSX only supports 1024x768
                    if (osPlatform.Contains("OS X"))
                        screenResolution = "1024x768";

                    capabilities.SetCapability("screen-resolution", screenResolution);
                    capabilities.SetCapability("selenium-version", seleniumVersion);
                    break;
                case BrowserType.Safari:
                    capabilities = DesiredCapabilities.Safari();
                    capabilities = SetSauceOsxBrowserCapabilities(capabilities, browserVersion);
                    capabilities.SetCapability("screen-resolution", "1024x768");
                    capabilities.SetCapability("selenium-version", seleniumVersion);
                    break;
                case BrowserType.Android:
                    capabilities = DesiredCapabilities.Android();
                    browserVersion = String.IsNullOrEmpty(browserVersion) ? "4.4" : browserVersion;

                    if (browserVersion == "beta")
                    {
                        //Sauce real device beta uses appium, max 5 devices concurrently
                        capabilities.SetCapability("platformName", "Android");
                        capabilities.SetCapability("deviceName", "Samsung Galaxy S4 Device");
                        capabilities.SetCapability("platformVersion", "4.3");
                        capabilities.SetCapability("browserName", "Chrome");
                    }
                    else
                    {
                        // We are using Selendroid with Sauce Connect
                        capabilities.SetCapability("platform", "Linux");
                        capabilities.SetCapability("version", browserVersion);
                        capabilities.SetCapability("deviceName", "Android Emulator");
                        capabilities.SetCapability("browserName", "Android");
                        capabilities.SetCapability("javascriptEnabled", true);
                    }

                    capabilities.SetCapability("appium-version", "");
                    capabilities.SetCapability("device-orientation", "portrait");
                    capabilities.SetCapability("newCommandTimeout", "60");

                    break;
                case BrowserType.iPhone:
                    capabilities = DesiredCapabilities.IPhone();
                    browserVersion = String.IsNullOrEmpty(browserVersion) ? "7.1" : browserVersion;
                    capabilities.SetCapability("platformName", "iOS");
                    capabilities.SetCapability("platformVersion", browserVersion);
                    capabilities.SetCapability("browserName", "safari");
                    capabilities.SetCapability("deviceName", "iPhone Simulator");
                    capabilities.SetCapability("device-orientation", "portrait");
                    capabilities.SetCapability("appium-version", "");
                    capabilities.SetCapability("newCommandTimeout", "180");
                    capabilities.SetCapability("safariAllowPopups", "true");
                    break;
                case BrowserType.iPad:
                    capabilities = DesiredCapabilities.IPad();
                    browserVersion = String.IsNullOrEmpty(browserVersion) ? "7.1" : browserVersion;
                    capabilities.SetCapability("platformName", "iOS");
                    capabilities.SetCapability("platformVersion", browserVersion);
                    capabilities.SetCapability("browserName", "safari");
                    capabilities.SetCapability("deviceName", "iPad Simulator");
                    capabilities.SetCapability("device-orientation", "landscape");
                    capabilities.SetCapability("safariAllowPopups", "true");
                    capabilities.SetCapability("newCommandTimeout", "180");
                    capabilities.SetCapability("appium-version", "");
                    break;
                default:
                    throw new ArgumentException("Unrecognised browser choice '" + browserType +
                                                "' when initialising driver for Saucelabs.");
            }

            // NOTE: Increasing the command-timeout from 180 to 300 due to persistent VM timeouts during periods of high concurrency.
            capabilities.SetCapability("command-timeout", 300);

            capabilities.SetCapability("idle-timeout", 180);
            capabilities.SetCapability("locationContextEnabled", false);
            capabilities.SetCapability("username", saucelabsUsername);
            capabilities.SetCapability("accessKey", saucelabsAccessKey);

            capabilities = ConfigureSauceLabsTunnel(capabilities);

            capabilities.SetCapability("name", ScenarioContext.Current.ScenarioInfo.Title);
            capabilities.SetCapability("tags", ScenarioContext.Current.ScenarioInfo.Tags);

            var driver = new SauceLabsDriver(
                new Uri(sauceLabsHubUrl),
                capabilities,
                nodeQueueingTimeout);

            SaucelabsJobId = driver.JobId.ToString();

            return driver;
        }

        private static DesiredCapabilities ConfigureSauceLabsTunnel(DesiredCapabilities capabilities)
        {
            capabilities.SetCapability("tunnel-identifier", Configuration.Configuration.SauceLabsTunnelName);

            if (Configuration.Configuration.SauceLabsUsername != Configuration.Configuration.SauceLabsParentName)
                capabilities.SetCapability("parent-tunnel", Configuration.Configuration.SauceLabsParentName);

            return capabilities;
        }

        private static DesiredCapabilities SetSauceOsxBrowserCapabilities(DesiredCapabilities capabilities, string browserVersion)
        {
            string osVersion;
            switch (browserVersion)
            {
                case "8":
                    osVersion = "OS X 10.10";
                    break;
                case "7":
                    osVersion = "OS X 10.9";
                    break;
                case "6":
                    osVersion = "OS X 10.8";
                    break;
                case "5":
                    osVersion = "OS X 10.6";
                    break;
                default:
                    var errorMessage = String.Format("The given browser version {0} is not applicable to Sauce OSX", browserVersion);
                    throw new ArgumentException(errorMessage);
            }
            capabilities.SetCapability(CapabilityType.Version, browserVersion);
            capabilities.SetCapability("platform", osVersion);
            return capabilities;
        }

        public enum BrowserType
        {
            Firefox,
            IE,
            Chrome,
            Safari,
            Android,
            iPad,
            iPhone
        }

        private static bool BrowserSupportsResizing(BrowserType browserType)
        {
            return browserType != BrowserType.Android && browserType != BrowserType.iPad && browserType != BrowserType.iPhone;
        }

        private static IWebDriver AttachEventFiringWebDriver(IWebDriver driver)
        {
            EventFiringWebDriver firingDriver = new EventFiringWebDriver(driver);

            firingDriver.ElementValueChanging += WebDriverEventHandlers.webdriver_ElementValueChangingHandler;
            firingDriver.FindingElement += WebDriverEventHandlers.webdriver_FindingElementHandler;
            firingDriver.Navigating += WebDriverEventHandlers.webdriver_NavigatingHandler;
            firingDriver.ElementClicking += WebDriverEventHandlers.webdriver_ElementClickingHandler;
            //firingDriver.ElementClicked += WebDriverEventHandlers.webdriver_ElementClickedHandler;
            //firingDriver.ElementValueChanged += WebDriverEventHandlers.webdriver_ElementValueChangedHandler;
            //firingDriver.Navigated += WebDriverEventHandlers.webdriver_NavigatedHandler;

            return firingDriver;
        }

        public enum TestExecutionEnvironment
        {
            Local,
            SauceLabs,
            Grid
        }
    }
}
