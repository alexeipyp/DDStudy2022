using Api.Mapper.MapperActions;
using Api.Models.Attachments;
using Api.Models.BlackList;
using Api.Models.Likes;
using Api.Models.MuteList;
using Api.Models.Post;
using Api.Models.Subscribes;
using Api.Models.User;
using AutoMapper;
using Common;
using DAL.Entities;

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

            CreateMap<DAL.Entities.User, UserAvatarModel>()
                .AfterMap<UserAvatarMapperAction>()
                ;
            CreateMap<DAL.Entities.User, UserAvatarProfileModel>()
                .IncludeBase<DAL.Entities.User, UserAvatarModel>()
                ;
            CreateMap<DAL.Entities.UserActivity, UserActivityModel>();

            CreateMap<DAL.Entities.Avatar, AttachModel>();

            CreateMap<DAL.Entities.PostAttach, AttachModel>();
            CreateMap<DAL.Entities.PostAttach, AttachWithLinkModel>()
                .AfterMap<PostAttachMapperAction>()
                ;

            CreateMap<DAL.Entities.User, DAL.Entities.UserSession>()
                .ForMember(d => d.UserId, m => m.MapFrom(s => s.Id))
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

            // Map Comments & Comments stats from DB to Models
            CreateMap<DAL.Entities.Comment, CommentModel>();
            CreateMap<DAL.Entities.Comment, DAL.Entities.CommentWithStats>();
            CreateMap<DAL.Entities.CommentStats, DAL.Entities.CommentWithStats>()
                .ForMember(d => d.Stats, m => m.MapFrom(s => s))
                ;
            CreateMap<DAL.Entities.CommentWithStats, CommentModel>()
                .IncludeBase<DAL.Entities.Comment, CommentModel>()
                ;
            CreateMap<DAL.Entities.CommentStats, CommentStatsModel>();

            // Map Posts & Posts stats from DB to Models
            CreateMap<DAL.Entities.Post, PostModel>()
                .ForMember(d => d.Attaches, m => m.MapFrom(s => s.PostAttaches))
                ;
            CreateMap<DAL.Entities.Post, DAL.Entities.PostWithStats>();
            CreateMap<DAL.Entities.PostStats, DAL.Entities.PostWithStats>()
                .ForMember(d => d.Stats, m => m.MapFrom(s => s))
                ;
            CreateMap<DAL.Entities.PostWithStats, PostModel>()
                .IncludeBase<DAL.Entities.Post, PostModel>()
                ;
            CreateMap<DAL.Entities.PostStats, PostStatsModel>();


            CreateMap<LikePostRequest, DAL.Entities.LikeToPost>()
                .ForMember(d => d.Created, m => m.MapFrom(s => DateTime.UtcNow))
                ;
            CreateMap<LikeCommentRequest, DAL.Entities.LikeToComment>();

            CreateMap<FollowUserRequest, DAL.Entities.Subscribe>();

            CreateMap<AddUserToBlackListRequest, DAL.Entities.BlackListItem>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()));
            CreateMap<AddUserToMuteListRequest, DAL.Entities.MuteListItem>()
                .ForMember(d => d.Id, m => m.MapFrom(s => Guid.NewGuid()));
        }
    }
}
