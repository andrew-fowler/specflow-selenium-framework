using NUnit.Framework;
using OpenQA.Selenium;
using SpecflowSeleniumFramework.SpecflowBindings;
using TechTalk.SpecFlow;

namespace ExampleConsumer.StepDefinitions
{
    [Binding]
    public class Example : BaseStepDefinition
    {
        [Given(@"I navigate to '(.*)'")]
        public void GivenINavigateTo(string url)
        {
            Driver.Navigate().GoToUrl(url);
        }

        [Then(@"the google search box is visible")]
        public void ThenTheGoogleSearchBoxIsVisible()
        {
            var googleSearchBox = Driver.FindElement(By.Id("lst-ib"));
            Assert.That(googleSearchBox.Displayed, Is.True);
        }
    }
}
