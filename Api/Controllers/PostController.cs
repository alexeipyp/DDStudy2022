using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models.Post;
using Api.Models.Attachments;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PostController : ControllerBase
    {
        private readonly PostService _postService;
        private readonly AttachService _attachService;

        public PostController(PostService postService, AttachService attachService)
        {
            _postService = postService;
            _attachService = attachService;
            _postService.SetLinkGenerator(
               linkAvatarGenerator: x =>
               Url.Action(nameof(UserController.GetUserAvatar), "User", new
               {
                   userId = x.Id,
               }),
               linkContentGenerator: x => Url.Action(nameof(GetPostAttach), new
               {
                   postContentId = x.Id,
               }))
                ;
        }

        [HttpPost]
        [Authorize]
        public async Task CreatePost(CreatePostRequestModel request)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                var model = new CreatePostModel
                {
                    AuthorId = userId,
                    Annotation = request.Annotation,
                    Attaches = request.Attaches.Select(x =>
                    new PostAttachModel(x, userId, q => _attachService.MoveAttachFromTemp(q.TempId)))
                    .ToList(),
                };
                await _postService.CreatePost(model);
            }
        }

        [HttpPost]
        [Authorize]
        public async Task CommentPost(CreateCommentPostRequestModel request)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                var model = new CreateCommentPostModel(request, userId);
                await _postService.CommentPost(model);
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<List<PostModel>> GetPosts(int skip = 0, int take = 10)
        => await _postService.GetPosts(skip, take);

        [HttpGet]
        [Authorize]
        public async Task<List<CommentModel>> GetComments(Guid postId, int skip = 0, int take = 10)
            => await _postService.GetComments(postId, skip, take);

        [HttpGet]
        [Authorize]
        public async Task<FileResult> GetPostAttach(Guid postAttachId, bool download = false)
        {
            var attach = await _postService.GetPostAttach(postAttachId);
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
                return File(fs, attach.MimeType, attach.Name);
            else
                return File(fs, attach.MimeType);
        }
    }
}
