namespace Api.Models
{
    public class UserModel
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTimeOffset BirthDate { get; set; }

        public UserModel(string name, string email, DateTimeOffset birthDate)
        {
            Name = name;
            Email = email;
            BirthDate = birthDate;
        }
    }
}
