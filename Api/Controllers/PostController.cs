using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models.Post;
using Api.Models.Attachments;
using Common.Extentions;
using Common.Consts;
using Api.Models.Likes;
using Common.CustomExceptions;
using Common.CustomExceptions.UnauthorizedExceptions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;

        public PostController(PostService postService, LinkGeneratorService links)
        {
            _postService = postService;
            links.LinkAvatarGenerator = x =>
               Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
               {
                   userId = x.Id
               });
            links.LinkContentGenerator = x =>
               Url.ControllerAction<AttachController>(nameof(AttachController.GetPostAttach), new
               {
                   postAttachId = x.Id
               });
        }

        [HttpPost]
        [Authorize]
        public async Task CreatePost(CreatePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.CreatePost(userId, request);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task CommentPost(CreateCommentPostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.CommentPost(userId, request);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<PostModel>> GetFeed(int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.GetFeed(userId, skip, take);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<PostModel>> GetSubscriptionsFeed(int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.GetSubscriptionsFeed(userId, skip, take);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<PostModel>> GetUserPosts(Guid userToVisitId, int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.GetUserPosts(userId, userToVisitId, skip, take);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<CommentModel>> GetComments(Guid postId, int skip = 0, int take = 10)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.GetComments(userId, postId, skip, take);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

    }
}
