using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SportScraper
{
    public class BasicFitScraper : IHostedService
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IResultWriter resultWriter;

        public BasicFitScraper(IConfiguration configuration, 
            IHttpClientFactory httpClientFactory,
            IResultWriter resultWriter)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.resultWriter = resultWriter;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            this.resultWriter.RegisterProducer();
            var sites = configuration.GetSection("basicfit").GetChildren();
            var client = this.httpClientFactory.CreateClient();
            foreach (var site in sites)
            {
                var url = site.Value;
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
                            this.resultWriter.SaveToOuput(new UniformGroupLesson()
                            {
                                Provider = "BasicFit",
                                Location = locationName.Attributes["value"].Value,
                                Name = name,
                                From = ConstructWithTime(day, time[0].Trim()),
                                To = ConstructWithTime(day, time[1].Trim()),
                            });
                        }
                        else
                        {
                            Console.WriteLine($"Not written {name} and {time}");
                        }
                    }

                }
            }
            this.resultWriter.ProducerEnds();
            await this.StopAsync(cancellationToken);
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

        private DateTime[] parseHeaders(string headers)
        {
            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
          
            return Task.CompletedTask;
        }
    }
}
