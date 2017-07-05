using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeCamp.Controllers
{
    [Route("api/[controller]")]
    public class CampsController : Controller
    {
        [HttpGet("")]
        public IActionResult Get()
        {
            return Ok(new { Name = "Kris", Color = "Blue" });
        }
    }
}
