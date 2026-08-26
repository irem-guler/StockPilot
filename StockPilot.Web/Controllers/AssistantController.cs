using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockPilot.Web.Services;

namespace StockPilot.Web.Controllers
{
    [Authorize]
    public class AssistantController : Controller
    {
        private readonly AssistantService _assistantService;

        public AssistantController(AssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ask([FromBody] AskRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Question))
            {
                return Json(new { answer = "Please type a question." });
            }

            var answer = await _assistantService.AskAsync(request.Question);
            return Json(new { answer });
        }

        public class AskRequest
        {
            public string Question { get; set; } = string.Empty;
        }
    }
}