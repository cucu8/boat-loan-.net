using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SadikTuranECommerce.DTO;
using SadikTuranECommerce.Entities;
using SadikTuranECommerce.Helpers;

namespace SadikTuranECommerce.Controllers
{
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly BoatRentalDbContext _context;

        public UsersController(BoatRentalDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDTO>>> GetUsers()
        {
            var users = await _context.Users
            .Include(u => u.Boats)
            .ThenInclude(b => b.Images)
            .ToListAsync();

            var userDtos = users.Select(u => new UserResponseDTO
            {
                Id = u.Id,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Name = u.Name,
                UserType = u.UserType,
            }).ToList();

            return Ok(userDtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDTO>> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.Boats)
                    .ThenInclude(b => b.Images)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(new { Message = $"User with Id = {id} not found." });
            }
            return Ok(new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Name = user.Name,
                UserType = user.UserType,
            });
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDTO>> CreateUser([FromBody] UserCreateDTO userDto)
        {
            // Şifre ve ConfirmPassword kontrolü
            if (userDto.Password != userDto.ConfirmPassword)
                return BadRequest(new { Message = "Password and ConfirmPassword do not match." });

            // Email benzersizliği kontrolü
            if (await _context.Users.AnyAsync(u => u.Email == userDto.Email))
                return BadRequest(new { Message = "Email is already registered." });

            // Telefon numarası verilmişse benzersizliğini kontrol et
            if (!string.IsNullOrEmpty(userDto.PhoneNumber))
            {
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == userDto.PhoneNumber))
                    return BadRequest(new { Message = "Phone number is already registered." });
            }

            PasswordHelper.CreatePasswordHash(userDto.Password, out string passwordHash, out string passwordSalt);

            var user = new User
            {
                Email = userDto.Email,
                PhoneNumber = userDto.PhoneNumber,
                Name = userDto.Name,
                UserType = userDto.UserType,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
     
            var userCredential = new UserCredential
            {
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                User = user
            };

            _context.Users.Add(user);
            _context.UserCredentials.Add(userCredential);

            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, new UserResponseDTO
            {
                Id = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Name = user.Name,
                UserType = user.UserType,
         
            });
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO loginDto)
        {
            var user = await _context.Users
                .Include(u => u.UserCredential)
                .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null) return BadRequest(new { message = "User Not Found Or Password incorrect" });

            if (user.UserCredential == null)
                return BadRequest(new { Message = "Invalid email or password." });

            bool isPasswordValid = PasswordHelper.VerifyPasswordHash(
                loginDto.Password,
                user.UserCredential.PasswordHash,
                user.UserCredential.PasswordSalt);

            if (!isPasswordValid)
                return BadRequest(new { Message = "Invalid email or password." });

            var token = JwtHelper.GenerateToken(user, _context.GetService<IConfiguration>());

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.UserType,
                    user.Name
                }
            });
        }


        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDTO>> UpdateUser(int id, [FromBody] UserUpdateDTO userUpdateDto)
        {
            var userToUpdate = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (userToUpdate == null)
            {
                return NotFound(new { Message = $"User with Id = {id} not found." });
            }

            // Check if the new email is already registered by another user
            if (!string.IsNullOrEmpty(userUpdateDto.Email) && userUpdateDto.Email != userToUpdate.Email)
            {
                if (await _context.Users.AnyAsync(u => u.Email == userUpdateDto.Email))
                {
                    return BadRequest(new { Message = "Email is already registered by another user." });
                }
                userToUpdate.Email = userUpdateDto.Email;
            }

            // Check if the new phone number is already registered by another user
            if (!string.IsNullOrEmpty(userUpdateDto.PhoneNumber) && userUpdateDto.PhoneNumber != userToUpdate.PhoneNumber)
            {
                if (await _context.Users.AnyAsync(u => u.PhoneNumber == userUpdateDto.PhoneNumber))
                {
                    return BadRequest(new { Message = "Phone number is already registered by another user." });
                }
                userToUpdate.PhoneNumber = userUpdateDto.PhoneNumber;
            }

           
            userToUpdate.UpdatedAt = DateTime.UtcNow; 

            await _context.SaveChangesAsync();

            return Ok(new UserResponseDTO
            {
                Email = userToUpdate.Email,
                PhoneNumber = userToUpdate.PhoneNumber,
            });
        }


    }
}
