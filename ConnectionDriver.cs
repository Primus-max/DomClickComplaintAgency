using OpenQA.Selenium.Chrome;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomclickComplaint
{
    public class ConnectionDriver
    {
        UndetectedChromeDriver driver;
        public async Task<UndetectedChromeDriver> GetDriverAsync()
        {

            try
            {
                var options = new ChromeOptions();

                driver = UndetectedChromeDriver.Create(options = options,
                                    driverExecutablePath:
                    await new ChromeDriverInstaller().Auto());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось подключиться к драйверу по причине: {ex.Message}");
                return null;
            }

            return driver;
        }
    }
}
