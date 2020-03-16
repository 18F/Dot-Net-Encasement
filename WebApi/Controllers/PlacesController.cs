using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using System.Linq;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlacesController : ControllerBase
    {
        private IPlacesContext _context; 

        public PlacesController(IPlacesContext context) {
            _context = context;
        }
        
        [HttpGet]
        public ContentResult Get()
        {
            var addresses = _context.Places;
            return Content(FormatJson(addresses), "application/json");
        }

        [HttpGet("search")]
        public ContentResult Get(string state)
        {
            var addresses = _context.Places
            .Where(a => a.State == state);
            return Content(FormatJson(addresses), "application/json");
        }

        private string FormatJson(IQueryable<Places> addresses) {
            return JsonConvert.SerializeObject(addresses);
        }
    }
}
