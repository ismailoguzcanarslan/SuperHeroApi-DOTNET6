using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using SuperHeroApi.Data;
using SuperHeroApi.Migrations;
using SuperHeroApi.Models;
using SuperHeroApi.Resources.Localization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using User = SuperHeroApi.Models.User;

namespace SuperHeroApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IStringLocalizer<Localization> _stringLocalizer;

        public TokenController(IConfiguration configuration, DataContext context, IStringLocalizer<Localization> stringLocalizer)
        {
            _configuration = configuration;
            _context = context;
            _stringLocalizer = stringLocalizer; 
        }

        [HttpPost]
        [Route("register")]
        public async Task<ActionResult> Register(UserInfo userInfo)
        {
            var existedUser = await GetUser(userInfo.Email);

            if (existedUser != null)
                return BadRequest(_stringLocalizer.GetString("userexist").Value);

            CreatePasswordHash(userInfo.Password, out byte[] passwordHash, out byte[] passwordSalt);

            User user = new User();
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            user.Name = userInfo.UserName;
            user.Email = userInfo.Email;
            user.Domain = userInfo.Email.Split('@').First();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(_stringLocalizer.GetString("register").Value);
        }

        [HttpPost]
        [Route("login")]
        public async Task<ActionResult> Login(UserLogin userLogin)
        {
            var existedUser = await GetUser(userLogin.Email);

            if (existedUser == null)
                return BadRequest(_stringLocalizer.GetString("usernotexist").Value);

            if (!VerifyPasswordHash(userLogin.Password, existedUser.PasswordHash, existedUser.PasswordSalt))
                return BadRequest(_stringLocalizer.GetString("wrongpassword").Value);

            string token = CreateToken(existedUser);

            var refreshToken = GenerateRefreshToken();
            SetRefreshToken(refreshToken);

            TokenDto tokenDto = new TokenDto(token, refreshToken.Token);
            
            return Ok(tokenDto);
        }

        #region Private Methods
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private async Task<User> GetUser(string email)
        {
            var user = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();
            return user;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            var claims = new[] {
                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
                        new Claim("UserId", user.Id.ToString()),
                        new Claim("DisplayName", user.Name),
                        new Claim("UserName", user.Domain),
                        new Claim("Email", user.Email)
                    };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Audience"],
                claims,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: signIn);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshToken()
        {
            var refreshToken = new RefreshToken
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                Expires = DateTime.Now.AddDays(7),
                Created = DateTime.Now
            };

            return refreshToken;
        }

        private void SetRefreshToken(RefreshToken newRefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newRefreshToken.Expires
            };
            Response.Cookies.Append("refreshToken", newRefreshToken.Token, cookieOptions);
        }
        #endregion


    }
}
