using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Omar.Dtos.ApplicationUserDto;
using Omar.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Omar.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AccountController(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost("addEmployee")]
        // [Authorize(Roles = "Owner")]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Assign Role
            await _userManager.AddToRoleAsync(user, "Employee");

            return Ok(new { Message = "Employee registered successfully" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { Message = "Invalid email or password" });
            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
                return Unauthorized(new { Message = "Account is blocked" });
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var token = await GenerateTokenAsync(user);
                return Ok(new { Token = token });
            }
            return Unauthorized(new { Message = "Invalid email or password" });
        }
        [HttpPost("blockedEmployee")]
        public async Task<IActionResult> BlockEmployee(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "Employee not found" });
            user.LockoutEnd = DateTime.UtcNow.AddYears(100); // Block for a long time
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok(new { Message = "Employee blocked successfully" });
        }
        [HttpPost("unblockEmployee")]
        public async Task<IActionResult> UnblockEmployee(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return NotFound(new { Message = "Employee not found" });
            user.LockoutEnd = null; // Unblock
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);
            return Ok(new { Message = "Employee unblocked successfully" });
        }
        private async Task<string> GenerateTokenAsync(ApplicationUser user)
        {
            try
            {
                var tokenHeader = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(_configuration["JWTSetting:securityKey"]!);
                var roles = await _userManager.GetRolesAsync(user);
                List<Claim> claims = new List<Claim>
                    {
                    new Claim(ClaimTypes.NameIdentifier,user.Id),
                    new Claim(ClaimTypes.Email,user.Email)
                };
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                var token = tokenHeader.CreateToken(tokenDescriptor);
                return tokenHeader.WriteToken(token);
            }
            catch (Exception ex)
            {
                // Log the exception (you can use a logging framework like Serilog, NLog, etc.) 
                Console.WriteLine($"Error generating token: {ex.Message}");
                throw; // Rethrow the exception to be handled by the calling method
            }
        }
    }
}
