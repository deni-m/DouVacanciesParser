using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using DouVacanciesParserProject.Models;
using HtmlAgilityPack;

namespace DouVacanciesParserProject
{
    public class VacanciesParser
    {
        /// <summary>
        /// Parse vacancies and save it to text file
        /// </summary>
        /// <param name="feedsUrl">Something like http://jobs.dou.ua/vacancies/feeds/?cities=%D0%9A%D0%B8%D0%B5%D0%B2&category=.NET </param>
        /// <param name="targetFilename">full file name where to save parser results</param>
        public void ParseAndSave(string feedsUrl, string targetFilename)
        {
            var links = GetVacanciesLinks(feedsUrl);
            var vacancies = new List<Vacancy>();

            foreach (var link in links)
            {
                var vacancyPage = GetVacancyPage(link);
                var vacancy = ParseVacancy(vacancyPage);

                vacancies.Add(vacancy);
            }

            SaveVacancies(vacancies, targetFilename);
        }

        private Vacancy ParseVacancy(string vacancyPage)
        {
            var result = new Vacancy();

            var html = new HtmlDocument();
            html.LoadHtml(vacancyPage);

            var vacancyRoot = html.DocumentNode.SelectSingleNode("//div[@class='b-vacancy']");
            result.CompanyName =
                vacancyRoot.SelectSingleNode(".//div[@class='b-compinfo']//div[@class='l-n']/a").InnerText;
            result.Title = vacancyRoot.SelectSingleNode(".//h1[@class='g-h2']").InnerText;


            var descriptionRoot = vacancyRoot.SelectSingleNode("./div[@class='l-vacancy']");
            result.Link = descriptionRoot.SelectSingleNode("./div[contains(@class, 'social-likes')]")
                          .Attributes.SingleOrDefault(x => x.Name.Equals("data-url"))
                          .Value;

            var projectdescriptionNode = descriptionRoot.SelectSingleNode("./div[@class='project']/div//p");
            result.ProjectDescription = projectdescriptionNode != null ? projectdescriptionNode.InnerText : string.Empty;

            var sections = descriptionRoot.SelectNodes("./div[@class='vacancy-section']");

            if (sections.Count > 1)
            {
                foreach (var section in sections)
                {
                    var title = section.SelectSingleNode("./h3").InnerText;
                    var content = section.SelectSingleNode("./div//p").InnerText;

                    if (title.ToLower().Contains("необходимые навыки"))
                    {
                        result.SectionRequires = content;
                    }
                    else if (title.ToLower().Contains("будет плюсом"))
                    {
                        result.SectionWillBeAPluss = content;
                    }
                    else if (title.ToLower().Contains("предлагаем"))
                    {
                        result.SectionOffer = content;
                    }
                    else if (title.ToLower().Contains("обязанности"))
                    {
                        result.SectionResponsibility = content;
                    }
                }
            }

            return result;
        }

        private string GetVacancyPage(string url)
        {
            string result;

            using (var wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                result = wc.DownloadString(url);
            }

            return result;
        }

        private List<string> GetVacanciesLinks(string feedsUrl)
        {
            var reader = XmlReader.Create(feedsUrl);
            var feed = SyndicationFeed.Load(reader);
            reader.Close();

            return feed.Items.Select(item => item.Id).ToList();
        }

        private void SaveVacancies(List<Vacancy> vacancies, string filePath)
        {
            using (var file = File.CreateText(filePath))
            {
                foreach (var vacancy in vacancies.OrderBy(vac => vac.CompanyName))
                {
                    file.WriteLine("--------------------------------------------------------------------");
                    file.WriteLine(vacancy.CompanyName);
                    file.WriteLine(vacancy.Title);

                    if (!string.IsNullOrEmpty(vacancy.SectionRequires))
                    {
                        var lines = vacancy.SectionRequires.Split('—', '•', '*');
                        foreach (var line in lines)
                        {
                            file.WriteLine(line.Replace("\t", " "));
                        }
                    }
                }
            }
        }
    }
}