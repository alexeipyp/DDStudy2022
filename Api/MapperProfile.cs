using Api.Models.Attachments;
using Api.Models.Post;
using Api.Models.User;
using AutoMapper;
using Common;

namespace Api
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<CreateUserModel, DAL.Entities.User>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.PasswordHash, m => m.MapFrom(s => HashHelper.GetHash(s.Password)))
                .ForMember(d => d.BirthDate, m => m.MapFrom(s => s.BirthDate.UtcDateTime))
                ;
            CreateMap<DAL.Entities.User, UserModel>();
            CreateMap<DAL.Entities.Avatar, AttachModel>();
            CreateMap<DAL.Entities.PostAttach, AttachModel>();

            CreateMap<PostAttachModel, DAL.Entities.PostAttach>();
            CreateMap<CreatePostModel, DAL.Entities.Post>()
                .ForMember(d => d.PostAttaches, m => m.MapFrom(s => s.Attaches))
                .ForMember(d => d.UploadDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;
            CreateMap<CreateCommentPostModel, DAL.Entities.Comment>()
                .ForMember(d => d.UploadDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;
        }
    }
}
