using OpenQA.Selenium;
using OpenQA.Selenium.DevTools;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Serilog;
using Serilog.Events;
using Log = Serilog.Log;

namespace DomclickComplaint
{
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");

        private UndetectedChromeDriver? _driver;
        private Uri _curRubric = new Uri("https://tomsk.domclick.ru/search?deal_type=sale&category=living&offer_type=flat&offer_type=layout&aids=13675");
        private string? _logFileName;
        private string? _sellerName;

        private Random _randomeTimeWating = new Random();

        int _wrongComplaint = 0;
        public Complaint(string logFileName, string sellerName)
        {
            _logFileName = logFileName;
            _sellerName = sellerName;

            InitLogger();
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
            Thread.Sleep(_randomeTimeWating.Next(1000, 2500));
            Authorization authorization = new();
            bool isAuthSeccess = await authorization.AuthenticateAsync(_driver);

            if (isAuthSeccess)
            {
                wait.Until(_driver => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                _driver.Navigate().GoToUrl(_curRubric);

                Thread.Sleep(_randomeTimeWating.Next(1000, 2500));

                await GetElementsAsync(_driver);

                //Send(sellersCards);

                _driver.Quit();
            }
        }

        public async Task GetElementsAsync(IWebDriver driver)
        {
            var elements = new List<IWebElement>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var offersList = new List<IWebElement>();
            var lastScrollPosition = 0;

            int countComplainted = 0;

            int totalCount = 0;
            try
            {
                // Находим элемент по атрибуту data-e2e-id
                IWebElement totalCountOffers = driver.FindElement(By.CssSelector("[data-e2e-id='offers-count']"));

                // Получаем текстовое значение элемента
                string textFromElement = totalCountOffers.Text;

                // Удаление букв с использованием регулярного выражения
                string digitsOnly = Regex.Replace(textFromElement, "[^0-9]", "");

                // Преобразование строки с цифрами в число
                totalCount = int.Parse(digitsOnly);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось полуть totalCountOffers: {ex.Message}");
            }


            // Проверяю странице на предмет предложения принять куки и кликаю если есть такая кнопка
            try
            {
                HandleCookieBanner();
            }
            catch (Exception) { }

            //List<IWebElement> _sellersCards = sellersCards;
            List<ComplaintedSellers> complaintedSellersList = new List<ComplaintedSellers>();
            HashSet<IWebElement> clickedElements = new HashSet<IWebElement>();
            IWebElement? sellerName = null;

            ComplaintedSellers complainted = new();


            // В цикле прокручиваю страницу и собираю элементы.
            do
            {
                try
                {
                    offersList = driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).ToList();
                }
                catch (Exception ex)
                {
                    Log.Error($"Не удалось получить offersList {ex.Message}");
                }


                foreach (var offer in offersList)
                {
                    //IsPhoneExists = false;

                    if (clickedElements.Contains(offer))
                    {
                        continue;
                    }


                    // Если этот класс на странице, значит имена не показываются class="RMRbUr QlOLRO"
                    try
                    {
                        sellerName = offer.FindElement(By.CssSelector(".NNu3K6"));
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Не удалось получить sellerName {ex.Message}");
                        continue;
                    }

                    string? sellerNameText = sellerName?.Text;

                    if (Equals(_sellerName, sellerNameText))
                    {

                        IWebElement? objAdress = null;
                        IWebElement? objPrice = null;
                        ComplaintedSellers complaintedSellers = new ComplaintedSellers();

                        try
                        {
                            // Получаю адресс
                            objAdress = offer.FindElement(By.CssSelector(".RMRbUr.Wzltww.ldTPKa"));
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Не удалось получить адресс объекта {ex.Message}");
                        }

                        try
                        {
                            // Получаю цену
                            objPrice = offer.FindElement(By.CssSelector("._5oAgZI.Z4r7pA"));
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Не удалось получить цену объекта {ex.Message}");
                        }


                        complaintedSellers.ObjectAdress = objAdress?.Text;
                        complaintedSellers.ObjectPrice = objPrice?.Text;

                        ReportComplaint(offer, wait);
                        SubmitComplaint(complainted);

                        countComplainted++;
                    }

                    Thread.Sleep(_randomeTimeWating.Next(5000, 12000));

                    if (clickedElements.Count < totalCount)
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
                        }
                        catch (Exception) { }

                    }
                }

            } while (clickedElements.Count < totalCount);

            Console.WriteLine($"Всего отправлено жалоб - {clickedElements.Count}");
        }

        private void HandleCookieBanner()
        {
            var cookieBtn = _driver.FindElement(By.XPath("//div[contains(@class, 'cookie-button')]"));
            cookieBtn.Click();
            Thread.Sleep(1000);
        }

        private bool ShowPhone(IWebElement offer, WebDriverWait wait, List<ComplaintedSellers> complaintedSellersList, ref ComplaintedSellers complainted)
        {
            IWebElement sellerName = null;

            var showPhoneButton = offer.FindElement(By.CssSelector("button[data-e2e-id='show-curSellerName-button']"));

            try
            {
                sellerName = offer.FindElement(By.CssSelector(".NNu3K6"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", sellerName);
            }
            catch (Exception)
            {

            }

            var clickableshowPhoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(showPhoneButton));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", showPhoneButton);

            clickableshowPhoneButton.Click();

            Thread.Sleep(_randomeTimeWating.Next(3000, 5000));

            string fileName = "complaintedSellers.json";
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

            if (File.Exists(filePath))
            {
                try
                {
                    string json = File.ReadAllText(filePath);
                    complaintedSellersList = JsonSerializer.Deserialize<List<ComplaintedSellers>>(json);
                }
                catch (Exception ex)
                {
                    Log.Error($"Ошибка при десериализации файла: {ex.Message}");
                }
            }

            //string curSellerName = sellerName.Text;
            //foreach (ComplaintedSellers complaintedSeller in complaintedSellersList)
            //{
            //    if (Equals(_sellerName, curSellerName))
            //    {
            //        complainted = new();
            //        return true;
            //    }
            //}

            return false;
        }

        private void AddToFavorites(IWebElement offer, WebDriverWait wait)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", offer);
            Thread.Sleep(_randomeTimeWating.Next(700, 1700));

            var setFavoriteOffer = offer.FindElement(By.CssSelector("button[data-e2e-id='product-snippet-favorite']"));
            var clickableSetFavoriteOffer = wait.Until(ExpectedConditions.ElementToBeClickable(setFavoriteOffer));
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            clickableSetFavoriteOffer.Click();
        }

        private void ReportComplaint(IWebElement offer, WebDriverWait wait)
        {
            Thread.Sleep(_randomeTimeWating.Next(1500, 3000));
            var complaintButton = offer.FindElement(By.CssSelector("button[data-e2e-id='snippet-complaint-button']"));

            var jsExecutor = (IJavaScriptExecutor)_driver;
            jsExecutor.ExecuteScript("arguments[0].scrollIntoView(true);", complaintButton);

            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            jsExecutor.ExecuteScript("arguments[0].dispatchEvent(new MouseEvent('mouseover', { bubbles: true }));", complaintButton);

            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            jsExecutor.ExecuteScript("arguments[0].click();", complaintButton);

            var complaintElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(@class, 'multipleButtonSelect-root') and contains(@class, 'multipleButtonSelect-root--medium')]")));

            var complaintOptions = complaintElement.FindElements(By.TagName("label"));

            var randomIndex = new Random().Next(0, complaintOptions.Count);

            var clickableComplaintOption = wait.Until(ExpectedConditions.ElementToBeClickable(complaintOptions[randomIndex]));
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            clickableComplaintOption.Click();
        }

        private void SubmitComplaint(ComplaintedSellers complainted)
        {
            Thread.Sleep(_randomeTimeWating.Next(1500, 3000));

            // data-e2e-id="heart-icon"

            var complaintButton = _driver.FindElement(By.CssSelector(".modal-footer-button-14-0-1"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", complaintButton);
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            complaintButton.Click();
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));


            string message = $"Адрес объекта: {complainted.ObjectAdress} цена объекта: {complainted.ObjectPrice}";
            LogManager.LogMessage(message, _logFileName);

        }

        private void InitLogger()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day) // Указываем имя файла и интервал для его перекрытия
            .CreateLogger();
        }
    }
}
