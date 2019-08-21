using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SpecflowSeleniumFramework.Other;

namespace SpecflowSeleniumFramework
{

    /// <summary>
    /// A central location for all extension methods of the WebDriver API
    /// </summary>
    public static class WebdriverExtensions
    {
        //public static Retry Retry(this IWebDriver driver, int maxRetries = 5)
        //{
        //    return new Retry(driver, maxRetries);
        //}

        #region "Driver-centric Utils refs"
        
        /// <summary>
        /// Convenience method for wrapping up the standard WebDriverWait construction.
        /// </summary>
        /// <param name="driver">The relevant driver instance.</param>
        /// <param name="timeout">The length of time in seconds to poll for the condition.</param>
        /// <returns>The WebDriverWait instance</returns>
        /// <example>
        /// <code>
        /// driver.Wait().Until(d => checkbox.Displayed);
        /// </code>
        /// </example>
        public static WebDriverWait Wait(this IWebDriver driver, int timeout = 5)
        {
            return WebDriverSupport.Wait(driver, timeout);
        }

        /// <summary>
        /// Convenience method for wrapping up the standard WebDriverWait construction.
        /// </summary>
        /// <param name="driver">The relevant driver instance.</param>
        /// <param name="message">The message to display upon timeout.</param>
        /// <param name="timeout">The length of time in seconds to poll for the condition.</param>
        /// <returns>The WebDriverWait instance</returns>
        /// <example>
        /// <code>
        /// driver.Wait().Until(d => checkbox.Displayed);
        /// </code>
        /// </example>
        public static WebDriverWait Wait(this IWebDriver driver, string message, int timeout = 5)
        {
            return WebDriverSupport.Wait(driver, message, timeout);
        }

        /// <summary>
        /// Executes the supplied javascript.
        /// </summary>
        /// <param name="driver">The driver instance to use.</param>
        /// <param name="strJs">The javascript code to execute.</param>
        public static void ExecuteJavascript(this IWebDriver driver, string strJs)
        {
            WebDriverSupport.ExecuteJavascript(driver, strJs);
        }

        /// <summary>
        /// Attempts to check if the supplied element is interactable. I.e. has the greatest Z index in its DOM area and is enabled.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="elementToQuery">The element to query.</param>
        /// <returns>If the element is interactable.</returns>
        public static bool IsInteractable(this IWebDriver driver, IWebElement elementToQuery)
        {
            var js = String.Format("return document.elementFromPoint({0}, {1});", elementToQuery.Location.X,
                                      elementToQuery.Location.Y);

            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            IWebElement elementFound = (IWebElement)executor.ExecuteScript(js);

            return (WebDriverSupport.ElementsMatch(elementToQuery, elementFound) && elementToQuery.Enabled);
        }

        /// <summary>
        /// This will take a screenshot of the currently rendered page and save it to the path set in the script.
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="strFilename">The inputFilename to save the screenshot to, without extension.</param>
        public static string TakeScreenshot(this IWebDriver driver, string strFilename)
        {
            return ScreenshotCreator.TakeScreenshot(driver, strFilename);
        }

        public static void SwitchToNewWindow(this IWebDriver driver)
        {
            WebDriverSupport.SwitchToNewWindow(driver);
        }

        /// <summary>
        /// On IE on saucelabs, when returning to a window with SwitchTo.Window() a click is required to refocus on the window
        /// </summary>
        /// <param name="targetLocator"></param>
        /// <param name="handle"></param>
        /// <param name="browser"></param>
        /// <param name="usingSauce"></param>
        /// <returns></returns>
        public static IWebDriver Window(this ITargetLocator targetLocator, string handle, string browser, bool usingSauce)
        {
            return WebDriverSupport.Window(targetLocator, handle, browser, usingSauce);
        }

        #endregion "Driver-centric Utils refs"

        #region "IWebElement"

        /// <summary>
        /// Sends the specified string to the element one character at a time with a short delay between pushes.
        /// This is primarily aimed at controls that do not deal with superfast input, such as elements with autosuggest events.
        /// </summary>
        /// <param name="element">The element to interact with.</param>
        /// <param name="text">The text to send slowly</param>
        /// <example>
        /// <code>
        /// myElement.SendKeysSlowly("Edinburgh");
        /// </code>
        /// </example>
        public static void SendKeysSlowly(this IWebElement element, string text, int mSecDelay = 250)
        {
            WebDriverSupport.SendKeysSlowly(element, text, mSecDelay);
        }

        #endregion "ExtendedMethods"

