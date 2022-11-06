namespace Api.Models
{
    public class UserModel
    {
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

    }
}
