﻿namespace Api.Models.User
{
    public class UserModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }

    }

    public class UserAvatarModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string? AvatarLink { get; set; }
        public UserAvatarModel(UserModel model, Func<UserModel, string?>? linkGenerator)
        {
            Id = model.Id;
            Name = model.Name;
            AvatarLink = linkGenerator?.Invoke(model);
        }
    }
}
