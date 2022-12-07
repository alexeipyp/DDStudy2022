using Api.Models.BlackList;
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
    public class BlackListController : ControllerBase
    {
        private readonly BlackListService _blackListService;
        public BlackListController(BlackListService blackListService, LinkGeneratorService links)
        {
            _blackListService = blackListService;

            links.LinkAvatarGenerator = x =>
            Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
            {
                userId = x.Id,
            });
        }

        [HttpPost]
        [Authorize]
        public async Task AddUserToBlackList(AddUserToBlackListRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _blackListService.AddUserToBlackList(userId, request);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task RemoveUserFromBlackList(Guid unblockingUserId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _blackListService.RemoveUserFromBlackList(userId, unblockingUserId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetBlackList()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _blackListService.GetBlackList(userId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }
    }
}
