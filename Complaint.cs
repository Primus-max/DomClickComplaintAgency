using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DomclickComplaint
{
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");

        private UndetectedChromeDriver? _driver;
        private Uri _curRubric;
        private string? _logFileName;


        public Complaint(Uri UrlRubric, string logFileName)
        {
            _curRubric = UrlRubric;
            _logFileName = logFileName;
        }

        public async void SendComplaint()
        {
            // Получаем драйвер
            ConnectionDriver connection = new();
            _driver = await connection.GetDriverAsync();

            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30)); // Ожидание для всего класса

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
                wait.Until(_driver => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                _driver.Navigate().GoToUrl(_curRubric);

                List<IWebElement> sellersCards = await GetElementsAsync(_driver);

                Send(sellersCards);
            }
        }

        public void Send(List<IWebElement> sellersCards)
        {
            List<IWebElement> _sellersCards = sellersCards;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            ComplaintedSellers complainted = new();

            foreach (var offer in _sellersCards)
            {
                // Получаем кнопку "Добавить в избранное" и жмем
                try
                {
                    // Прокручиваю страницу к элементу
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", offer);

                    var setFavoriteOffer = offer.FindElement(By.CssSelector("button[data-e2e-id='product-snippet-favorite']"));
                    var clickableShowMoreButton = wait.Until(ExpectedConditions.ElementToBeClickable(setFavoriteOffer));
                    clickableShowMoreButton.Click();
                }
                catch (Exception) { }

                Thread.Sleep(1500);

                // Нажимаю на кнопку "Показать телефон"
                try
                {
                    var showPhoneButton = offer.FindElement(By.CssSelector("button[data-e2e-id='show-phone-button']"));
                    var sellerName = offer.FindElement(By.CssSelector(".NNu3K6"));

                    // Добавляю данные для записи в логи и добавления в файл Json
                    complainted.PhoneSeller = showPhoneButton.Text;
                    complainted.NameSeller = sellerName.Text;

                    string fileName = "complaintedSellers.json";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                    if (!File.Exists(filePath))
                    {
                        File.Create(filePath).Close();
                    }

                    string jsonString = JsonSerializer.Serialize(complainted);

                    File.AppendAllText(filePath, jsonString + Environment.NewLine);


                    Thread.Sleep(1000);

                    var clickableshowPhoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(showPhoneButton));
                    clickableshowPhoneButton.Click();
                }
                catch (Exception) { }


                Thread.Sleep(3000);
                // Нажимаю кнопку "Пожаловаться"
                try
                {
                    var complaintButton = offer.FindElement(By.CssSelector("button[data-e2e-id='snippet-complaint-button']"));

                    // смещаем курсор на offer, чтобы появилась кнопка жалобы
                    var actions = new Actions(_driver);
                    actions.MoveToElement(offer).Perform();

                    Thread.Sleep(1000);

                    complaintButton.Click();
                }
                catch (Exception) { }

                // Выбираем рандомуню кнопку для жалобы
                try
                {
                    Thread.Sleep(1500);
                    var complaintElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".multipleButtonSelect-root-6-2-3.multipleButtonSelect-root--medium-6-2-3")));

                    var complaintOptions = complaintElement.FindElements(By.TagName("label"));

                    var randomIndex = new Random().Next(0, complaintOptions.Count);

                    var clickableComplaintOption = wait.Until(ExpectedConditions.ElementToBeClickable(complaintOptions[randomIndex]));
                    clickableComplaintOption.Click();
                }
                catch (Exception) { }


                // Получаю кнопку "Пожаловаться"
                try
                {
                    var complaintButton = _driver.FindElement(By.CssSelector(".modal-footer-button-12-1-1"));
                    var actions = new Actions(_driver);
                    actions.MoveToElement(complaintButton).Perform();
                    Thread.Sleep(1000);
                    complaintButton.Click();
                    Thread.Sleep(1000);
                }
                catch (Exception) { }
            }
        }

        public async Task<List<IWebElement>> GetElementsAsync(IWebDriver driver)
        {
            var elements = new List<IWebElement>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var offersListWhithoutHeart = new List<IWebElement>();
            var lastScrollPosition = 0;

            // В цикле прокручиваю странице и собираю элементы, выдаю случайные 10 элементов.
            do
            {
                try
                {
                    var offersList = driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).ToList();

                    foreach (var offer in offersList)
                    {
                        var childElements = offer.FindElements(By.CssSelector("[data-e2e-id='heart-outlined-icon']"));
                        if (childElements.Count > 0)
                        {
                            offersListWhithoutHeart.AddRange(childElements);
                        }
                    }

                    if (offersListWhithoutHeart.Count < 30)
                    {
                        var lastOfferElement = offersList.Last();
                        var lastOfferPositionY = lastOfferElement.Location.Y + 230;
                        var scrollStep = 230;
                        lastScrollPosition = lastScrollPosition == 0 ? scrollStep : lastScrollPosition;
                        while (lastScrollPosition < lastOfferPositionY)
                        {
                            lastScrollPosition += scrollStep;
                            var js = $"window.scrollTo(0, {lastScrollPosition});";
                            ((IJavaScriptExecutor)driver).ExecuteScript(js);
                            Thread.Sleep(400);
                        }

                        try
                        {
                            // Если есть кнопка "Показать еще" докручиваем до нее и кликаем.
                            var showMoreButton = driver.FindElement(By.CssSelector("button[data-e2e-id='next-offers-button']"));
                            while (!showMoreButton.Displayed || !showMoreButton.Enabled)
                            {
                                var js = "window.scrollTo(0, document.body.scrollHeight);";
                                ((IJavaScriptExecutor)driver).ExecuteScript(js);
                                Thread.Sleep(1000);
                            }

                            var clickableShowMoreButton = wait.Until(ExpectedConditions.ElementToBeClickable(showMoreButton));
                            clickableShowMoreButton.Click();
                        }
                        catch (Exception) { }

                        try
                        {
                            // Елсли есть индиктор динамической подгрузки страницы, пролистывам к нему и ждем
                            var loadingElement = driver.FindElement(By.CssSelector("div[data-e2e-id='next-offers-button-lazy']"));
                            while (!loadingElement.Displayed)
                            {
                                var js = "window.scrollTo(0, document.body.scrollHeight);";
                                ((IJavaScriptExecutor)driver).ExecuteScript(js);
                                Thread.Sleep(1000);
                            }

                            var clickableLoadingElement = wait.Until(ExpectedConditions.ElementToBeClickable(loadingElement));
                            //clickableLoadingElement.Click();
                        }
                        catch (Exception) { }

                    }
                }
                catch (Exception) { }
            } while (offersListWhithoutHeart.Count < 30);

            // Выбираем 10 случайных элементов и добавляем их в новый список
            var randomElementsList = driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).OrderBy(x => Guid.NewGuid()).Take(10).ToList();

            // Возвращаем список со случайными элементами
            return randomElementsList;
        }
    }
}
