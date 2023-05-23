using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using System.Threading;

namespace DomclickComplaint
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            int delayProgram = SetTimeDelay();
            Uri uri = ChooseCategory();

            while (true)
            {
                try
                {
                    // Запуск программы
                    LaunchComplaint(uri);

                    // Задержка между сессиями работы программы
                    Thread.Sleep(delayProgram);
                }
                catch (Exception) { }
            }
        }

        // Объединяющий метод для запуска программы
        static void LaunchComplaint(Uri uri)
        {
            string logFileName = DateTime.Now.ToString("HH-mm-ss") + ".txt";

            Complaint complaint = new Complaint(uri, logFileName);

            complaint.SendComplaint();
        }

        // Метод выбора категории
        static Uri ChooseCategory()
        {
            Console.WriteLine("Выберите рубрику:");
            Console.WriteLine("  1 - Квартиры");
            Console.WriteLine("  2 - Комнаты");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                return new Uri("https://tomsk.domclick.ru/search?deal_type=sale&category=living&offer_type=flat&from=topline2020&address=d5883f07-6a8e-4ba2-b0de-c266d11dd0e4&aids=13667&offset=0");
            }
            else if (choice == "2")
            {
                return new Uri("https://tomsk.domclick.ru/search?category=living&deal_type=sale&offer_type=room&from=topline2020");
            }
            else
            {
                Console.WriteLine("Некорректный выбор! Попробуйте еще раз.");
                return ChooseCategory();
            }
        }

        // Время задержки запуска программы в часах
        public static int SetTimeDelay()
        {
            int delayMilliseconds = 0;

            while (delayMilliseconds <= 0)
            {
                Console.WriteLine("Введите время задержки работы программы в часах:");

                string input = Console.ReadLine();

                if (!int.TryParse(input, out int delayHours))
                {
                    Console.WriteLine("Вы ввели не число. Попробуйте еще раз.");
                    continue;
                }

                if (delayHours <= 0)
                {
                    Console.WriteLine("Время задержки должно быть положительным числом. Попробуйте еще раз.");
                    continue;
                }

                delayMilliseconds = delayHours * 60 * 60 * 1000;
            }

            return delayMilliseconds;
        }
    }
}
