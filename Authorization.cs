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
        private Random _randomeTimeWating = new Random();
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


                // Устанавливаем ожидание для всех ээлементов
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                // Находим кнопку авторизации
                var authButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@data-e2e-id='topline__sign-in']")));

                // Перемещаю курсор к кнопке и кликаю
                var action = new Actions(driver);
                action.MoveToElement(authButton).Perform();
                authButton.Click();

                // Получаю input для ввода телефона для авторизации
                var phoneNumberInput = wait.Until(ExpectedConditions.ElementExists(By.Id("topline-login-form__phone-input")));
                // phoneNumberInput.SendKeys(_phoneNumber);

                // По цифре ввожу номер телефона в поле
                Thread.Sleep(_randomeTimeWating.Next(1000, 2500));
                foreach (char c in _phoneNumber)
                {
                    phoneNumberInput.SendKeys(c.ToString());
                    Thread.Sleep(_randomeTimeWating.Next(100, 450));
                }

                Thread.Sleep(_randomeTimeWating.Next(1000, 2500));

                // Получаю кнопку отправки номера авторизации 

                var submitButton = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("button[data-e2e-id='topline-login-form__submit-button']")));

                // Перемещаю курсор к кнопке и кликаю
                action.MoveToElement(submitButton).Perform();
                Thread.Sleep(_randomeTimeWating.Next(700, 2000));
                submitButton.Click();
                //phoneNumberInput.Submit();

                Thread.Sleep(_randomeTimeWating.Next(1000, 2500));

                // Получаю input для вставки пароля и вставляю
                var passwordInput = wait.Until(ExpectedConditions.ElementExists(By.Id("topline-login-form__password-input")));
                //passwordInput.SendKeys(_password);
                foreach (char c in _password)
                {
                    passwordInput.SendKeys(c.ToString());
                    Thread.Sleep(_randomeTimeWating.Next(100, 450));
                }

                Thread.Sleep(_randomeTimeWating.Next(1000, 2500)); ;

                // Получаю кнопку для продтверждения пароля
                var loginButton = wait.Until(ExpectedConditions.ElementExists(By.CssSelector("button[data-e2e-id='topline-login-form__submit-button']")));

                Thread.Sleep(_randomeTimeWating.Next(700, 1800));

                // Перемещаю курсор к кнопке и кликаю
                //action.MoveToElement(loginButton).Perform();
                loginButton.Click();


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
