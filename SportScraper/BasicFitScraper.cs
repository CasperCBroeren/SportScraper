using HtmlAgilityPack;
using Microsoft.Extensions.Hosting;
using SportScraper.Data;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SportScraper
{
    public class BasicFitScraper : BackgroundService, IBasicFitScraper
    {
        private readonly ISportInformation sportInformation;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IResultWriter resultWriter;

        public BasicFitScraper(ISportInformation sportInformation, 
            IHttpClientFactory httpClientFactory,
            IResultWriter resultWriter)
        {
            this.sportInformation = sportInformation;
            this.httpClientFactory = httpClientFactory;
            this.resultWriter = resultWriter;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await DownloadData();
                Thread.Sleep(24 * 60 * 60 * 1000);
            }             
        }

        public async Task DownloadData()
        {
            this.resultWriter.RegisterProducer("BasicFit");
            var sites = await sportInformation.GetAsync("BasicFit");
            var client = this.httpClientFactory.CreateClient();
            foreach (var site in sites)
            {
                var clubId = site.Substring(site.Length - 37, 32);
                var url = $"https://www.basic-fit.com/on/demandware.store/Sites-BFE-Site/nl_NL/Booker-Timetable?club_id={clubId}&seotitle=Sportschool%20Schiedam%20De%20Brauwweg&seosequence=13?club_id={clubId}";
                Console.WriteLine($"Scraping site: {url}");
                var body = await client.GetStringAsync(url);
                var doc = new HtmlDocument();
                doc.LoadHtml(body);
                var locationName = doc.DocumentNode.SelectSingleNode(@"//div[@class=""filter-search asset-search-block js-search-bar-form""]/input[2]");
                var timetable = doc.DocumentNode.SelectSingleNode(@"//section[@class=""timetable-component desktop""]//div[@class=""timetable""]");
                var timetableHeaders = timetable.SelectNodes(@"//div[@class=""timetable-row""]/div/p");
                var headers = ParseHeaders(timetableHeaders);

                for (int i = 0; i < headers.Length; i++)
                {
                    var day = headers[i];
                    var dayBox = timetable.SelectNodes($@"//div[@class=""timetable-col__container""]/div[{i + 1}]/div/div");
                    foreach (var cell in dayBox)
                    {
                        var name = cell.SelectSingleNode("span[1]").InnerText.Replace("\n", string.Empty);
                        var time = cell.SelectSingleNode("span[2]").InnerText.Split('-');
                        if (!string.IsNullOrWhiteSpace(name) && time.Length == 2)
                        {
                            this.resultWriter.SaveToOuput("BasicFit", new UniformGroupLesson()
                            {
                                Provider = "BasicFit",
                                Location = locationName.Attributes["value"].Value,
                                Name = name,
                                From = ConstructWithTime(day, time[0].Trim()),
                                To = ConstructWithTime(day, time[1].Trim()),
                            });
                        }
                    }

                }
            }
            this.resultWriter.ProducerEnds("BasicFit");
        }

        private DateTime ConstructWithTime(DateTime day, string hourMinutes)
        {
            try
            {
                var splited = hourMinutes.Split(':');
                var hour = int.Parse(splited[0]);
                var minutes = int.Parse(splited[1]);
                return new DateTime(day.Year, day.Month, day.Day, hour, minutes, 0);
            }
            catch(Exception exc)
            {
                return day;
            }
        }

        private DateTime[] ParseHeaders(HtmlNodeCollection timetableHeaders)
        {
            var result = new List<DateTime>();
            foreach(var node in timetableHeaders)
            {
                var content = node.InnerText.Split(',')[1].Split(' ');
                var day = int.Parse(content[1]);
                var month = MonthName(content[2 ]);
                result.Add(new DateTime(DateTime.Now.Year, month, day));
            }
            return result.ToArray();
        }

        private static int MonthName(string name)
        {
            return name switch
            {
                "Jan" => 1,
                "Feb" => 2,
                "Mar" => 3,
                "Apr" => 4,
                "May" => 5,
                "Jun" => 6,
                "Jul" => 7,
                "Aug" => 8,
                "Sep" => 9,
                "Okt" => 10,
                "Nov" => 11,
                "Dec" => 12,
                _ => DateTime.Now.Month
            };
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
          
            return Task.CompletedTask;
        }
    }

    public interface IBasicFitScraper
    {
        Task DownloadData();
    }
}
