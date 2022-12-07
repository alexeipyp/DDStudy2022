namespace Api.Models.User
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;

    }

    public class UserAvatarModel : UserModel
    {
        public string? AvatarLink { get; set; }
    }

    public class UserAvatarProfileModel : UserAvatarModel
    {
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

    }
}
