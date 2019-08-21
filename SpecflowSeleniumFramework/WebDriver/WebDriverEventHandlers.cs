using OpenQA.Selenium.Support.Events;

namespace SpecflowSeleniumFramework
{
    class WebDriverEventHandlers
    {
        protected internal static void webdriver_ElementValueChangingHandler(object sender, WebElementEventArgs e)
        {
            WebDriverEventLog.Add("Changing element ");
        }

        protected internal static void webdriver_FindingElementHandler(object sender, FindElementEventArgs e)
        {
            WebDriverEventLog.Add("Finding element: " + e.FindMethod);
        }

        protected internal static void webdriver_NavigatingHandler(object sender, WebDriverNavigationEventArgs e)
        {
            WebDriverEventLog.Add("Navigating to: " + e.Url);
        }

        protected internal static void webdriver_ElementClickingHandler(object sender, WebElementEventArgs e)
        {
            WebDriverEventLog.Add("Clicking element");
        }
    }
}
