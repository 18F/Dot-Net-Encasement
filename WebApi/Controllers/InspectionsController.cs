using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using System.Linq;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InspectionsController : ControllerBase
    {
        private IInspectionsContext _context;

        public InspectionsController(IInspectionsContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ContentResult Get()
        {
            var inspections = _context.Inspections;
            return Content(FormatJson(inspections), "application/json");
        }

        [HttpGet("search")]
        public ContentResult Get(string grade)
        {
            var inspections = _context.Inspections
            .Where(a => a.GradeRecent == grade);
            return Content(FormatJson(inspections), "application/json");
        }

        private string FormatJson(IQueryable<Inspections> inspections)
        {
            return JsonConvert.SerializeObject(inspections);
        }
    }
}
