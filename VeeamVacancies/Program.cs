using System;
using System.Text.RegularExpressions;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace VeeamVacancies
{
    class Program
    {
        static IWebDriver driver;

        static void SetupDriver()
        {
            FirefoxOptions options = new FirefoxOptions();
            driver = new FirefoxDriver(options);
            driver.Manage().Window.Maximize();
        }

        static void OpenUrl(IWebDriver driver, string url)
        {
            Console.WriteLine($"Opening URL: {url}");
            driver.Navigate().GoToUrl(url);
        }

        static IWebElement FindElement(string value, string by = "xpath", int time = 10)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException($"\"{nameof(value)}\" should not be null or empty.", nameof(value));

            if (driver is null)
                throw new ArgumentNullException(nameof(driver));

            IWebElement element;
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(time));

            try
            {
                switch (by)
                {
                    case "class_name":
                        element = wait.Until(driver => driver.FindElement(By.ClassName(value)));
                        break;
                    case "name":
                        element = wait.Until(driver => driver.FindElement(By.Name(value)));
                        break;
                    case "xpath":
                        element = wait.Until(driver => driver.FindElement(By.XPath(value)));
                        break;
                    case "id":
                        element = wait.Until(driver => driver.FindElement(By.Id(value)));
                        break;
                    case "link_text":
                        element = wait.Until(driver => driver.FindElement(By.LinkText(value)));
                        break;
                    default:
                        Console.WriteLine("Error! Element type is not allowed.");
                        throw new ArgumentException();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            return element;
        }

        static void SelectFromDropdown(string dropdown, string value, bool clickToClose = false, string findDropdownBy = "xpath", string findValueBy = "xpath")
        {
            IWebElement dropdownElement;
            IWebElement valueElement;

            try
            {
                dropdownElement = FindElement(dropdown, findDropdownBy);
                if (dropdownElement == null)
                    throw new ArgumentNullException();
                dropdownElement.Click();

                valueElement = FindElement(value, findValueBy);
                if (dropdownElement == null)
                    throw new ArgumentNullException();
                valueElement.Click();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
            if (clickToClose)
                dropdownElement.Click();
        }

        enum Language
        {
            Russian = 0, 
            English = 1,
            Deutsch = 2,
        }

        static void Main(string[] args)
        {
            SetupDriver();

            string schema = "https://";
            string url = "careers.veeam.ru/vacancies";

            OpenUrl(driver, schema + url);

            string departmentName = "Разработка продуктов";

            #region XPaths
            string dprtDropdownXPath = "//*[@id=\"root\"]/div/div[1]/div/div[2]/div[1]/div/div[2]/div/div";
            string dprtXPath = $"//div/a[text()[contains(., '{departmentName}')]]";
            string langDropdownXPath = "//*[@id=\"root\"]/div/div[1]/div/div[2]/div[1]/div/div[3]/div/div";
            string langXPath = $"//input[@id=\"lang-option-{(int)Language.English}\"]";
            #endregion

            SelectFromDropdown(dprtDropdownXPath, dprtXPath);
            SelectFromDropdown(langDropdownXPath, langXPath, true);

            (int expected, int counted) = (0, 0);
            expected = Int32.Parse(Regex.Match(FindElement(dprtDropdownXPath).Text, @"\d+").Value);
            counted = driver.FindElements(By.ClassName("card-sm")).Count;
            int difference = expected - counted >= 0 ? expected - counted : counted - expected;
            Console.WriteLine($"Expected number of vacancies: {expected}\r\n" +
                $"Counted number of vacancies: {counted}\r\n" +
                $"The difference between result and expectation is {difference}");
        }
    }
}
