using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class AccessManagementService
    {
        private readonly DataContext _context;

        public AccessManagementService(DataContext context)
        {
            _context = context;
        }

        public IQueryable<Guid> GetReadAccessPolicy(Guid userId)
        {
            var policyQuery = _context.Users.AsQueryable();
            // Исключаем аккаунты, которые заблокировал текущий пользователь, или которые заблокировали текущего пользователя
            policyQuery = policyQuery.Where(x => !x.BlockedUsers!.Any(author => author.BlockedUserId == userId) && !x.BlockedByUsers!.Any(author => author.UserId == userId));
            // Исключаем приватные аккаунты, на которые текущий пользователь не подписан, кроме аккаунта текущего пользователя
            policyQuery = policyQuery.Where(x => x.Id == userId || (!x.Config.IsPrivate || x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted)));
            
            return policyQuery.Select(x => x.Id);
        }

        public IQueryable<Guid> GetFollowerAccessPolicy(Guid userId)
        {
            var policyQuery = _context.Users.AsQueryable();
            // Исключаем аккаунты, на которые текущий пользователь не подписан
            policyQuery = policyQuery.Where(x => x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted));

            return policyQuery.Select(x => x.Id);

        }

        public async Task<bool> GetReadPermission(Guid userId, Guid authorId)
        {
            return await GetReadAccessPolicy(userId).AnyAsync(x => x == authorId);
        }

        public async Task<bool> GetWritePermission(Guid userId, Guid authorId)
        {
            var policyQuery = _context.Users.AsQueryable();
            // Исключаем аккаунты, которые добавили текущего пользователя в Mute-лист
            policyQuery = policyQuery.Where(x => !x.MutedUsers!.Any(author => author.MutedUserId == userId));
            // Исключаем аккаунты, которые заблокировал текущий пользователь, или которые заблокировали текущего пользователя
            policyQuery = policyQuery.Where(x => !x.BlockedUsers!.Any(author => author.BlockedUserId == userId) && !x.BlockedByUsers!.Any(author => author.UserId == userId));
            // Исключаем приватные аккаунты, на которые текущий пользователь не подписан, кроме аккаунта текущего пользователя
            policyQuery = policyQuery.Where(x => x.Id == userId || (!x.Config.IsPrivate || x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted)));

            return await policyQuery.AnyAsync(x => x.Id == authorId);
        }

        public async Task<bool> GetInstantFollowPermission(Guid authorId)
        {
            var policyQuery = _context.Users.AsQueryable();
            // Исклюаем приватные аккаунты
            policyQuery = policyQuery.Where(x => !x.Config.IsPrivate);

            return await policyQuery.AnyAsync(x => x.Id == authorId);
        }

        public async Task<bool> GetFollowPermission(Guid userId, Guid followingUserId)
        {
            var policyQuery = _context.Users.AsQueryable();
            // Исключаем аккаунты, которые заблокировал текущий пользователь, или которые заблокировали текущего пользователя
            policyQuery = policyQuery.Where(x => !x.BlockedUsers!.Any(author => author.BlockedUserId == userId) && !x.BlockedByUsers!.Any(author => author.UserId == userId));

            return await policyQuery.AnyAsync(x => x.Id == followingUserId);
        }
    }
}
