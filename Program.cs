using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;

namespace DomclickComplaint
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            while (true)
            {
                try
                {
                    LaunchComplaint();

                    Thread.Sleep(TimeSpan.FromHours(12));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"При запуске программы произошла ошибка: {ex.Message}");
                }
            }
        }

        static void LaunchComplaint()
        {
            string? logFileName = DateTime.Now.ToString("HH-mm-ss") + ".txt";

            Uri uri = ChooseCategory();

            Complaint complaint = new(uri, logFileName);

            complaint.SendComplaint();
        }

        // Метод выбора категории
        static Uri ChooseCategory()
        {
            Uri uri;

            while (true)
            {
                Console.WriteLine("Выберите рубрику:");
                Console.WriteLine("  1 - Квартиры");
                Console.WriteLine("  2 - Комнаты");

                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    uri = new Uri("https://tomsk.domclick.ru/search?deal_type=sale&category=living&offer_type=flat&from=topline2020&address=d5883f07-6a8e-4ba2-b0de-c266d11dd0e4&aids=13667&offset=0");
                    break;
                }
                else if (choice == "2")
                {
                    uri = new Uri("https://tomsk.domclick.ru/search?category=living&deal_type=sale&offer_type=room&from=topline2020");
                    break;
                }
                else
                {
                    Console.WriteLine("Некорректный выбор! Попробуйте еще раз.");
                }
            }

            return uri;
        }
    }
}