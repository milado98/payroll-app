using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PayrollAPI.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PayrollAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        // ───── LOGIN ─────
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            if (await _userManager.IsLockedOutAsync(user))
                return Unauthorized(new { 
                    message = "Account locked. Try again in 15 minutes." 
                });

            var result = await _signInManager.CheckPasswordSignInAsync(
                user, dto.Password, lockoutOnFailure: true);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid credentials" });

            var token = await GenerateJwtToken(user);

            return Ok(new {
                token,
                email = user.Email,
                firstName = user.FirstName,
                lastName = user.LastName
            });
        }

        // ───── FIRST TIME SETUP - Creates Super Admin ─────
        [HttpPost("setup")]
        public async Task<IActionResult> Setup([FromBody] SetupDto dto)
        {
            // Safety check - only works when NO users exist
            if (_userManager.Users.Any())
                return BadRequest(new { 
                    message = "Setup already completed. This endpoint is disabled." 
                });

            var user = new AppUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(new { 
                    errors = result.Errors.Select(e => e.Description) 
                });

            await _userManager.AddToRoleAsync(user, "SuperAdmin");

            return Ok(new {
                message = "Super Admin created successfully!",
                email = user.Email
            });
        }

        // ───── JWT TOKEN GENERATOR ─────
        private async Task<string> GenerateJwtToken(AppUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var jwtSettings = _config.GetSection("JwtSettings");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]!));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(jwtSettings["ExpiryMinutes"]!)),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

                    // CREATE A NEW STAFF USER (SuperAdmin only)
            [HttpPost("create-user")]
            [Authorize(Roles = "SuperAdmin")]
            public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
            {
                // Check if email already exists
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest(new { message = "A user with this email already exists" });

                var user = new AppUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);

                if (!result.Succeeded)
                    return BadRequest(new {
                        errors = result.Errors.Select(e => e.Description)
                    });

                // Validate the role exists
                var validRoles = new[]
                {
                    "SuperAdmin", "HRAdmin", "FinanceOfficer", "Manager", "Employee"
                };

                if (!validRoles.Contains(dto.Role))
                    return BadRequest(new {
                        message = $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}"
                    });

                await _userManager.AddToRoleAsync(user, dto.Role);

                return Ok(new {
                    message = "User created successfully",
                    email = user.Email,
                    role = dto.Role
                });
            }
    }

    

    // ───── DTOs ─────
    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class SetupDto
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class CreateUserDto
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
}