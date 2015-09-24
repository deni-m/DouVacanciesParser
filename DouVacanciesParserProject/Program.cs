using System;
using System.Diagnostics;

namespace DouVacanciesParserProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var feedsUrl = "http://jobs.dou.ua/vacancies/feeds/?cities=%D0%9A%D0%B8%D0%B5%D0%B2&category=.NET";
            var targetFile = "c:\\temp\\vacancies.txt";

            Console.WriteLine("Feed URL: " + feedsUrl);
            Console.WriteLine("Results will be saved into file: " + targetFile);

            var sw = Stopwatch.StartNew();
            var vp = new VacanciesParser();
            vp.ParseAndSave(feedsUrl, targetFile);

            Console.WriteLine($"Parsing completed. Elapsed time: {((int)sw.ElapsedMilliseconds / 1000)} seconds");
            Console.ReadLine();
        }
    }
}
