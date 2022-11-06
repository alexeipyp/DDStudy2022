using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateUserModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        [Compare(nameof(Password))]
        public string RetryPassword { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

    }
}
