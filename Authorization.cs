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


                // Находим кнопку авторизации и кликаем
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                var authButton = wait.Until(ExpectedConditions.ElementExists(By.XPath("//div[@data-e2e-id='topline__sign-in']")));

                //int coorX = authButton.Location.X;
                //int coorY = authButton.Location.Y;

                var action = new Actions(driver);
                action.MoveToElement(authButton).Perform();
                authButton.Click();

                // await Task.Delay(1000);

                var phoneNumberInput = wait.Until(ExpectedConditions.ElementExists(By.Id("topline-login-form__phone-input")));
                // phoneNumberInput.SendKeys(_phoneNumber);
                foreach (char c in _phoneNumber)
                {
                    phoneNumberInput.SendKeys(c.ToString());
                    Thread.Sleep(100);
                }


                Thread.Sleep(1000);
                var submitButton = driver.FindElement(By.CssSelector("button[data-e2e-id='topline-login-form__submit-button']"));
                submitButton.Click();
                //phoneNumberInput.Submit();

                //// Принимаю пользовательское соглашение
                //var acceptButton1 = wait.Until(ExpectedConditions.ElementExists(By.XPath("//button[@data-e2e-id='terms-widget-next-button']")));
                //Thread.Sleep(1000);

                //acceptButton1.Click();


                //var acceptButton2 = wait.Until(ExpectedConditions.ElementExists(By.XPath("//button[@data-e2e-id='terms-widget-next-button']")));
                //Thread.Sleep(1000);
                //acceptButton2.Click();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка авторизации: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }


        private async Task<AuthData> ReadAuthDataAsync()
        {
            using var streamReader = new StreamReader("authData.json");
            var json = await streamReader.ReadToEndAsync();
            return JsonConvert.DeserializeObject<AuthData>(json);
        }
    }
}
