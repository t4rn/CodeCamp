using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace CodeCamp.Api.Controllers
{
    [Route("api/[controller]")]
    public class OperationsController : Controller
    {
        private readonly IConfigurationRoot _config;
        private readonly ILogger<OperationsController> _logger;

        public OperationsController(ILogger<OperationsController> logger,
            IConfigurationRoot config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpOptions("reloadConfig")]
        public IActionResult ReloadConfiguration()
        {
            try
            {
                _config.Reload();

                return Ok("Configuration Reloaded.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex in reload configuration: {ex}");
            }

            return BadRequest("Could not reload configuration.");
        }
    }
}
