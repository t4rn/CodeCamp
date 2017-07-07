using CodeCamp.Api.Filters;
using CodeCamp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MyCodeCamp.Data;
using MyCodeCamp.Data.Entities;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace CodeCamp.Api.Controllers
{
    public class AuthController : Controller
    {
        private readonly CampContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly SignInManager<CampUser> _sigInManager;
        private readonly UserManager<CampUser> _userMgr;
        private readonly IPasswordHasher<CampUser> _hasher;
        private readonly IConfigurationRoot _config;

        public AuthController(CampContext context,
            SignInManager<CampUser> signInMgr,
            ILogger<AuthController> logger,
            UserManager<CampUser> userMgr,
            IPasswordHasher<CampUser> hasher,
            IConfigurationRoot config)
        {
            _context = context;
            _sigInManager = signInMgr;
            _logger = logger;
            _userMgr = userMgr;
            _hasher = hasher;
            _config = config;
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

        [ValidateModel]
        [HttpPost("api/auth/token")]
        public async Task<IActionResult> CreateToken([FromBody] CredentialModel model)
        {
            try
            {
                var user = await _userMgr.FindByNameAsync(model.UserName);
                if (user != null)
                {
                    if (_hasher.VerifyHashedPassword(user, user.PasswordHash, model.Password) == PasswordVerificationResult.Success)
                    {
                        var userClaims = await _userMgr.GetClaimsAsync(user);

                        IEnumerable<Claim> claims = new Claim[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // uniqueness for JWT
                            new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                            new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                            new Claim(JwtRegisteredClaimNames.Email, user.Email)
                        }.Union(userClaims);

                        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:Key"]));
                        SigningCredentials creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                             issuer: _config["Tokens:Issuer"],
                             audience: _config["Tokens:Audience"],
                             claims: claims,
                             expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(_config["Tokens:Expiration"])),
                             signingCredentials: creds
                            );

                        return Ok(new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        });
                    }
                }
                var loginResult = await _sigInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (loginResult.Succeeded)
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Ex in CreateToken JWT: {ex}");
            }

            return BadRequest($"Failed to create token for login: {model.UserName}");
        }
    }
}
