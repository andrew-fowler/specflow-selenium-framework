using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SpecflowSeleniumFramework.DriverWrappers
{
    class SauceLabsDriver : RemoteWebDriver, IWebDriver
    {
        public SauceLabsDriver(ICommandExecutor commandExecutor, ICapabilities desiredCapabilities)
            : base(commandExecutor, desiredCapabilities)
        {
        }

        public SauceLabsDriver(ICapabilities desiredCapabilities)
            : base(desiredCapabilities)
        {
        }

        public SauceLabsDriver(Uri remoteAddress, ICapabilities desiredCapabilities)
            : base(remoteAddress, desiredCapabilities)
        {
        }

        public SauceLabsDriver(Uri remoteAddress, ICapabilities desiredCapabilities, TimeSpan commandTimeout)
            : base(remoteAddress, desiredCapabilities, commandTimeout)
        {
        }

        /// <summary>
        /// Returns the normally protected SessionId object for Sauce API lookups
        /// </summary>
        public SessionId JobId { get { return SessionId; } }
    }
}
