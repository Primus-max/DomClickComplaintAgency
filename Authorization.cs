using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//TODO Сделать движение курсора
//TODO Сделать для всех задержек рандомное время

namespace DomclickComplaint
{
    public class Authorization
    {
        private string? _phoneNumber;

        private string? _password;

        private static readonly object _lock = new object();
        public async Task<bool> AuthenticateAsync(UndetectedChromeDriver driver)
        {
            try
            {
                AuthData authData;
                lock (_lock)
                {
                    var json = File.ReadAllText("authData.json");
                    authData = JsonConvert.DeserializeObject<AuthData>(json);
                }

                _phoneNumber = authData?.PhoneNumber;
                _password = authData?.Password;


                // Находим кнопку авторизации
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var authButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@data-e2e-id='topline__sign-in']")));
                               
                // Перемещаю курсор к кнопке и кликаю
                var action = new Actions(driver);
                action.MoveToElement(authButton).Perform();                
                authButton.Click();

                // Получаю input для ввода телефона для авторизации
                var phoneNumberInput = wait.Until(ExpectedConditions.ElementExists(By.Id("topline-login-form__phone-input")));
                // phoneNumberInput.SendKeys(_phoneNumber);
                // По цифре ввожу номер телефона в поле
                foreach (char c in _phoneNumber)
                {
                    phoneNumberInput.SendKeys(c.ToString());
                    Thread.Sleep(200);
                }

                Thread.Sleep(1000);

                // Получаю кнопку отправки номера авторизации 
                var submitButton = driver.FindElement(By.CssSelector("button[data-e2e-id='topline-login-form__submit-button']"));

                // Перемещаю курсор к кнопке и кликаю
                action.MoveToElement(authButton).Perform();
                submitButton.Click();
                //phoneNumberInput.Submit();

                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }    
    }
}
