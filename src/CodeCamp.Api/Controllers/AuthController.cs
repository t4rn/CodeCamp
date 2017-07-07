using CodeCamp.Api.Filters;
using CodeCamp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Threading.Tasks;

namespace CodeCamp.Api.Controllers
{
    public class AuthController :Controller
    {
        private readonly CampContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<CampUser> _sigInManager;

        public AuthController(CampContext context,
            SignInManager<CampUser> signInMgr,
            ILogger<AuthController> logger)
        {
            _context = context;
            _sigInManager = signInMgr;
            _logger = logger;
        }

        [HttpPost("api/auth/login")]
        [ValidateModel]
        public async Task<IActionResult> Login([FromBody] CredentialModel model)
        {
            try
            {
                var loginResult = await _sigInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (loginResult.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex in Login: {ex}");
            }

            return BadRequest($"Failed to login with login: {model.UserName}");
        }
    }
}
