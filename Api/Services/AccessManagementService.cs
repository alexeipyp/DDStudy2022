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
            return _context.Users.Where(x =>
            (x.Id == userId
                ||
                (!x.BlockedUsers!.Any(author => author.BlockedUserId == userId)
                    &&
                !x.BlockedByUsers!.Any(author => author.UserId == userId)
                    &&
                    (
                        (!x.Config.IsPrivate)
                        ||
                        (x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted))
                    )
                )
            ))
            .Select(x => x.Id);
        }

        public IQueryable<Guid> GetFollowerAccessPolicy(Guid userId)
        {
            return _context.Users.Where(x =>
                x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted))
                .Select(x => x.Id)
                ;
        }

        public async Task<bool> GetWriteAccessPermission(Guid userId, Guid authorId)
        {
            return await _context.Users.AnyAsync(x =>
            (x.Id == userId
                ||
            x.Id == authorId
                &&
                (!x.BlockedUsers!.Any(author => author.BlockedUserId == userId)
                    &&
                !x.BlockedByUsers!.Any(author => author.UserId == userId)
                    &&
                !x.MutedUsers!.Any(author => author.MutedUserId == userId)
                    &&
                    (
                        (!x.Config.IsPrivate)
                        ||
                        (x.Followers!.Any(author => author.FollowerId == userId && author.IsAccepted))
                    )
                )
            ));
        }

        public async Task<bool> GetInstantFollowPermission(Guid authorId)
        {
            return await _context.Users.AnyAsync(x => !x.Config.IsPrivate && x.Id == authorId);
        }

        public async Task<bool> GetFollowPermission(Guid userId, Guid followingUserId)
        {
            return await _context.Users
                .AnyAsync(x => x.Id == userId
                            && !x.BlockedUsers!.Any(user => user.BlockedUserId == followingUserId && user.UserId == userId) 
                            && !x.BlockedByUsers!.Any(user => user.UserId == followingUserId && user.BlockedUserId == userId));
        }
    }
}
