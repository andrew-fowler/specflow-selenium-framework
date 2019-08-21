using System;
using System.IO;
using System.Net;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Remote;

namespace SpecflowSeleniumFramework.DriverWrappers
{
    public class SeleniumGridDriver : RemoteWebDriver, ITakesScreenshot
    {
        public SeleniumGridDriver(Uri remoteAddress, ICapabilities capabilities, TimeSpan commandTimeout)
            : base(remoteAddress, capabilities, commandTimeout)
        {
        }

        public new Screenshot GetScreenshot()
        {
            //Get the screenshot as base64. 
            var screenshotResponse = Execute(DriverCommand.Screenshot, null);
            var base64 = screenshotResponse.Value.ToString();
            //Convert it. 
            return new Screenshot(base64);
        }

        public string RemoteHost
        {
            get
            {
                const string hubSub = "/wd/hub";
                var hubRoot = Configuration.Configuration.GridHubUrl.Replace(hubSub, String.Empty);

                var uri = new Uri(string.Format("{0}/grid/api/testsession?session={1}", hubRoot, SessionId));

                var request = (HttpWebRequest) WebRequest.Create(uri);
                request.Method = "POST";
                request.ContentType = "application/json";

                var retries = 0;

                // The retry beaviour below is used to harden the framework against intermittent comms
                // dropouts.  These are caused by network issues, or node resource problems.
                retry:
                using (var httpResponse = (HttpWebResponse) request.GetResponse())
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var response = JObject.Parse(streamReader.ReadToEnd());

                    if (response == null || response.SelectToken("proxyId") == null)
                    {
                        if (response == null)
                            Console.WriteLine("Node Id response was null, retrying.");

                        else if (response.SelectToken("proxyId") == null)
                            Console.WriteLine("Node response did not contain proxyId, retrying.");

                        if (retries < 5)
                        {
                            retries++;
                            System.Threading.Thread.Sleep(2000);
                            goto retry;
                        }
                        else
                        {
                            return "COULD_NOT_CONNECT";
                        }
                    }

                    return response.SelectToken("proxyId").ToString();
                }
            }
        }
    }
}
