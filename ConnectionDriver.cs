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
                //options.AddUserProfilePreference("credentials_enable_service", false);
                //options.AddUserProfilePreference("profile.password_manager_enabled", false);
                //options.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);

                options.AddArguments("--no-sandbox", "--disable-dev-shm-usage", "--disable-notifications", "--disable-popup-blocking");

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
