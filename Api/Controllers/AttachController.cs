﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Common.CustomExceptions;
using Api.Services;
using Api.Models.Attachments;
using DAL.Entities;
using Common.Extentions;
using Common.Consts;
using Common.CustomExceptions.UnauthorizedExceptions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class AttachController : ControllerBase
    {
        private readonly AttachService _attachService;
        private readonly UserService _userService;
        private readonly PostService _postService;

        public AttachController(AttachService attachService, UserService userService, PostService postService)
        {
            _attachService = attachService;
            _userService = userService;
            _postService = postService;
        }

        [HttpPost]
        [Authorize]
        public async Task<List<MetadataModel>> UploadFiles([FromForm] List<IFormFile> files) 
            => await _attachService.UploadFiles(files);

        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false) 
            => RenderAttach(await _userService.GetUserAvatar(userId), download);

        [HttpGet]
        [Route("{postAttachId}")]
        [Authorize]
        public async Task<FileResult> GetPostAttach(Guid postAttachId, bool download = false)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return RenderAttach(await _postService.GetPostAttach(userId, postAttachId), download);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }
        private FileStreamResult RenderAttach(AttachModel attach, bool download)
        {
            var fs = new FileStream(attach.FilePath, FileMode.Open);
            if (download)
            {
                return File(fs, attach.MimeType, attach.Name);
            }
            else
            {
                return File(fs, attach.MimeType);
            }
        }

    }
}
