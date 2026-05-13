using FluentAssertions;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text;

namespace DatesAndStuff.Web.Tests;

[TestFixture]
public class FlightTest
{
    private IWebDriver driver;
    private StringBuilder verificationErrors;
    private bool acceptNextAlert = true;

    [SetUp]
    public void SetupTest()
    {
        driver = new ChromeDriver();
        verificationErrors = new StringBuilder();
    }

    [TearDown]
    public void TeardownTest()
    {
        try
        {
            driver.Quit();
            driver.Dispose();
        }
        catch (Exception)
        {
        }

        Assert.That(verificationErrors.ToString(), Is.EqualTo(""));
    }

    [Test]
    public void BlazeDemo_MexicoCity_To_Dublin_ShouldHaveAtLeastThreeFlights()
    {
        // Arrange
        double maximumPrice = 500;

        driver.Navigate().GoToUrl("https://blazedemo.com");

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        var fromSelectElement = wait.Until(ExpectedConditions.ElementIsVisible(
            By.Name("fromPort")
        ));

        var toSelectElement = wait.Until(ExpectedConditions.ElementIsVisible(
            By.Name("toPort")
        ));

        var fromSelect = new SelectElement(fromSelectElement);
        var toSelect = new SelectElement(toSelectElement);

        fromSelect.SelectByText("Mexico City");
        toSelect.SelectByText("Dublin");

        // Act
        var submitButton = wait.Until(ExpectedConditions.ElementToBeClickable(
            By.CssSelector("input[type='submit']")
        ));

        submitButton.Click();

        // Assert
        wait.Until(ExpectedConditions.ElementIsVisible(
            By.CssSelector("table")
        ));

        var flights = driver.FindElements(By.CssSelector("table tbody tr"));

        flights.Count.Should().BeGreaterThanOrEqualTo(3);

        foreach (var flight in flights)
        {
            var columns = flight.FindElements(By.TagName("td"));

            var priceText = columns[5].Text;
            priceText = priceText.Replace("$", "");

            var price = double.Parse(priceText);

            if (price < maximumPrice)
            {
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();

                var downloadsPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads"
                );

                var filePath = Path.Combine(downloadsPath, "cheap-dublin-flight.png");

                screenshot.SaveAsFile(filePath);

                break;
            }
        }
    }
    private bool IsElementPresent(By by)
    {
        try
        {
            driver.FindElement(by);
            return true;
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }

    private bool IsAlertPresent()
    {
        try
        {
            driver.SwitchTo().Alert();
            return true;
        }
        catch (NoAlertPresentException)
        {
            return false;
        }
    }

    private string CloseAlertAndGetItsText()
    {
        try
        {
            IAlert alert = driver.SwitchTo().Alert();
            string alertText = alert.Text;

            if (acceptNextAlert)
            {
                alert.Accept();
            }
            else
            {
                alert.Dismiss();
            }

            return alertText;
        }
        finally
        {
            acceptNextAlert = true;
        }
    }
}