using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models.Post;
using Api.Models.Attachments;
using Common.Extentions;
using Common.Consts;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
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
        
        public async Task<IEnumerable<PostModel>> GetPosts(int skip = 0, int take = 10)
        => await _postService.GetPosts(skip, take);

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<CommentModel>> GetComments(Guid postId, int skip = 0, int take = 10)
            => await _postService.GetComments(postId, skip, take);

    }
}
