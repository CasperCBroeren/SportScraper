using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SportScraper.Pages
{
    public class EditConfigModel : PageModel
    {
        public string TypeOfConfig { get; private set; }
        public string[] ConfigItems { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private readonly IConfiguration configuration;

        public EditConfigModel(ILogger<IndexModel> logger, IConfiguration configuration)
        {
            _logger = logger;
            this.configuration = configuration;
        }

        public void OnGet(string type)
        {
            this.TypeOfConfig = type;
            this.ConfigItems = configuration.GetSection(type.ToLower())
                .GetChildren()
                .ToList()
                .Select(x => x.Value)
                .ToArray();
        }

        public void OnPost(string type)
        {

            this.TypeOfConfig = type;
            var newCollection = configuration.GetSection(type.ToLower())
                .GetChildren()
                .ToList()
                .Select(x => x.Value)
                .ToList();

            var newItem = this.Request.Form["field-" + newCollection.Count];
            newCollection.Add(newItem);
            configuration.AddToSection(type, newCollection.ToArray());
            this.ConfigItems = newCollection.ToArray();
        }
    }
}