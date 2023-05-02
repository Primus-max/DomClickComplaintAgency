using Newtonsoft.Json;
using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomclickComplaint
{
    public class Authorization
    {
        private string? _phoneNumber;

        private string? _password;

        public async Task<bool> AuthenticateAsync(UndetectedChromeDriver driver)
        {
            try
            {
                var json = await File.ReadAllTextAsync("authData.json");
                var authData = JsonConvert.DeserializeObject<AuthData>(json);

                _phoneNumber = authData?.PhoneNumber;
                _password = authData?.Password;

                // Находим кнопку авторизации и кликаем
                IWebElement authBotton = driver.FindElement(By.XPath("//div[@data-e2e-id='topline__sign-in']"));                
                authBotton.Click();

                // код авторизации, использующий _driver, _phoneNumber и _password

                return true; // или false, если авторизация не удалась
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                return false;
            }
        }

    }
}
