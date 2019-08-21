using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.Events;
using OpenQA.Selenium.Support.UI;

namespace SpecflowSeleniumFramework
{
    public static class WebDriverSupport
    {
       
        /// <summary>
        /// Returns a nodes InnerHtml value
        /// </summary>
        /// <param name="driver">The current <see cref="IWebDriver"/> reference</param>
        /// <param name="element">The <see cref="IWebElement"/> instance to query</param>
        /// <returns></returns>
        internal static string GetInnerHtml(IWebDriver driver, IWebElement element)
        {
            return (string)((IJavaScriptExecutor)driver).ExecuteScript("return arguments[0].innerHTML", element);
        }

        public static void WaitForUrlToContain(IWebDriver driver, string value, int timeoutSec = 5)
        {
            Wait(driver, timeoutSec).Until(d => driver.Url.Contains(value));
        }

        public static void SwitchToNewWindow(IWebDriver driver)
        {
            Wait(driver,
                 "Tried to switch to a new window, but there was no new window to switch to.",
                 Timeouts.Extreme)
                .Until(d => driver.WindowHandles.Count != 1);

            driver.SwitchTo().Window(FindNewWindowHandle(driver, driver.CurrentWindowHandle, 30));
        }

        private static string FindNewWindowHandle(IWebDriver driver, string existingHandle, int timeout)
        {
            string foundHandle = string.Empty;
            DateTime endTime = DateTime.Now.Add(TimeSpan.FromSeconds(timeout));
            while (string.IsNullOrEmpty(foundHandle) && DateTime.Now < endTime)
            {
                IList<string> currentHandles = driver.WindowHandles;

                foreach (string currentHandle in currentHandles)
                {
                    if (existingHandle != currentHandle)
                    {
                        foundHandle = currentHandle;
                        break;
                    }

                }

                if (string.IsNullOrEmpty(foundHandle))
                {
                    Thread.Sleep(250);
                }
            }

            // Note: could optionally check for handle found here and throw
            // an exception if no window was found.
            return foundHandle;
        }

