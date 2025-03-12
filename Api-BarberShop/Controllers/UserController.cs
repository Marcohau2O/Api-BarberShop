using Api_BarberShop.Context;
using Api_BarberShop.Model;
using Api_BarberShop.Servicios.IServices;
using Api_BarberShop.Servicios.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace Api_BarberShop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly IUserServices _userservices;
        private readonly AppDbContext _context;
        private readonly IEmailServices _emailservices;

        public UserController(IUserServices services, AppDbContext dbContext, IEmailServices emailserves)
        {
            _context = dbContext;
            _userservices = services;
            _emailservices = emailserves;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Name == request.Name);

            if (user == null)
                return Unauthorized(new { message = "Credenciales incorrectas" });

            var token = await _userservices.Authenticate(request.Name, request.Password);

            if (token == null)

                return Unauthorized(new { message = "Credenciales incorrectas" });

            return Ok(new { token, usertype = user.UserType, name = user.Name, UserId = user.Id });

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegisterDto request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Request is null" });
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                Password = request.Password,
                ConfirmPassword = request.ConfirmPassword,
                UserType = request.UserType,
                ResetPasswordToken = null,
                ResetPasswordExpiry = null
            };

            var result = await _userservices.RegisterUser(user);

            if (!result)
                return StatusCode(500, new { message = "Hubo un error al registrar el usuario " });

            return Ok(new { message = "Usuario registrado con éxito" });
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
                return BadRequest("Usuario no encontrado");

            var token = Guid.NewGuid().ToString();
                user.ResetPasswordToken = token;
            user.ResetPasswordExpiry = DateTime.UtcNow.AddHours(1);

            await _context.SaveChangesAsync();

            await _emailservices.SendPasswordResetEmail(user.Email, token);

            return Ok("Correo de recuperación enviado");
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

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.ResetPasswordToken == model.Token && u.ResetPasswordExpiry > DateTime.UtcNow);
            if (user == null) 
                return BadRequest("Token invalido o expirado");

            var passwordHasher = new PasswordHasher();
            user.Password = passwordHasher.HashPassword(model.NewPassword);
            user.ConfirmPassword = passwordHasher.HashPassword(model.NewConfirmPassword);
            user.ResetPasswordToken = null;
            user.ResetPasswordExpiry = null;

            await _context.SaveChangesAsync();

            return Ok("Contraseña cambiada exitosamente");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new { message = "Token no proporcionado" });
            }

            bool result = await _userservices.Logout(token);
            if (result)
            {
                return Ok(new { message = "Sesión cerrada correctamente" });
            }

            return BadRequest(new { message = "Error al cerrar sesión" });
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            var users = await _userservices.GetUsers();
            return Ok(users);

        }

        [HttpGet("UserAppointments/{userId}")]
        public async Task<IActionResult> GetUserAppointments(int userId)
        {
            var userWithAppointments = await _context.Users
                .Include(u => u.Appointments)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (userWithAppointments == null)
                return NotFound("Usuario no encontrado");

            return Ok(new
            {
                userWithAppointments.Id,
                userWithAppointments.Name,
                userWithAppointments.Email,
                Appointment = userWithAppointments.Appointments.Select(a => new
                {
                    a.Id,
                    a.Date,
                    a.Time,
                    a.Status,
                }).ToList()
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.Include(u => u.Appointments)
                                           .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado" });
            }

            // Eliminar citas asociadas primero
            _context.Appointments.RemoveRange(user.Appointments);

            // Ahora eliminar el usuario
            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Usuario eliminado correctamente" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsr(int id, [FromBody] UpdateUserDto updatedUser)
        {
            var result = await _userservices.UpdateUser(id, updatedUser);
            if (!result) return NotFound(new { message = "Usuario no encontrado" });
            return Ok(new { message = "Usuario actualizado correctamente" });
        }

        [HttpGet("User/{id}")]
        public async Task<IActionResult> GetUserDetails(int id)
        {
            var user = await _context.Users.FindAsync(id);
                if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }
    }
}
