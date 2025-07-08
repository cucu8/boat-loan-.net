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

        [Authorize]
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
                    return BadRequest(new { Message = "Telefon numarası daha önce kullanıldı" });
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


        [Authorize]
        [HttpPut("{id}/change-password")]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDTO changePasswordDto)
        {
         
            var user = await _context.Users
                .Include(u => u.UserCredential) 
                .FirstOrDefaultAsync(u => u.Id == id);

            // Kullanıcı bulunamadıysa hata dön
            if (user == null)
            {
                return NotFound(new { Message = $"User with ID '{id}' not found." });
            }

            // Kullanıcının kimlik bilgileri (şifre hash/salt) eksikse hata dön
            if (user.UserCredential == null)
            {
                // Bu durum normalde olmamalı, her kullanıcının kimlik bilgisi olmalı
                return BadRequest(new { Message = "User has no credentials to change password. Please contact support." });
            }

            // 2. Mevcut şifreyi doğrula
            bool isCurrentPasswordValid = PasswordHelper.VerifyPasswordHash(
                changePasswordDto.CurrentPassword,
                user.UserCredential.PasswordHash,
                user.UserCredential.PasswordSalt);

            if (!isCurrentPasswordValid)
            {
                return BadRequest(new { Message = "The current password you entered is incorrect." });
            }

            // 3. Yeni şifre ve onay şifresinin eşleştiğini kontrol et
            if (changePasswordDto.NewPassword != changePasswordDto.NewPasswordConfirm)
            {
                return BadRequest(new { Message = "New password and confirmation do not match." });
            }

            // 4. Yeni şifreyi mevcut şifreyle aynı olup olmadığını kontrol et
            if (changePasswordDto.NewPassword == changePasswordDto.CurrentPassword)
            {
                return BadRequest(new { Message = "New password cannot be the same as the current password." });
            }

            // 5. Yeni şifre için hash ve salt oluştur
            PasswordHelper.CreatePasswordHash(changePasswordDto.NewPassword, out string newPasswordHash, out string newPasswordSalt);

            // 6. Kullanıcının kimlik bilgilerini güncelle
            user.UserCredential.PasswordHash = newPasswordHash;
            user.UserCredential.PasswordSalt = newPasswordSalt;
            user.UpdatedAt = DateTime.UtcNow; // Kullanıcı kaydının güncellenme zamanını işaretle

            // 7. Değişiklikleri veritabanına kaydet
            await _context.SaveChangesAsync();

            // Başarılı yanıt dön
            return Ok(new { Message = "Your password has been successfully changed." });
        }

    }
}
