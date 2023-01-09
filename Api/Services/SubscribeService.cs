using Api.Models.Subscribes;
using Api.Models.User;
using AutoMapper;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.NotFoundExceptions;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SubscribeService
    {
        private readonly IMapper _mapper;
        private readonly DataContext _context;
        private readonly AccessAndFilterService _accessService;
        public SubscribeService(IMapper mapper, DataContext context, AccessAndFilterService accessService)
        {
            _mapper = mapper;
            _context = context;
            _accessService = accessService;
        }

        public async Task<SubscribeStatusModel> FollowUser(Guid userId, FollowUserRequest request)
        {
            if (!(await _accessService.GetFollowPermission(userId, request.AuthorId)))
                throw new ForbiddenException("not allowed to sub");

            var sub = _mapper.Map<DAL.Entities.Subscribe>(request, o => o.AfterMap((s, d) => d.FollowerId = userId));
            sub.IsAccepted = await _accessService.GetInstantFollowPermission(userId, request.AuthorId);

            SubscribeStatusModel res;
            if (sub.IsAccepted)
            {
                res = new SubscribeStatusModel { isFollowing = sub.IsAccepted, isSubscribeRequestSent = sub.IsAccepted };
            }
            else
            {
                res = new SubscribeStatusModel { isFollowing = sub.IsAccepted, isSubscribeRequestSent = !sub.IsAccepted };
            }

            await _context.Subscribes.AddAsync(sub);
            await _context.SaveChangesAsync();
            return res;
        }

        public async Task<SubscribeStatusModel> UndoFollowUser(Guid userId, Guid authorId)
        {
            var sub = await GetSubscribeById(authorId, userId);
            _context.Subscribes.Remove(sub);
            await _context.SaveChangesAsync();
            return new SubscribeStatusModel { isFollowing = false, isSubscribeRequestSent = false };
        }

        public async Task AcceptFollowRequest(Guid userId, Guid followerCandidateId)
        {
            var sub = await GetSubscribeById(userId, followerCandidateId, true);
            sub.IsAccepted = true;
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAvatarModel>> GetSubscribesListById(Guid userId)
        {
            var subs = await _context.Subscribes
                .Where(x => x.FollowerId == userId && x.IsAccepted)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .AsNoTracking()
                .Select(x => _mapper.Map<UserAvatarModel>(x.Author))
                .ToListAsync();

            return subs;
        }

        public async Task<IEnumerable<UserAvatarModel>> GetFollowersListById(Guid userId)
        {
            var followers = await _context.Subscribes
                .Where(x => x.AuthorId == userId && x.IsAccepted)
                .Include(x => x.Follower).ThenInclude(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x.Follower))
                .AsNoTracking()
                .ToListAsync();

            return followers;
        }

        public async Task<SubscribeStatusModel> GetSubscribeStatus(Guid followerCandidateId, Guid authorCandidateId)
        {
            bool isFollowing = false;
            bool isSubscribeRequestSent = false;
            if (followerCandidateId != authorCandidateId)
            {
                isFollowing = await CheckSubscribeById(authorCandidateId, followerCandidateId, false);
                if (!isFollowing)
                {
                    isSubscribeRequestSent = await CheckSubscribeById(authorCandidateId, followerCandidateId, true);
                }
                else
                {
                    isSubscribeRequestSent = isFollowing;
                }          
            }
            return new SubscribeStatusModel { isFollowing = isFollowing, isSubscribeRequestSent = isSubscribeRequestSent };
        }

        public async Task<IEnumerable<UserAvatarModel>> GetFollowRequestsListById(Guid userId)
        {
            var requests = await _context.Subscribes
                .Where(x => x.AuthorId == userId && !x.IsAccepted)
                .Include(x => x.Follower).ThenInclude(x => x.Avatar)
                .AsNoTracking()
                .Select(x => _mapper.Map<UserAvatarModel>(x.Follower))
                .ToListAsync();

            return requests;
        }

        public async Task MutualUndoFollow(Guid userId1, Guid userId2)
        {
            var subs = await _context.Subscribes.Where(x =>
                (x.AuthorId == userId1 && x.FollowerId == userId2)
                ||
                (x.AuthorId == userId2 && x.FollowerId == userId1))
                .ToListAsync();
            if (subs != null)
            {
                _context.Subscribes.RemoveRange(subs);
                await _context.SaveChangesAsync();
            }
        }

        private async Task<DAL.Entities.Subscribe> GetSubscribeById(Guid authorId, Guid followerId, bool isSubRequest = false)
        {
            var sub = await _context.Subscribes
                .FirstOrDefaultAsync(x => x.AuthorId == authorId && x.FollowerId == followerId && x.IsAccepted != isSubRequest);
            if (sub == null)
                throw new SubscribeNotFoundException();
            
            return sub;
        }

        private async Task<bool> CheckSubscribeById(Guid authorId, Guid followerId, bool isSubRequest = false)
        {
            return await _context.Subscribes.AnyAsync(x => x.AuthorId == authorId && x.FollowerId == followerId && x.IsAccepted != isSubRequest);
        }

    }
}
