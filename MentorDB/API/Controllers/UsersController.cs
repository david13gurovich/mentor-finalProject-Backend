using Domain.In;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersService _service;
        private readonly IConfiguration _configuration;

        public UsersController(UsersService service, IConfiguration config)
        {
            _service = service;
            _configuration = config;
        }

        // POST: api/Users
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostUser(InUser user)
        {
            bool? res = await _service.AddUser(new User() { Id = user.Id, Name = user.Name, Password = user.Password, Email = user.Email});

            if (res == null)
            {
                return Problem("Entity set 'MentorDataContext.User'  is null.");
            }
            else if (res == false)
            {
                return Conflict();
            }

            return Ok();
        }

        [HttpPost("Login")]
        public async Task<IActionResult> LoginUser(LogUser user)
        {
            bool? res = await _service.ValidateUser(user);
            if (res == null)
            {
                return Problem("Entity set 'MentorDataContext.User'  is null.");

            }
            else if (res == false)
            {
                return NotFound();

            }
            string token = Signin(user);
            InToken inToken = new InToken() { Token = token };
            return Ok(inToken);
        }


        private string Signin(LogUser account)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, account.Id),
                new Claim(JwtRegisteredClaimNames.Sub, _configuration["JWTParams:Subject"]),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString())
            };

            var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWTParams:SecretKey"]));
            var mac = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["JWTParams:Issuer"],
                _configuration["JWTParams:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(20),
                signingCredentials: mac);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser()
        {
            string userId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID not found.");
            }

            bool? res = await _service.DeleteUser(userId);
            if (res == null || res == false)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
}
