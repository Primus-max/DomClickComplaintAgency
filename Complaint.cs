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
using System.Threading.Tasks;

namespace DomclickComplaint
{
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");

        private UndetectedChromeDriver? _driver;
        private Uri _curRubric;
        private string? _logFileName;

        private Random _randomeTimeWating = new Random();

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
            Thread.Sleep(_randomeTimeWating.Next(1000, 2500));
            Authorization authorization = new();
            bool isAuthSeccess = await authorization.AuthenticateAsync(_driver);

            if (isAuthSeccess)
            {
                wait.Until(_driver => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                _driver.Navigate().GoToUrl(_curRubric);

                Thread.Sleep(_randomeTimeWating.Next(1000, 2500));

                List<IWebElement> sellersCards = await GetElementsAsync(_driver);

                Send(sellersCards);

                _driver.Quit();
            }
        }

        public void Send(List<IWebElement> sellersCards)
        {
            List<IWebElement> _sellersCards = sellersCards;
            List<ComplaintedSellers> complaintedSellersList = new List<ComplaintedSellers>();
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            ComplaintedSellers complainted = new();

            foreach (var offer in _sellersCards)
            {

                // Нажимаю на кнопку "Показать телефон"
                try
                {
                    // Нахожу элементы на странице
                    var showPhoneButton = offer.FindElement(By.CssSelector("button[data-e2e-id='show-phone-button']"));
                    var sellerName = offer.FindElement(By.CssSelector(".NNu3K6"));

                    // Прокручиваю до элемента sellerName
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", sellerName);

                    // Ожидаю, пока showPhoneButton станет кликабельным
                    var clickableshowPhoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(showPhoneButton));

                    // Прокручиваю до элемента showPhoneButton
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", showPhoneButton);

                    // Нажимаю на кнопку
                    clickableshowPhoneButton.Click();


                    Thread.Sleep(_randomeTimeWating.Next(3000, 5000));

                    // Добавляю данные для записи в файл Json (база данных)
                    complainted.PhoneSeller = showPhoneButton.Text;
                    complainted.NameSeller = sellerName.Text;

                    // Проверяю, если такой телефон уже есть в базе, то перехожу к следующему
                    string fileName = "complaintedSellers.json";
                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                    bool phoneExists = false;

                    if (File.Exists(filePath))
                    {
                        string json = File.ReadAllText(filePath);

                        foreach (ComplaintedSellers complaintedSeller in complaintedSellersList)
                        {
                            if (complaintedSeller.PhoneSeller == showPhoneButton.Text)
                            {
                                complainted = new();

                                phoneExists = true;
                                break;
                            }
                        }
                    }
                    if (phoneExists) continue;

                }
                catch (Exception) { }

                Thread.Sleep(_randomeTimeWating.Next(500, 1000));


                // Получаем кнопку "Добавить в избранное" и жмем
                try
                {
                    // Прокручиваю страницу к элементу
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", offer);

                    Thread.Sleep(_randomeTimeWating.Next(700, 1700));
                    var setFavoriteOffer = offer.FindElement(By.CssSelector("button[data-e2e-id='product-snippet-favorite']"));
                    var clickableSetFavoriteOffer = wait.Until(ExpectedConditions.ElementToBeClickable(setFavoriteOffer));
                    Thread.Sleep(_randomeTimeWating.Next(500, 1500));
                    clickableSetFavoriteOffer.Click();
                }
                catch (Exception) { }

                Thread.Sleep(_randomeTimeWating.Next(3000, 4200));
                // Нажимаю кнопку "Пожаловаться"
                try
                {
                    var complaintButton = offer.FindElement(By.CssSelector("button[data-e2e-id='snippet-complaint-button']"));

                    // смещаем курсор на offer, чтобы появилась кнопка жалобы
                    var actions = new Actions(_driver);
                    actions.MoveToElement(offer).Perform();

                    Thread.Sleep(_randomeTimeWating.Next(500, 1500));

                    complaintButton.Click();
                }
                catch (Exception) { }

                // Выбираем рандомуню кнопку для жалобы
                try
                {
                    Thread.Sleep(_randomeTimeWating.Next(1500, 3100));
                    //var complaintElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector(".multipleButtonSelect-root-7-0-1.multipleButtonSelect-root--medium-7-0-1")));

                    var complaintElement = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//div[contains(@class, 'multipleButtonSelect-root') and contains(@class, 'multipleButtonSelect-root--medium')]")));

                    var complaintOptions = complaintElement.FindElements(By.TagName("label"));

                    var randomIndex = new Random().Next(0, complaintOptions.Count);

                    var clickableComplaintOption = wait.Until(ExpectedConditions.ElementToBeClickable(complaintOptions[randomIndex]));
                    Thread.Sleep(_randomeTimeWating.Next(500, 1500));
                    clickableComplaintOption.Click();
                }
                catch (Exception) { }



                try
                {
                    // Получаю кнопку "Пожаловаться" 
                    var complaintButton = _driver.FindElement(By.CssSelector(".modal-footer-button-14-0-1"));
                    //var complaintButton = _driver.FindElement(By.XPath("//div[contains(@class, 'modal-footer-button')]"));


                    // Прокручиваю страницу до кнопки "Пожаловаться"
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", complaintButton);
                    Thread.Sleep(_randomeTimeWating.Next(500, 1500));
                    complaintButton.Click();
                    Thread.Sleep(_randomeTimeWating.Next(500, 1500));



                    if (complainted.NameSeller != null && complainted.PhoneSeller != null)
                    {  // Записываю в лог                    
                        string message = $"Жалоба на: {complainted.NameSeller} с номером телефона: {complainted.PhoneSeller}";
                        LogManager.LogMessage(message, _logFileName);

                        // Записываю в json (база данных)
                        string fileName = "complaintedSellers.json";
                        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);

                        if (File.Exists(filePath))
                        {
                            string json = File.ReadAllText(filePath);
                            complaintedSellersList = JsonSerializer.Deserialize<List<ComplaintedSellers>>(json);
                        }

                        complaintedSellersList.Add(complainted);

                        JsonSerializerOptions options = new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            WriteIndented = true
                        };

                        string jsonString = JsonSerializer.Serialize(complaintedSellersList, options);
                        File.WriteAllText(filePath, jsonString);
                    }


                    complainted = new();
                }
                catch (Exception) { }

                Thread.Sleep(_randomeTimeWating.Next(5000, 12000));
            }
        }

        public async Task<List<IWebElement>> GetElementsAsync(IWebDriver driver)
        {
            var elements = new List<IWebElement>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            var offersListWhithoutHeart = new List<IWebElement>();
            var lastScrollPosition = 0;

            Random randomOffers = new Random();

            // В цикле прокручиваю страницу и собираю элементы.
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
            var randomElementsList = driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).OrderBy(x => Guid.NewGuid()).Take(randomOffers.Next(9, 16)).ToList();

            // Возвращаем список со случайными элементами
            return randomElementsList;
        }
    }
}
