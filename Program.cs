using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;

namespace DomclickComplaint
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            string? logFileName = DateTime.Now.ToString("HH-mm-ss");

            Uri uri;

            Console.WriteLine("Выберите рубрику: 1 - Квартиры, 2 - Комнаты");
            string choice = Console.ReadLine();

            if (choice == "1")
            {
                uri = new Uri("https://tomsk.domclick.ru/search?deal_type=sale&category=living&offer_type=flat&from=topline2020&address=d5883f07-6a8e-4ba2-b0de-c266d11dd0e4&aids=13667&offset=0");
            }
            else if (choice == "2")
            {
                uri = new Uri("http://example.com/rooms");
            }
            else
            {
                Console.WriteLine("Некорректный выбор!");
                return;
            }

            Console.WriteLine($"Вы выбрали рубрику: {choice}");
            Console.WriteLine($"Ссылка на рубрику: {uri}");

            Complaint complaint = new(uri, logFileName);

            complaint.SendComplaint();
        }
    }
}