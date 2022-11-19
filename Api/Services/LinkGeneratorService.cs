using Api.Models.Attachments;
using Api.Models.User;
using DAL.Entities;

namespace Api.Services
{
    public delegate string? LinkContentGenerator(PostAttach s);
    public delegate string? LinkAvatarGenerator(User s);
    public class LinkGeneratorService
    {
        public LinkContentGenerator? LinkContentGenerator;
        public LinkAvatarGenerator? LinkAvatarGenerator;

        public void FixAvatar(User s, UserAvatarModel d)
        {
            d.AvatarLink = s.Avatar == null ? null : LinkAvatarGenerator?.Invoke(s);
        }
        public void FixAttach(PostAttach s, AttachWithLinkModel d)
        {
            d.AttachLink = LinkContentGenerator?.Invoke(s);
        }
    }
}
