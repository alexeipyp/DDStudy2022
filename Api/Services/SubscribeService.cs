using Api.Models.Subscribes;
using Api.Models.User;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class SubscribeService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        public SubscribeService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task FollowUser(Guid userId, FollowUserRequest request)
        {
            var sub = _mapper.Map<DAL.Entities.Subscribe>(request, o => o.AfterMap((s, d) => d.FollowerId = userId));
            await _context.Subscribes.AddAsync(sub);
            await _context.SaveChangesAsync();
        }

        public async Task UndoFollowUser(Guid userId, UndoFollowUserRequest request)
        {
            var sub = await GetSubscribeById(request.AuthorId, userId);
            _context.Subscribes.Remove(sub);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAvatarModel>> GetSubscribesListById(Guid userId)
        {
            var subs = await _context.Subscribes
                .Where(x => x.FollowerId == userId)
                .Include(x => x.Author).ThenInclude(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x.Author))
                .ToListAsync();

            return subs;
        }

        public async Task<IEnumerable<UserAvatarModel>> GetFollowersListById(Guid userId)
        {
            var followers = await _context.Subscribes
                .Where(x => x.AuthorId == userId)
                .Include(x => x.Follower).ThenInclude(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x.Follower))
                .ToListAsync();

            return followers;
        }

        public bool CheckSubscription(Guid authorId, Guid followerId) 
            => _context.Subscribes.Any(x => x.AuthorId == authorId && x.FollowerId == followerId);

        public async Task<IEnumerable<Guid>> GetAuthorsIdsByFollowerId(Guid followerId)
        {
            var subs = await _context.Subscribes.AsNoTracking()
                .Where(x => x.FollowerId == followerId)
                .Select(x => x.AuthorId)
                .ToListAsync();

            return subs;
        }

        public async Task<IEnumerable<Guid>> GetFollowersIdsByAuthorId(Guid authorId)
        {
            var followers = await _context.Subscribes.AsNoTracking()
                .Where(x => x.AuthorId == authorId)
                .Select(x => x.FollowerId)
                .ToListAsync();

            return followers;
        }

        private async Task<DAL.Entities.Subscribe> GetSubscribeById(Guid authorId, Guid followerId)
        {
            var sub = await _context.Subscribes.FirstOrDefaultAsync(x => x.AuthorId == authorId && x.FollowerId == followerId);
            if (sub == null)
            {
                throw new Exception("subscribe not found");
            }

            return sub;
        }
    }
}
