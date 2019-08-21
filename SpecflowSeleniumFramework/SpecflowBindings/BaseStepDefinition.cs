using OpenQA.Selenium;
using SpecflowSeleniumFramework.Contexts;
using TechTalk.SpecFlow;

namespace SpecflowSeleniumFramework.SpecflowBindings
{
    [Binding]
    public class BaseStepDefinition : Steps
    {
        protected static IWebDriver Driver;

        protected const string SharedContextName = "SharedContext";

        #region Context Accessors

        protected static SharedContext SharedContext
        {
            get { return (SharedContext)ScenarioContext.Current[SharedContextName]; }
            set { ScenarioContext.Current[SharedContextName] = value; }
        }
        #endregion Context Accessors
    }
}
