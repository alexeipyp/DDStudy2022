using Common.Enums;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
            var policy = AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers;

            return GetPermittedUserIds(userId, policy);
        }

        public IQueryable<Guid> GetFeedAccessPolicy(Guid userId)
        {
            var policy = AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers | AccessPolicyItems.ExcludeCurrentUser;

            return GetPermittedUserIds(userId, policy);
        }

        public IQueryable<Guid> GetFollowerAccessPolicy(Guid userId)
        {
            var policy = AccessPolicyItems.OnlySubscribed;

            return GetPermittedUserIds(userId, policy);
        }

        public IQueryable<Guid> GetUserAccessPolicy(Guid userId, Guid authorId)
        {
            var policyQuery = GetReadAccessPolicy(userId);
            policyQuery = policyQuery.Where(x => x == authorId);

            return policyQuery;
        }

        public async Task<bool> GetReadPermission(Guid userId, Guid authorId)
        {
            return await GetReadAccessPolicy(userId).AnyAsync(x => x == authorId);
        }

        public async Task<bool> GetWritePermission(Guid userId, Guid authorId)
        {
            var policy = AccessPolicyItems.ApplyMuteList | AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers;
            var policyQuery = GetPermittedUserIds(userId, policy);

            return await policyQuery.AnyAsync(x => x == authorId);
        }

        public async Task<bool> GetInstantFollowPermission(Guid userId, Guid authorId)
        {
            var policy = AccessPolicyItems.OnlyPublicUsers;
            var policyQuery = GetPermittedUserIds(userId, policy);

            return await policyQuery.AnyAsync(x => x == authorId);
        }

        public async Task<bool> GetFollowPermission(Guid userId, Guid followingUserId)
        {
            var policy = AccessPolicyItems.ApplyBlackList;
            var policyQuery = GetPermittedUserIds(userId, policy);
           
            return await policyQuery.AnyAsync(x => x == followingUserId);
        }

        private IQueryable<Guid> GetPermittedUserIds(Guid userId, AccessPolicyItems policyItems = AccessPolicyItems.None)
        {
            var policyQuery = _context.Users.AsQueryable();

            if (policyItems.HasFlag(AccessPolicyItems.ApplyBlackList))
            {
                // Исключаем аккаунты, которые заблокировал текущий пользователь, или которые заблокировали текущего пользователя
                policyQuery = policyQuery.Where(x => !x.BlockedUsers!.Any(author => author.BlockedUserId == userId) && !x.BlockedByUsers!.Any(author => author.UserId == userId));
            }

            if (policyItems.HasFlag(AccessPolicyItems.ApplyMuteList))
            {
                // Исключаем аккаунты, которые добавили текущего пользователя в Mute-лист
                policyQuery = policyQuery.Where(x => !x.MutedUsers!.Any(author => author.MutedUserId == userId));
            }

            if (policyItems.HasFlag(AccessPolicyItems.ExcludeUnsubscribedPrivateUsers))
            {
                // Исключаем приватные аккаунты, на которые текущий пользователь не подписан, кроме аккаунта текущего пользователя
                policyQuery = policyQuery.Where(x => x.Id == userId || (!x.Config.IsPrivate || x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted)));
            }

            if (policyItems.HasFlag(AccessPolicyItems.ExcludeCurrentUser))
            {
                // Исключаем аккаунт текущего пользователя
                policyQuery = policyQuery.Where(x => x.Id != userId);
            }

            if (policyItems.HasFlag(AccessPolicyItems.OnlySubscribed))
            {
                // Исключаем аккаунты, на которые текущий пользователь не подписан
                policyQuery = policyQuery.Where(x => x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted));
            }

            if (policyItems.HasFlag(AccessPolicyItems.OnlyFollowers))
            {

            }

            if (policyItems.HasFlag(AccessPolicyItems.OnlyPublicUsers))
            {
                // Исклюаем приватные аккаунты
                policyQuery = policyQuery.Where(x => !x.Config.IsPrivate);
            }

            return policyQuery.Select(x => x.Id);
        }

        private async Task<bool> GetPermission(Guid userId, Guid userToVisitId, AccessPolicyItems policyItems)
        {
            var policyQuery = GetPermittedUserIds(userId, policyItems);
            return await policyQuery.AnyAsync(x => x == userToVisitId);
        }

    }
}
