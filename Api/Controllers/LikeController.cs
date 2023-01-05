using Api.Models.Likes;
using Api.Models.Post;
using Api.Services;
using Common.Consts;
using Common.CustomExceptions.UnauthorizedExceptions;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class LikeController : ControllerBase
    {
        private readonly PostService _postService;

        public LikeController(PostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task<PostStatsModel> LikePost(LikePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.LikePost(userId, request);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task<CommentStatsModel> LikeComment(LikeCommentRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _postService.LikeComment(userId, request);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }
    }
}
