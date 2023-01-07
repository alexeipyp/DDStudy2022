using Common.Enums;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Api.Services
{
    public class AccessAndFilterService
    {
        private readonly DataContext _context;

        public AccessAndFilterService(DataContext context)
        {
            _context = context;
        }

        public IQueryable<DAL.Entities.User> GetReadAccessPolicy(Guid userId)
        {
            var policy = AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers;
            return GetPermittedUsers(userId, policy);
        }

        public async Task<bool> GetReadPermission(Guid userId, Guid authorId)
        {
            var policy = AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers;
            return await GetPermission(userId, authorId, policy);
        }

        public async Task<bool> GetWritePermission(Guid userId, Guid authorId)
        {
            var policy = AccessPolicyItems.ApplyMuteList | AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludeUnsubscribedPrivateUsers;
            return await GetPermission(userId, authorId, policy);
        }

        public async Task<bool> GetInstantFollowPermission(Guid userId, Guid authorId)
        {
            var policy = AccessPolicyItems.ApplyBlackList | AccessPolicyItems.ExcludePrivateUsers;
            return await GetPermission(userId, authorId, policy);
        }

        public async Task<bool> GetFollowPermission(Guid userId, Guid followingUserId)
        {
            var policy = AccessPolicyItems.ApplyBlackList; 
            return await GetPermission(userId, followingUserId, policy);
        }

        public IQueryable<DAL.Entities.User> GetUserAccessPolicy(Guid userId, Guid authorId)
        {
            var policyQuery = GetReadAccessPolicy(userId);
            policyQuery = policyQuery.Where(x => x.Id == authorId);

            return policyQuery;
        }

        public IQueryable<DAL.Entities.User> GetSearchFeedFiltering(Guid userId)
        {
            var policyQuery = GetReadAccessPolicy(userId);
            var filterOptions = new FilterOptions{ SubscribesFilter = SubscribesFilterOption.Exclude, CurrentUserFilter = CurrentUserFilterOption.Exclude};

            return GetFilteredUsers(userId, filterOptions, policyQuery);
        }

        public IQueryable<DAL.Entities.User> GetSubscribeFeedFiltering(Guid userId)
        {
            var policyQuery = GetReadAccessPolicy(userId);
            var filterOptions = new FilterOptions { SubscribesFilter = SubscribesFilterOption.Only, CurrentUserFilter = CurrentUserFilterOption.Exclude };

            return GetFilteredUsers(userId, filterOptions, policyQuery);
        }


        private IQueryable<DAL.Entities.User> GetPermittedUsers(Guid userId, AccessPolicyItems policyItems)
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

            if (policyItems.HasFlag(AccessPolicyItems.ExcludePrivateUsers))
            {
                // Исключаем приватные аккаунты
                policyQuery = policyQuery.Where(x => !x.Config.IsPrivate);
            }

            return policyQuery;
        }

        private IQueryable<DAL.Entities.User> GetFilteredUsers(Guid userId, FilterOptions filterOptions, IQueryable<DAL.Entities.User> permittedUsers)
        {
            var filterQuery = permittedUsers;

            switch (filterOptions.SubscribesFilter)
            {
                case SubscribesFilterOption.Only:
                    // Исключаем аккаунты, на которые текущий пользователь не подписан
                    filterQuery = filterQuery.Where(x => x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted));
                    break;
                case SubscribesFilterOption.Exclude:
                    // Исключаем аккаунты, на которые текущий пользователь подписан
                    filterQuery = filterQuery.Where(x => !x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted));
                    break;
            }

            switch (filterOptions.CurrentUserFilter)
            {
                case CurrentUserFilterOption.Exclude:
                    // Исключаем аккаунт текущего пользователя
                    filterQuery = filterQuery.Where(x => x.Id != userId);
                    break;
            }

            return filterQuery;
        }

        private async Task<bool> GetPermission(Guid userId, Guid userToVisitId, AccessPolicyItems policyItems)
        {
            var policyQuery = GetPermittedUsers(userId, policyItems);
            return await policyQuery.AnyAsync(x => x.Id == userToVisitId);
        }

    }
}
