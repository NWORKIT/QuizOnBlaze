
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using QuizOnBlaze.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static QuizOnBlaze.DTOs.LoginDTO;


namespace QuizOnBlaze.Controllers
{
    //[ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        
        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Authentication 
        /// </summary>
        /// <param name="request">LoginDTO</param>
        /// <returns>Login response</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginRequest request)
        {
            if (request == null)
            {
                _logger.LogDebug("LoginRequest is null");
                return BadRequest("LoginRequest is null");
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                _logger.LogDebug("Password is empty");
                //return BadRequest("Password is empty");
                return Redirect("/login?error=1");
            }

            var adminPassword = _configuration["AdminSettings:AdminPassword"];

            if (request.Password == adminPassword)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, "Admin"),
                    new Claim(ClaimTypes.Role, "Admin")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                    new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    });

                return Redirect("/admin");
            }

            //return BadRequest("Incorrect password");
            return Redirect("/login?error=1");
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }

    }
}
