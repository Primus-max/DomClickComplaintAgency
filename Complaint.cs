using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomclickComplaint
{
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");
        private UndetectedChromeDriver? _driver;

        public async void SendComplaint()
        {
            // Получаем драйвер
            ConnectionDriver connection = new();
            _driver = await connection.GetDriverAsync();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10)); // Ожидание для всего класса

            try
            {
                // Перехожу на сайт
                _driver.Navigate().GoToUrl(baseUri);
                _driver.Manage().Window.Maximize();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось перейти по указанному адресу по причине: {ex.Message}");
            }

            // Прохожу авторизацию
            Authorization authorization = new();
            bool isAuthSeccess = await authorization.AuthenticateAsync(_driver);

            if (isAuthSeccess)
            {
                // Получаю кнопку КУПИТЬ и кликаю
                var buyButton = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.CssSelector("[data-e2e-id='topline__link-undefined']")));

                IWebElement modalOverlay = _driver.FindElement(By.CssSelector(".modal-overlay-13-0-6"));
                if (modalOverlay.Displayed)
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    js.ExecuteScript("arguments[0].style.display='none';", modalOverlay);
                }

                // кликаем на элемент
                buyButton.Click();

            }
        }
    }
}
