using Api_BarberShop.Context;
using Api_BarberShop.Model;
using Api_BarberShop.Servicios.IServices;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api_BarberShop.Servicios.Services
{
    public class UserServices : IUserServices
    {
        public readonly AppDbContext _context;
        public readonly IConfiguration _config;

        public UserServices(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }
        public async Task<string?> Authenticate(string name, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Name == name);

            if (user == null)
                return null;

            var passwordHasher = new PasswordHasher();
            bool isPasswordValid = passwordHasher.VerifyPassword(password, user.Password);
            if (!isPasswordValid)
            {
                return null;
            }
           

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["JWT:Secret"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new System.Security.Claims.ClaimsIdentity(new[] { new Claim("id", user.Id.ToString())}),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public class PasswordHasher
        {
            public string HashPassword(string password)
            {
                byte[] salt = Encoding.ASCII.GetBytes("SaltySecret");

                return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                numBytesRequested: 256 / 8
                    ));
            }

            public bool VerifyPassword(string password, string hashedPassword)
            {
                byte[] salt = Encoding.ASCII.GetBytes("SaltySecret");

                string hashedInput = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: salt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 10000,
                numBytesRequested: 256 / 8
                    ));

                return hashedInput == hashedPassword;
            }
        }

        public async Task<bool> RegisterUser(User user)
        {
            try
            {
                var passwordHasher = new PasswordHasher();
                user.Password = passwordHasher.HashPassword(user.Password);
                user.ConfirmPassword = passwordHasher.HashPassword(user.ConfirmPassword);
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return true;
            } catch
            {
                return false;
            }
        }

        public async Task<bool> Logout(string token)
        {
            try
            {
                var revokedToken = new RevokedToken
                {
                    Token = token,
                    RevokedAt = DateTime.UtcNow
                };

                await _context.SaveChangesAsync();
                return true;
            }
            catch 
            { 
                return false; 
            }
        }
    }
}
