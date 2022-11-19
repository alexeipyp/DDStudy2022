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
using Common.Extentions;
using Common.Consts;
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

        public UserController(UserService userService, LinkGeneratorService links)
        {
            _userService = userService;

            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task AddAvatarToUser(MetadataModel model)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {;
                await _userService.AddAvatarToUser(userId, model);
            }
            else
                throw new NotAuthorizedException("you are not authorized");
        }

        [HttpGet]
        [Authorize]
        public async Task<List<UserAvatarModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserAvatarModel> GetCurrentUser()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new NotAuthorizedException("you are not authorized");
            }

    }
}
