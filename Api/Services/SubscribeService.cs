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
        private readonly AccessManagementService _accessService;
        public SubscribeService(IMapper mapper, DataContext context, AccessManagementService accessService)
        {
            _mapper = mapper;
            _context = context;
            _accessService = accessService;
        }

        public async Task FollowUser(Guid userId, FollowUserRequest request)
        {
            if (!(await _accessService.GetFollowPermission(userId, request.AuthorId)))
                throw new ForbiddenException("not allowed to sub");

            var sub = _mapper.Map<DAL.Entities.Subscribe>(request, o => o.AfterMap((s, d) => d.FollowerId = userId));
            if (!(await _accessService.GetInstantFollowPermission(request.AuthorId)))
                sub.IsAccepted = false;

            await _context.Subscribes.AddAsync(sub);
            await _context.SaveChangesAsync();
        }

        public async Task UndoFollowUser(Guid userId, Guid authorId)
        {
            var sub = await GetSubscribeById(authorId, userId);
            _context.Subscribes.Remove(sub);
            await _context.SaveChangesAsync();
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
                .Select(x => _mapper.Map<UserAvatarModel>(x.Author))
                .AsNoTracking()
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

        public async Task<IEnumerable<UserAvatarModel>> GetFollowRequestsListById(Guid userId)
        {
            var requests = await _context.Subscribes
                .Where(x => x.AuthorId == userId && !x.IsAccepted)
                .Include(x => x.Follower).ThenInclude(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x.Follower))
                .AsNoTracking()
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
            var sub = await _context.Subscribes.AsNoTracking().FirstOrDefaultAsync(x => x.AuthorId == authorId && x.FollowerId == followerId && x.IsAccepted != isSubRequest);
            if (sub == null)
                throw new SubscribeNotFoundException();
            

            return sub;
        }

    }
}