        /// <summary>
        /// Method to check if the supplied element is stable (is not moving, animating etc...)
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="locator"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static bool ElementIsStable(this IWebDriver driver, By locator, TimeSpan timeout)
        {
            const int mSecDelay = 1000;
            var primary = driver.FindElement(locator);
            int[] primaryCoordinates = GetElementViewPosition(driver, primary);
            var msecElapsed = 0;

            while (msecElapsed < timeout.TotalMilliseconds)
            {
                Thread.Sleep(mSecDelay);
                msecElapsed += mSecDelay;

                var secondary = driver.FindElement(locator);
                int[] secondaryCoordinates = GetElementViewPosition(driver, secondary);

                if (primary.Size == secondary.Size && primary.Location == secondary.Location
                    && primaryCoordinates == secondaryCoordinates)
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Function to wrap around the execution of Javascript
        /// </summary>
        /// <param name="driver">The driver to use</param>
        /// <param name="strJs">The Javascript query to be executed - this should be a function with arguments (if applicable)</param>
        /// <param name="args">the list of arguments for use in the Javascript function</param>
        /// <returns></returns>
        public static object ExecuteJavascript(this IWebDriver driver, string strJs, object[] args)
        {
            IJavaScriptExecutor executor = (IJavaScriptExecutor)driver;
            return executor.ExecuteScript(strJs, args);
        }

        /// <summary>
        /// A function to get the relative position of the element with reference to the viewport
        /// </summary>
        /// <param name="driver"></param>
        /// <param name="element"></param>
        /// <returns>Returns a tuple with the X,Y co-ordinates of the element</returns>
        public static int[] GetElementViewPosition(IWebDriver driver, IWebElement element)
        {
            const string getOffsetFunction = @"function viewportOffset(forElement) {{" +
                "var valueT = 0, valueL = 0;" +
                "var element = forElement;" +
                "do {{" +
                    "valueT += element.offsetTop  || 0;" +
                    "valueL += element.offsetLeft || 0;" +
                    "if (element.offsetParent == document.body) break;" +
                "}} while (element = element.offsetParent);" +
                "element = forElement;" +
                "do {{" +
                    "if (element.tagName == 'BODY') {{" +
                        "valueT -= element.scrollTop  || 0;" +
                        "valueL -= element.scrollLeft || 0;" +
                    "}}" +
                "}} while (element = element.parentNode);" +
                "return [valueL, valueT];" +
            "}}";
            object[] args = new object[1];
            args[0] = element;
            return (int[])ExecuteJavascript(driver, getOffsetFunction, args);
        }
    }

    ///// <summary>
    ///// A class to wrap up retry behaviours.
    ///// </summary>
    ///// <remarks>Note: These tend to be used only in exceptional circumstances.  Utils.StaleGuard is preferred, as long as the Action doesn't
    ///// include a resolvable location call as part of a chain.</remarks>
    //public class Retry
    //{
    //    private readonly int _maxRetries;
    //    private readonly IWebDriver _driver;

    //    public Retry(IWebDriver driver, int maxRetries)
    //    {
    //        _driver = driver;
    //        _maxRetries = maxRetries;
    //    }

    //    public bool Displayed(By locator)
    //    {
    //        int retries = 0;
    //    Retry:
    //        try
    //        {
    //            return _driver.FindElement(locator).Displayed;
    //        }
    //        catch (StaleElementReferenceException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //        catch (NoSuchElementException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Repeatedly click the element for the provided locator until a <see cref="StaleElementReferenceException"/> is not thrown.
    //    /// </summary>
    //    /// <param name="locator">The locator of the element to click.</param>
    //    public void Click(By locator)
    //    {
    //        int retries = 0;
    //    Retry:
    //        try
    //        {
    //            _driver.FindElement(locator).Click();
    //        }
    //        catch (StaleElementReferenceException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //        catch (NoSuchElementException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //    }

    //    /// <summary>
    //    /// Repeatedly click the element for the provided locator until a <see cref="StaleElementReferenceException"/> is not thrown.
    //    /// </summary>
    //    /// <param name="locator">The locator of the element to click.</param>
    //    /// <param name="parent">The locator of the parent element</param>
    //    public void Click(By locator, IWebElement parent)
    //    {
    //        int retries = 0;
    //    Retry:
    //        try
    //        {
    //            parent.FindElement(locator).Click();
    //        }
    //        catch (StaleElementReferenceException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //        catch (NoSuchElementException)
    //        {
    //            if (retries < _maxRetries)
    //            {
    //                retries++;
    //                Thread.Sleep(500);
    //                goto Retry;
    //            }
    //            throw;
    //        }
    //    }

    //    ///// <summary>
    //    ///// Repeatedly attempt to sendkeys to the element for the provided locator until a <see cref="StaleElementReferenceException"/> is not thrown.
    //    ///// </summary>
    //    ///// <param name="locator">The locator of the element to interact with.</param>
    //    ///// <param name="text">The text to send to the element.</param>
    //    //public void SendKeys(By locator, string text)
    //    //{
    //    //    int retries = 0;
    //    //Retry:
    //    //    try
    //    //    {
    //    //        _driver.FindElement(locator).SendKeys(text);
    //    //    }
    //    //    catch (StaleElementReferenceException)
    //    //    {
    //    //        if (retries < _maxRetries)
    //    //        {
    //    //            retries++;
    //    //            Thread.Sleep(500);
    //    //            goto Retry;
    //    //        }
    //    //        throw;
    //    //    }
    //    //    catch (NoSuchElementException)
    //    //    {
    //    //        if (retries < _maxRetries)
    //    //        {
    //    //            retries++;
    //    //            Thread.Sleep(500);
    //    //            goto Retry;
    //    //        }
    //    //        throw;
    //    //    }
    //    //}
    //}
}
