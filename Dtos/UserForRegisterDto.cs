using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegisterDto
    {
        [Required]
        public string Username { get; set; }
        
        [Required]
        [StringLength(8, MinimumLength = 6, ErrorMessage = "You must specify your password between 6 or 8 characters")]
        public string Password { get; set; }
    }
}