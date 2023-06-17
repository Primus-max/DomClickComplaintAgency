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
            string sellerName = ChooseSellerForComplaint();
            Uri uri = new Uri("https://domclick.ru/");

            while (true)
            {
                try
                {
                    // Запуск программы
                    LaunchComplaint(uri, sellerName);

                    // Задержка между сессиями работы программы
                    Thread.Sleep(delayProgram);
                }
                catch (Exception) { }
            }
        }

        // Объединяющий метод для запуска программы
        static void LaunchComplaint(Uri uri, string sellerName)
        {
            string logFileName = DateTime.Now.ToString("HH-mm-ss") + ".txt";

            Complaint complaint = new Complaint(logFileName, sellerName);

            complaint.SendComplaint();
        }

        // Метод выбора категории
        static string ChooseSellerForComplaint()
        {
            Console.WriteLine("Введите номер телефона в таком формате +7 (909) 548-60-37:");
            string sellerPhone = Console.ReadLine();

            while (string.IsNullOrEmpty(sellerPhone)) 
            {                
                ChooseSellerForComplaint();
            }
            
            return sellerPhone.Trim();
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
