using Api.Mapper.MapperActions;
using Api.Models.Attachments;
using Api.Models.Likes;
using Api.Models.Post;
using Api.Models.Subscribes;
using Api.Models.User;
using AutoMapper;
using Common;

namespace Api.Mapper
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
            CreateMap<DAL.Entities.User, DAL.Entities.UserConfig>()
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.Id))
                .ForMember(d => d.IsPrivate, m => m.MapFrom(s => false))
                ;

            CreateMap<DAL.Entities.User, UserModel>();
            CreateMap<DAL.Entities.Avatar, AttachModel>();

            CreateMap<DAL.Entities.PostAttach, AttachModel>();
            CreateMap<DAL.Entities.PostAttach, AttachWithLinkModel>()
                .AfterMap<PostAttachMapperAction>()
                ;

            CreateMap<DAL.Entities.User, DAL.Entities.UserSession>()
                .ForMember(d => d.User, m => m.MapFrom(s => s))
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTime.UtcNow))
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()))
                .ForMember(d => d.RefreshTokenId, m => m.MapFrom(s => Guid.NewGuid()))
                ;

            CreateMap<MetadataModel, AddAvatarRequest>();
            CreateMap<AddAvatarRequest, MetaPathModel>()
                .AfterMap<AddAvatarMapperAction>()
                ;

            CreateMap<MetadataModel, MetaPathModel>()
                .AfterMap<MetaPathMapperAction>()
                ;

            CreateMap<MetaPathModel, DAL.Entities.Avatar>()
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.AuthorId))
                ;
            CreateMap<MetaPathModel, DAL.Entities.PostAttach>();

            CreateMap<CreatePostRequest, CreatePostAuthorizedRequest>();
            CreateMap<CreatePostAuthorizedRequest, CreatePostModel>()
                .AfterMap<CreatePostMapperAction>()
                ;

            CreateMap<CreatePostModel, DAL.Entities.Post>()
                .ForMember(d => d.PostAttaches, m => m.MapFrom(s => s.Attaches))
                .ForMember(d => d.UploadDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;

            CreateMap<CreateCommentPostRequest, CreateCommentPostModel>();
            CreateMap<CreateCommentPostModel, DAL.Entities.Comment>()
                .ForMember(d => d.UploadDate, m => m.MapFrom(s => DateTime.UtcNow))
                ;
            CreateMap<DAL.Entities.Comment, CommentModel>()
                .ForMember(d => d.LikesAmount, m => m.MapFrom(s => s.Likes!.Count))
                ;

            CreateMap<DAL.Entities.User, UserAvatarModel>()
                .AfterMap<UserAvatarMapperAction>()
                ;
            CreateMap<DAL.Entities.Post, PostModel>()
                .ForMember(d => d.Attaches, m => m.MapFrom(s => s.PostAttaches))
                .ForMember(d => d.CommentsAmount, m => m.MapFrom(s => s.Comments!.Count))
                .ForMember(d => d.LikesAmount, m => m.MapFrom(s => s.Likes!.Count))
                ;

            CreateMap<LikePostRequest, DAL.Entities.LikeToPost>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTime.UtcNow))
                ;
            CreateMap<LikeCommentRequest, DAL.Entities.LikeToComment>();

            CreateMap<FollowUserRequest, DAL.Entities.Subscribe>();
        }
    }
}
