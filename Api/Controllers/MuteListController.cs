using Api.Models.BlackList;
using Api.Models.MuteList;
using Api.Models.User;
using Api.Services;
using Common.Consts;
using Common.CustomExceptions.UnauthorizedExceptions;
using Common.Extentions;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Api")]
    public class MuteListController : ControllerBase
    {
        private readonly MuteListService _muteListService;

        public MuteListController(MuteListService muteListService, LinkGeneratorService links)
        {
            _muteListService = muteListService;

            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task AddUserToMuteList(AddUserToMuteListRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _muteListService.AddUserToMuteList(userId, request);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task RemoveUserFromMuteList(Guid unmuteUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _muteListService.RemoveUserFromMuteList(userId, unmuteUserId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetMuteList()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _muteListService.GetMuteList(userId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }
    }
}
