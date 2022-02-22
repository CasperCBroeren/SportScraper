using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace SportScraper.Pages
{
    public class IndexModel : PageModel
    {
        public int BasicFitCount { get; set; }
        private readonly ILogger<IndexModel> _logger;
        private readonly IResultWriter resultWriter;

        public IndexModel(ILogger<IndexModel> logger, IResultWriter resultWriter)
        {
            _logger = logger;
            this.resultWriter = resultWriter;
        }

        public void OnGet()
        {
            this.BasicFitCount = this.resultWriter.All().Count(x => x.Provider == "BasicFit");
        }
    }
}