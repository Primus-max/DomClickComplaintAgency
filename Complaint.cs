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
                var setFavoriteOffer = offer.FindElement(By.CssSelector("button[data-e2e-id='product-snippet-favorite']"));
                setFavoriteOffer.Click();

                Thread.Sleep(1000);

                offer.Click();
                Thread.Sleep(3000);

                // Получить список открытых вкладок
                List<string> tabs = new List<string>(_driver.WindowHandles);

                // Переключиться на новую вкладку
                _driver.SwitchTo().Window(tabs[1]);


                var element = _driver.FindElement(By.CssSelector("button[data-e2e-id='agent-show-number']"));
                element.Click();


            }
        }

        public async Task<List<IWebElement>> GetElementsAsync(IWebDriver driver)
        {
            List<IWebElement> elements = new List<IWebElement>();
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
            List<IWebElement> offersList = null;
            List<IWebElement> offersListWhithoutHeart = new List<IWebElement>();

            do
            {
                ScrollToBottom(_driver);

                try
                {
                    offersList = _driver.FindElements(By.CssSelector(".NrWKB.QSUyP")).ToList();

                    foreach (var offer in offersList)
                    {
                        var childElements = offer.FindElements(By.CssSelector("[data-e2e-id='heart-outlined-icon']"));
                        if (childElements.Count > 0)
                        {
                            offersListWhithoutHeart.AddRange(childElements);
                        }
                    }
                }
                catch (Exception) { }


                if (offersListWhithoutHeart.Count < 10)
                {
                    try
                    {
                        var showMoreButton = _driver.FindElement(By.CssSelector("button[data-e2e-id='next-offers-button']"));
                        showMoreButton.Click();
                    }
                    catch (Exception) { }

                    try
                    {
                        var loadingElement = _driver.FindElements(By.CssSelector("div[data-e2e-id='next-offers-button-lazy']"));
                        while (loadingElement.Count != 0)
                        {
                            Thread.Sleep(100);
                            loadingElement = _driver.FindElements(By.CssSelector("div[data-e2e-id='next-offers-button-lazy']"));
                        }
                    }
                    catch (Exception) { }

                }
            } while (offersListWhithoutHeart.Count < 10);



            //while (true)
            //{
            //    //try
            //    //{
            //    //    //var elementList = driver.FindElements(By.CssSelector("div[data-e2e-id='offers-list__item'] div[data-e2e-id='heart-outlined-icon']"));

            //    //    if (elementList.Count < 30)
            //    //    {
            //    //        var button = driver.FindElement(By.CssSelector("div[data-e2e-id='next-offers-button-lazy']"));
            //    //        if (button != null)
            //    //        {
            //    //            await Task.Run(() => ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({ behavior: 'smooth', block: 'end' });", button));
            //    //            await Task.Delay(1000);
            //    //        }
            //    //        else
            //    //        {
            //    //            button = driver.FindElement(By.CssSelector("button[data-e2e-id='next-offers-button']"));
            //    //            await Task.Run(() => button.Click());
            //    //            wait.Until(ExpectedConditions.StalenessOf(button));
            //    //        }
            //    //    }
            //    //    else
            //    //    {
            //    //        elements.AddRange(elementList);
            //    //    }
            //    //}
            //    //catch (Exception)
            //    //{

            //    //    throw;
            //    //}



            //}

            // выбираем 10 случайных элементов и добавляем их в новый список
            var randomElementsList = offersList.OrderBy(x => Guid.NewGuid()).Take(10).ToList();

            // возвращаем список со случайными элементами
            return randomElementsList;
        }

        public static void ScrollToBottom(IWebDriver driver)
        {
            try
            {
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                long windowHeight = (long)js.ExecuteScript("return window.innerHeight");
                long totalHeight = (long)js.ExecuteScript("return document.body.scrollHeight");
                long scrollHeight = 0;
                long scrollStep = windowHeight;

                while (scrollHeight < totalHeight)
                {
                    js.ExecuteScript($"window.scrollBy(0, {scrollStep});");
                    Thread.Sleep(700);
                    scrollHeight += scrollStep;

                    if (scrollHeight + windowHeight > totalHeight)
                    {
                        scrollStep = totalHeight - scrollHeight;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while scrolling to bottom: {ex.Message}");
            }
        }
    }
}
