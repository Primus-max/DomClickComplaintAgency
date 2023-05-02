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

            try
            {
                _driver.Navigate().GoToUrl("https://domclick.ru");
                _driver.Manage().Window.Maximize();
            }
            catch (Exception)
            {

                throw;
            }


            Authorization authorization = new();
            bool isAuthSeccess = await authorization.AuthenticateAsync(_driver);


            if (isAuthSeccess)
            {

            }
        }
    }
}
