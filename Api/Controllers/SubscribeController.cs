using Api.Models.Subscribes;
using Common.Extentions;
using Common.Consts;
using Common.CustomExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Services;
using Api.Models.User;
using Common.CustomExceptions.NotAuthorizedExceptions;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SubscribeController : ControllerBase
    {
        private readonly SubscribeService _subscribeService;

        public SubscribeController(SubscribeService subscribeService, LinkGeneratorService links)
        {
            _subscribeService = subscribeService;
            links.LinkAvatarGenerator = x =>
               Url.ControllerAction<AttachController>(nameof(AttachController.GetUserAvatar), new
               {
                   userId = x.Id
               });
        }

        [HttpPost]
        [Authorize]
        public async Task FollowUser(FollowUserRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _subscribeService.FollowUser(userId, request);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task UndoFollowUser(UndoFollowUserRequest request)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _subscribeService.UndoFollowUser(userId, request.AuthorId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetSubscribes()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _subscribeService.GetSubscribesListById(userId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetFollowers()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _subscribeService.GetFollowersListById(userId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IEnumerable<UserAvatarModel>> GetFollowRequests()
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                return await _subscribeService.GetFollowRequestsListById(userId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }

        [HttpPost]
        [Authorize]
        public async Task AcceptFollowRequest(Guid followerCandidateId)
        {
            var userId = User.GetClaimValue<Guid>(ClaimNames.Id);
            if (userId != default)
            {
                await _subscribeService.AcceptFollowRequest(userId, followerCandidateId);
            }
            else
            {
                throw new UnauthorizedException("not authorized");
            }
        }
    }
}
