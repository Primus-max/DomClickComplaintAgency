using OpenQA.Selenium;
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

namespace DomclickComplaint
{
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");

        private UndetectedChromeDriver? _driver;
        private Uri _curRubric = new Uri("https://domclick.ru/search?deal_type=sale&category=living&offer_type=flat&offer_type=layout");
        private string? _logFileName;
        private string? _sellerName;

        private Random _randomeTimeWating = new Random();

        int _wrongComplaint = 0;
        public Complaint(string logFileName, string sellerName)
        {            
            _logFileName = logFileName;
            _sellerName = sellerName;
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
            bool IsPhoneExists = false;
            int countComplainted = 0;


            Random random = new Random();
            int randomOffers = random.Next(80, 112);

            // Находим элемент по атрибуту data-e2e-id
            IWebElement element = driver.FindElement(By.CssSelector("[data-e2e-id='offers-count']"));

            // Получаем текстовое значение элемента
            string text = element.Text;

            // Удаление букв с использованием регулярного выражения
            string digitsOnly = Regex.Replace(text, "[^0-9]", "");

            // Преобразование строки с цифрами в число
            int number = int.Parse(digitsOnly);


            // Проверяю странице на предмет предложения принять куки и кликаю если есть такая кнопка
            try
            {
                HandleCookieBanner();
            }
            catch (Exception) { }

            //List<IWebElement> _sellersCards = sellersCards;
            List<ComplaintedSellers> complaintedSellersList = new List<ComplaintedSellers>();
            HashSet<IWebElement> clickedElements = new HashSet<IWebElement>();

            ComplaintedSellers complainted = new();


            // В цикле прокручиваю страницу и собираю элементы.
            do
            {
                try
                {
                    offersList = driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).ToList();


                    foreach (var offer in offersList)
                    {
                        //IsPhoneExists = false;

                        if (clickedElements.Contains(offer))
                        {
                            continue;
                        }

                        try
                        {
                            var childWhithoutHeart = offer.FindElement(By.CssSelector("[data-e2e-id='heart-outlined-icon']"));

                            try
                            {
                                IsPhoneExists = ShowPhone(offer, wait, complaintedSellersList, ref complainted);
                                if (IsPhoneExists) continue;
                            }
                            catch (Exception)
                            {
                                _wrongComplaint++;

                                if (_wrongComplaint > randomOffers)
                                {
                                    Console.WriteLine($"Что то пошло не так, был превышен лимит попыток нажатия на кнопку {_wrongComplaint} , переподключаюсь...");
                                    //_driver.Quit();
                                    Thread.Sleep(5000);
                                    SendComplaint();
                                    return;
                                }
                                continue;
                            }

                            Thread.Sleep(_randomeTimeWating.Next(3000, 4200));

                            try
                            {
                                ReportComplaint(offer, wait, complaintedSellersList, ref complainted);
                            }
                            catch (Exception) { }

                            try
                            {
                                SubmitComplaint(complainted, wait, complaintedSellersList);
                                AddToFavorites(offer, wait);
                                clickedElements.Add(offer);
                                countComplainted++;
                            }
                            catch (Exception) { }

                            Thread.Sleep(_randomeTimeWating.Next(5000, 12000));
                        }
                        catch (Exception) { }
                    }

                    if (clickedElements.Count < randomOffers)
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
            } while (clickedElements.Count < randomOffers);

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

            var showPhoneButton = offer.FindElement(By.CssSelector("button[data-e2e-id='show-phone-button']"));

            try
            {
                sellerName = offer.FindElement(By.CssSelector(".NNu3K6"));
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", sellerName);
                complainted.NameSeller = sellerName.Text;
            }
            catch (Exception)
            {
                complainted.NameSeller = "Unknown";
            }

            var clickableshowPhoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(showPhoneButton));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", showPhoneButton);

            clickableshowPhoneButton.Click();

            Thread.Sleep(_randomeTimeWating.Next(3000, 5000));

            complainted.PhoneSeller = showPhoneButton.Text;
            //complainted.NameSeller = sellerName.Text;

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
                    Console.WriteLine("Ошибка при десериализации файла: " + ex.Message);
                }
            }

            string phone = showPhoneButton.Text;
            foreach (ComplaintedSellers complaintedSeller in complaintedSellersList)
            {
                if (complaintedSeller.PhoneSeller == phone)
                {
                    complainted = new();
                    return true;
                }
            }

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

        private void ReportComplaint(IWebElement offer, WebDriverWait wait, List<ComplaintedSellers> complaintedSellersList, ref ComplaintedSellers complainted)
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

        private void SubmitComplaint(ComplaintedSellers complainted, WebDriverWait wait, List<ComplaintedSellers> complaintedSellersList)
        {
            Thread.Sleep(_randomeTimeWating.Next(1500, 3000));
            var complaintButton = _driver.FindElement(By.CssSelector(".modal-footer-button-14-0-1"));

            ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", complaintButton);
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));
            complaintButton.Click();
            Thread.Sleep(_randomeTimeWating.Next(500, 1500));

            if (complainted.NameSeller != null && complainted.PhoneSeller != null)
            {
                string message = $"Жалоба на: {complainted.NameSeller} с номером телефона: {complainted.PhoneSeller}";
                LogManager.LogMessage(message, _logFileName);

                string fileName = "complaintedSellers.json";
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    complaintedSellersList = JsonSerializer.Deserialize<List<ComplaintedSellers>>(json);
                }

                complaintedSellersList?.Add(complainted);

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };

                string jsonString = JsonSerializer.Serialize(complaintedSellersList, options);
                File.WriteAllText(filePath, jsonString);
            }
        }
    }
}
