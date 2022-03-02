using Microsoft.AspNetCore.Mvc.RazorPages;
using SportScraper.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SportScraper.Pages
{
    public class EditConfigModel : PageModel
    {
        public string TypeOfConfig { get; private set; }
        public string[] ConfigItems { get; set; }
        public string Message { get; private set; }

        private readonly ISportInformation sportInformation;
        private readonly IBasicFitScraper basicFitScraper;

        public EditConfigModel(ISportInformation sportInformation, IBasicFitScraper basicFitScraper)
        {
            this.sportInformation = sportInformation;
            this.basicFitScraper = basicFitScraper;
        }

        public async Task OnGetAsync(string type)
        {
            this.TypeOfConfig = type;
            this.ConfigItems = (await sportInformation.GetAsync(type.ToString()))
                .ToArray();
        }

        public async Task OnPostAsync(string type)
        {
            this.TypeOfConfig = type;
            var newCollection = new List<string>();
            for (var i=0; i< this.Request.Form.Count; i++)
            {
                var newItem = this.Request.Form["field-" + newCollection.Count];
                if (!string.IsNullOrWhiteSpace(newItem))
                {
                    newCollection.Add(newItem);
                }
            }
            
            this.ConfigItems = await sportInformation.Set(type, newCollection.ToArray());
            basicFitScraper.DownloadData();
            this.Message = "The config has been updated, the sites will be scraped. Go back and see the indicator change. Then download the file (top right)";
        }
    }
}