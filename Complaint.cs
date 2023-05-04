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
    public class Complaint
    {
        Uri baseUri = new("https://domclick.ru");
        Uri regionUri = new("https://tomsk.domclick.ru");
        Uri regionBuyFlat = new("https://tomsk.domclick.ru/search?deal_type=sale&category=living&offer_type=flat&from=topline2020&address=d5883f07-6a8e-4ba2-b0de-c266d11dd0e4&aids=13667&offset=0");
        private UndetectedChromeDriver? _driver;

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
                // Перехожу на страницу >> Томск(Регионы) >> Купить >> Квартиры
                wait.Until(_driver => ((IJavaScriptExecutor)_driver).ExecuteScript("return document.readyState").Equals("complete"));
                _driver.Navigate().GoToUrl(regionBuyFlat);

                List<IWebElement> sellersCards = await GetElementsAsync(_driver);

                Send(sellersCards);


                //// Открыть новую вкладку
                //_driver.ExecuteScript("window.open();");

                //// Получить список открытых вкладок
                //List<string> tabs = new List<string>(_driver.WindowHandles);

                //// Переключиться на новую вкладку
                //_driver.SwitchTo().Window(tabs[1]);

            }
        }

        public void Send(List<IWebElement> sellersCards)
        {
            List<IWebElement> _sellersCards = sellersCards;
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(30));

            foreach (var offer in _sellersCards)
            {
                // Получаем кнопку "Добавить в избранное" и жмем
                try
                {
                    var setFavoriteOffer = offer.FindElement(By.CssSelector("button[data-e2e-id='product-snippet-favorite']"));
                    var clickableShowMoreButton = wait.Until(ExpectedConditions.ElementToBeClickable(setFavoriteOffer));
                    clickableShowMoreButton.Click();
                }
                catch (Exception) { }

                Thread.Sleep(1000);

                // Перехожу в карточку продавца
                offer.Click();
                Thread.Sleep(3000);
                // Получаю список открытых вкладок
                List<string> tabs = new List<string>(_driver.WindowHandles);

                // Переключаюсь на новую вкладку
                _driver.SwitchTo().Window(tabs[1]);


                Thread.Sleep(5000);
                // Нажимаю на кнопку "Показать телефон"
                var showPhoneButton = _driver.FindElement(By.CssSelector("button[data-e2e-id='agent-show-number']"));
                var clickableshowPhoneButton = wait.Until(ExpectedConditions.ElementToBeClickable(showPhoneButton));
                clickableshowPhoneButton.Click();


                // Прокручиваем до кнопки "Пожаловаться"
                try
                {
                    var complaintButton = _driver.FindElement(By.CssSelector("button[data-e2e-id='complaint_button']"));
                    var step = 200;
                    var currentScroll = 0;

                    while (!IsElementVisible(complaintButton))
                    {
                        currentScroll += step;
                        var js = $"window.scrollTo(0, {currentScroll});";
                        ((IJavaScriptExecutor)_driver).ExecuteScript(js);
                        Thread.Sleep(500);
                    }

                    complaintButton.Click();

                    bool IsElementVisible(IWebElement element)
                    {
                        return element.Displayed && element.Enabled;
                    }

                }
                catch (Exception) { }

                // Выбираем рандомуню кнопку для жалобы
                try
                {
                    var complaintElement = wait.Until(ExpectedConditions.ElementIsVisible(By.CssSelector("div[data-e2e-id='complaint-single-choice']")));

                    var complaintOptions = wait.Until(ExpectedConditions.PresenceOfAllElementsLocatedBy(By.CssSelector("div[data-e2e-id='complaint-single-choice'] *")));

                    var randomIndex = new Random().Next(0, complaintOptions.Count);

                    var clickableComplaintOption = wait.Until(ExpectedConditions.ElementToBeClickable(complaintOptions[randomIndex]));
                    clickableComplaintOption.Click();
                }
                catch (Exception) { }


                // Получаю кнопку "Пожаловаться"
                try
                {
                    var sendButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("button[data-e2e-id='complaint_send_button']")));
                    // Кликаем на кнопку
                    sendButton.Click();
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
