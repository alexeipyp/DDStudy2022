using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Common.CustomExceptions;
using System.Runtime.CompilerServices;
using Api.Models.User;
using Api.Models.Attachments;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly AttachService _attachService;

        public UserController(UserService userService, AttachService attachService)
        {
            _userService = userService;
            _attachService = attachService;
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model) => await _userService.CreateUser(model);

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                var filePath = _attachService.MoveAttachFromTemp(model.TempId);
                await _userService.AddAvatarToUser(userId, model, filePath); 
            }
            else
                throw new NotAuthorizedException("you are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<FileStreamResult> GetUserAvatar(Guid userId, bool download = false)
        {
            var attach = await _userService.GetUserAvatar(userId);
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

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new NotAuthorizedException("you are not authorized");
            }

    }
}
