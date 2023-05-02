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
        public async Task<UndetectedChromeDriver> GetDriverAsync()
        {
            var options = new ChromeOptions();

            var driver = UndetectedChromeDriver.Create(options = options,
                                driverExecutablePath:
                await new ChromeDriverInstaller().Auto());

            return driver;
        }
    }
}