        /// <summary>
        /// Convenience method for wrapping up the standard WebDriverWait construction.
        /// </summary>
        /// <param name="driver">The relevant driver instance.</param>
        /// <param name="timeout">The length of time in seconds to poll for the condition.</param>
        /// <returns>The WebDriverWait instance</returns>
        /// <example>
        /// <code>
        /// Utils.Wait(driver).Until(d => checkbox.Displayed);
        /// </code>
        /// </example>
        public static WebDriverWait Wait(IWebDriver driver, int timeout = 5)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout));
        }

        /// <summary>
        /// Convenience method for wrapping up the standard WebDriverWait construction.
        /// </summary>
        /// <param name="driver">The relevant driver instance.</param>
        /// <param name="message">The error message to use in a Timeout condition.</param>
        /// <param name="timeout">The length of time in seconds to poll for the condition.</param>
        /// <returns>The WebDriverWait instance</returns>
        /// <example>
        /// <code>
        /// Utils.Wait(driver, "The checkbox was not displayed as expected.").Until(d => checkbox.Displayed);
        /// </code>
        /// </example>
        public static WebDriverWait Wait(IWebDriver driver, string message, int timeout = 5)
        {
            return new WebDriverWait(driver, TimeSpan.FromSeconds(timeout)) { Message = message };
        }

        /// <summary>
        /// Simple existence method, wrapping up the inherently assertive FindElement.
        /// </summary>
        /// <param name="driver">The relevant <see cref="IWebDriver"/> instance.</param>
        /// <param name="loc">The locator of the element to check for.</param>
        /// <param name="timeout">An optional timeout specification.</param>
        /// <returns>Whether or not the supplied <see cref="By"/> locator located an element.</returns>
        public static bool ElementExists(this IWebDriver driver, By loc, int timeout = Timeouts.StandardImplicitTimeout)
        {
            try
            {
                Wait(driver, timeout).Until(d => driver.FindElements(loc).Count > 0);
                return true;
            }
            // Don't catch stale element exception, better to know this than assume any issue is existence
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// This method will retry an action a number of times, disregarding stale element exceptions.
        /// NOTE:  This can NOT be used for chained location actions like 'driver.find(element).Click' as 
        /// the driver.find(element) will resolve before passed through, which means the element will NOT
        /// be re-acquired.
        /// </summary>
        /// <param name="action">The Action to perform</param>
        /// <param name="maxRetries">The maximum number of retries</param>
        public static void StaleGuard(Action action, int maxRetries = 5)
        {
            var retries = 0;
        Retry:
            try
            {
                action.Invoke();
            }
            catch (StaleElementReferenceException)
            {
                if (retries < maxRetries)
                {
                    retries++;
                    Thread.Sleep(500);
                    goto Retry;
                }
                throw;
            }
        }

        /// <summary>
        /// This method will retry a click action a number of times, disregarding stale element exceptions.
        /// </summary>
        /// <param name="theElement">The Element to perform click on</param>
        /// <param name="maxRetries">The maximum number of retries</param>
        public static void StaleGuardClick(Func<IWebElement> theElement, int maxRetries = 5)
        {
            int retries = 0;
        Retry:
            try
            {
                theElement.Invoke().Click();
            }
            catch (StaleElementReferenceException)
            {
                if (retries < maxRetries)
                {
                    retries++;
                    Thread.Sleep(500);
                    goto Retry;
                }
                throw;
            }
        }

        /// <summary>
        /// Simple existence method, wrapping up the inherently assertive FindElement.
        /// </summary>
        /// <param name="driver">The relevant <see cref="IWebDriver"/> instance.</param>
        /// <param name="searchTarget">The locator for the desired element.</param>
        /// <param name="timeout">An optional timeout specification.</param>
        /// <param name="parent">The parent element locator to search within.</param>
        /// <returns>Whether or not the supplied <see cref="By"/> locator located an element.</returns>
        public static bool ElementExists(IWebDriver driver, IWebElement parent, By searchTarget,
                                  int timeout = Timeouts.StandardImplicitTimeout)
        {
            try
            {
                Wait(driver, timeout).Until(d => parent.FindElements(searchTarget).Count > 0);
                return true;
            }
            // Don't catch stale element exception, better to know this than assume any issue is existence
            catch (WebDriverTimeoutException)
            {
                return false;
            }
        }

        /// <summary>
        /// Simulate the mouse pointer hovering over a page element.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="elem">The element to hover over.</param>
        /// <param name="javascriptWorkaround">There is an issue in some releases of the advanced
        /// interaction API.  If using the one of these bindings set this pararm to true to enable a js workaround</param>
        public static void HoverOver(IWebDriver driver, IWebElement elem, bool javascriptWorkaround = false)
        {
            if (javascriptWorkaround)
            {
                const string js = "var fireOnThis = arguments[0];"
                                  + "var evObj = document.createEvent('MouseEvents');"
                                  + "evObj.initEvent( 'mouseover', true, true );"
                                  + "fireOnThis.dispatchEvent(evObj);";
                ((IJavaScriptExecutor)driver).ExecuteScript(js, elem);
            }
            else
            {
                Actions builder = new Actions(driver);
                builder.MoveToElement(elem).Build().Perform();
            }
        }

        /// <summary>
        /// Drag and drop a source element to a target element.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="source">The element to drag.</param>
        /// <param name="target">The element to drop 'on'.</param>
        public static void DragAndDrop(IWebDriver driver, IWebElement source, IWebElement target)
        {
            Actions builder = new Actions(driver);
            builder.DragAndDrop(source, target).Build().Perform();
        }

        /// <summary>
        /// In rare cases, we may need to show a hidden element.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="element">The element to unhide.</param>
        /// <remarks>Note: Using this breaks the as-user principle.</remarks>
        public static void ShowHiddenElement(IWebDriver driver, IWebElement element)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript("arguments[0].style.display='block';", element);
            executor.ExecuteScript("arguments[0].style.visibility='visible';", element);
        }

        /// <summary>
        /// Executes the supplied javascript.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="strJs">The javascript code to execute.</param>
        public static void ExecuteJavascript(IWebDriver driver, string strJs)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            executor.ExecuteScript(strJs);
        }

        /// <summary>
        /// Gets the value of supplied javascript property.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="strJs">The javascript property to return.</param>
        public static string GetJavascriptPropertyValue(IWebDriver driver, string strJs)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            try
            {
                return (string)executor.ExecuteScript("return " + strJs);
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception(String.Format("'{0}' does not appear to be initialised", strJs), ex);
            }
            catch (NullReferenceException ex)
            {
                throw new Exception(String.Format("'{0}' does not appear to be initialised (NullReferenceException)", strJs), ex);
            }
        }

        /// <summary>
        /// Checks to see if the supplied image element is visually rendered on the page.
        /// </summary>
        /// <param name="image">The image element to check</param>
        /// <returns>Whether or not the supplied image element is visually rendered on the page.</returns>
        public static bool IsImageRendered(IWebElement image)
        {
            return (image.Size.Width > 1 && image.Size.Height > 1);
        }

        /// <summary>
        /// A naive, but quick, equivalence check between two elements.
        /// </summary>
        /// <param name="e1">The first element to compare.</param>
        /// <param name="e2">The second element to compare.</param>
        /// <returns>Whether or not the elements appear to be equivalent.</returns>
        /// <remarks>Currently checks location, name, displayed and enabled for equivalence.</remarks>
        public static bool ElementsMatch(IWebElement e1, IWebElement e2)
        {
            return (
                       e1.Location.X == e2.Location.X &&
                       e1.Location.Y == e2.Location.Y &&
                       e1.TagName == e2.TagName &&
                       e1.Displayed == e2.Displayed &&
                       e1.Enabled == e2.Enabled
                   );
        }
        
        /// <summary>
        /// Convenience method for sending keys to an element slowly.  This is particularly relevant for controls that have
        /// keypress events, for example AutoSuggests.
        /// </summary>
        /// <param name="element">The element to type into.</param>
        /// <param name="text">The text to type.</param>
        /// <param name="mSecDelay">The amount of time in milliseconds to delay between each key press.</param>
        public static void SendKeysSlowly(IWebElement element, string text, int mSecDelay = 250)
        {
            for (int i = 0; i < text.Length; i++)
            {
                element.SendKeys(text.ToCharArray()[i].ToString());
                Thread.Sleep(mSecDelay);
            }
            Thread.Sleep(mSecDelay * 2);
        }

        #region Cookie Handling

        /// <summary>
        /// This will set a new cookie with the specified parameters.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="strName">The name of the cookie to store.</param>
        /// <param name="strValue">The value of the cookie to store.</param>
        /// <returns>The formed cookie object.</returns>
        /// <remarks>Ensure that any call to this method occurs after the landing page is requested.</remarks>
        public static Cookie SetCookie(IWebDriver driver, string strName, string strValue)
        {
            Cookie cookie = new Cookie(strName, strValue);
            driver.Manage().Cookies.AddCookie(cookie);
            return cookie;
        }

        /// <summary>
        /// This allows to to inspect all cookies associated with the current browser session.
        /// </summary>
        /// <returns>All cookies associated with the current browser session.</returns>
        /// <param name="driver">The driver instance to use.</param>
        public static ReadOnlyCollection<Cookie> GetCookies(IWebDriver driver)
        {
            return driver.Manage().Cookies.AllCookies;
        }

        /// <summary>
        /// Access a cookie by name.
        /// </summary>
        /// <param name="driver"> </param>
        /// <param name="strName">The name of the cookie to return.</param>
        /// <returns>The cookie matching the specified name if one exists.</returns>
        public static Cookie GetCookie(IWebDriver driver, string strName)
        {
            return driver.Manage().Cookies.GetCookieNamed(strName);
        }

        /// <summary>
        /// Delete a cookie using its object representation.
        /// </summary>
        /// <param name="driver"> </param>
        /// <param name="cookie">The cookie to remove from the browser session.</param>
        public static void DeleteCookie(IWebDriver driver, Cookie cookie)
        {
            driver.Manage().Cookies.DeleteCookie(cookie);
        }

        /// <summary>
        /// Delete a cookie using its name.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="strName">The cookie to remove from the browser session.</param>
        public static void DeleteCookieNamed(IWebDriver driver, string strName)
        {
            driver.Manage().Cookies.DeleteCookieNamed(strName);
        }

        /// <summary>
        /// Delete all cookies from the current browser session.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        public static void DeleteAllCookies(IWebDriver driver)
        {
            driver.Manage().Cookies.DeleteAllCookies();
        }

        #endregion Cookie Handling

        /// <summary>
        /// Applies a yellow border to elements to aid debugging.  
        /// </summary>
        /// <param name="driver">The relevant driver instance.</param>
        /// <param name="element">The element to highlight.</param>
        public static void HighlightElement(IWebDriver driver, IWebElement element)
        {
            for (var i = 0; i < 2; i++)
            {
                IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
                executor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                                       element, "color: yellow; border: 2px solid yellow;");
                executor.ExecuteScript("arguments[0].setAttribute('style', arguments[1]);",
                                       element, "");
            }
        }


        #region WebDriver event handling

        private static void webdriver_ElementValueChangingHandler(object sender, WebElementEventArgs e)
        {
            Log("Changing element ");
        }

        private static void webdriver_FindingElementHandler(object sender, FindElementEventArgs e)
        {
            Log("Finding element: " + e.FindMethod);
        }

        private static void webdriver_NavigatingHandler(object sender, WebDriverNavigationEventArgs e)
        {
            Log("Navigating to: " + e.Url);
        }

        private static void webdriver_ElementClickingHandler(object sender, WebElementEventArgs e)
        {
            Log("Clicking element");
        }

        private static void Log(string text)
        {
            Console.WriteLine("     {0}: {1}", DateTime.Now.ToString("hh:mm:ss.fff"), text);
        }

        public static IWebDriver AttachEventFiringWebDriver(IWebDriver driver)
        {
            EventFiringWebDriver firingDriver = new EventFiringWebDriver(driver);

            firingDriver.ElementValueChanging += webdriver_ElementValueChangingHandler;
            firingDriver.FindingElement += webdriver_FindingElementHandler;
            firingDriver.Navigating += webdriver_NavigatingHandler;
            firingDriver.ElementClicking += webdriver_ElementClickingHandler;

            return firingDriver;
        }

        #endregion WebDriver event handling

        public static void WaitForWindowCount(IWebDriver driver, int i)
        {
            Wait(driver).Until(d => driver.WindowHandles.Count == i);
        }

        public static IWebDriver Window(ITargetLocator targetLocator, string handle, string browser, bool usingSauce)
        {
            var regularPageNeutralElement = By.Id("copyright");
            var dayViewBookingPanelNeutralElement = By.XPath("//span[@class='header-title']");

            if (usingSauce && browser.ToLower() == "ie")
            {
                var driver = targetLocator.Window(handle);
                if (driver.FindElement(regularPageNeutralElement).Enabled)
                    driver.FindElement(regularPageNeutralElement).Click();
                else
                    driver.FindElement(dayViewBookingPanelNeutralElement).Click();

                return driver;
            }
            return targetLocator.Window(handle);
        }
    }
}
