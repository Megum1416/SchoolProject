using Microsoft.AspNetCore.Mvc;
using SchoolProject_DB.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using SchoolProject_DB.Services;  
using System.Threading.Tasks;

namespace SchoolProject_DB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TopicScraper _topicScraper;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _topicScraper = new TopicScraper(); 
        }

        public async Task<IActionResult> Index()
        {
            
            if (User.Identity.IsAuthenticated)
            {
                var isAdmin = User.Claims.FirstOrDefault(c => c.Type == "IsAdmin")?.Value;

                if (isAdmin == "True")
                {
                    ViewBag.Layout = "_Layout"; 
                }
                else
                {
                    ViewBag.Layout = "_Layout-Member"; 
                }
            }
            else
            {
                ViewBag.Layout = "_Layout-Guest"; 
            }

            
            var latestTopics = await _topicScraper.ScrapeLatestTopicsAsync();

            
            ViewData["LatestTopics"] = latestTopics;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}