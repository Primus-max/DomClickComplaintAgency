using OpenQA.Selenium;
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
        private UndetectedChromeDriver? _driver;

        public async void SendComplaint()
        {
            // Получаем драйвер
            ConnectionDriver connection = new();
            _driver = await connection.GetDriverAsync();


            _driver.Navigate().GoToUrl("https://domclick.ru");

            _driver.Manage().Window.Maximize();

            // Находим родительский элемент
            IWebElement parentElement = _driver.FindElement(By.XPath("//div[@data-e2e-id='topline__sign-in']"));

            // Кликаем на родительском элементе
            parentElement.Click();
        }
    }
}
