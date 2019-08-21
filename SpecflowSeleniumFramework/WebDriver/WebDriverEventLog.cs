using System;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework
{
    public class WebDriverEventLog
    {
        private static string _log = string.Empty;

        public new static string ToString()
        {
            return ScenarioContext.Current["LogContext"] +
                String.Format("     {0}: {1}{2}", DateTime.Now.ToString("hh:mm:ss.fff"), "<End of Log>", Environment.NewLine);
        }

        public static void Add(string text)
        {
            _log += String.Format("     {0}: {1}{2}", DateTime.Now.ToString("hh:mm:ss.fff"), text, Environment.NewLine);
            ScenarioContext.Current["LogContext"] += String.Format("     {0}: {1}{2}", DateTime.Now.ToString("hh:mm:ss.fff"), text, Environment.NewLine);
        }
    }
}
