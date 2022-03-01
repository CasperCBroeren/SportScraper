using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace SportScraper.Data
{
    public record SportsInfoConfig
    {
        public string[] BasicFit { get; set; }
    }
    public interface ISportInformation
    {
        Task<string[]> GetAsync(string sportSupplier);

        Task<string[]> Set(string sportSupplier, string[] items);
    }
    public class DiskSportInformation : ISportInformation
    {
        private const string pathOnDisk = "sportsinfo.json";

        public async Task<string[]> GetAsync(string sportSupplier)
        {
            var settings = await GetConfigAsync();
            return GetSportSupplier(sportSupplier, settings);
        }

        private static string[] GetSportSupplier(string sportSupplier, SportsInfoConfig settings)
        {
            return sportSupplier switch
            {
                "BasicFit" => settings.BasicFit,
                _ => Array.Empty<string>()
            };
        }
        private static async Task<SportsInfoConfig> GetConfigAsync()
        {
            using var fileStream = new FileStream(pathOnDisk, FileMode.Open, FileAccess.Read);
            return await JsonSerializer.DeserializeAsync<SportsInfoConfig>(fileStream);
        }

        public async Task<string[]> Set(string sportSupplier, string[] items)
        {
            var settings = await GetConfigAsync();
            if (sportSupplier == "BasicFit")
            {
                settings.BasicFit = items;
            }
            using var fileStream = new FileStream(pathOnDisk, FileMode.Create, FileAccess.Write);
            await JsonSerializer.SerializeAsync(fileStream, settings);
            return GetSportSupplier(sportSupplier, settings);
        }
    }
}
