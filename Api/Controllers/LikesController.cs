using Api.Models.Likes;
using Api.Services;
using Common.Consts;
using Common.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class LikesController : ControllerBase
    {
        private readonly PostService _postService;

        public LikesController(PostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task LikePost(LikePostRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.LikePost(userId, request);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task UndoLikeToPost(LikeToPostUndoRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.UndoLikeToPost(userId, request);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task LikeComment(LikeCommentRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.LikeComment(userId, request);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task UndoLikeToComment(LikeToCommentUndoRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _postService.UndoLikeToComment(userId, request);
            }
        }
    }
}
