using Api.Models.BlackList;
using Api.Models.User;
using AutoMapper;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.NotFoundExceptions;
using DAL;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class BlackListService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private readonly SubscribeService _subscribeService;

        public BlackListService(IMapper mapper, DataContext context, SubscribeService subscribeService)
        {
            _mapper = mapper;
            _context = context;
            _subscribeService = subscribeService;
        }

        public async Task AddUserToBlackList(Guid userId, AddUserToBlackListRequest request)
        {
            if (await CheckUserExistInBlackList(userId, request.BlockedUserId))
                throw new ForbiddenException("user already in black list");

            var dbBlackListItem = _mapper.Map<DAL.Entities.BlackListItem>(request, o => o.AfterMap((s, d) => d.UserId = userId));
            await _context.BlackList.AddAsync(dbBlackListItem);
            await _context.SaveChangesAsync();

            await _subscribeService.MutualUndoFollow(request.BlockedUserId, userId);
        }

        public async Task RemoveUserFromBlackList(Guid userId, Guid unblockedUserId)
        {
            var blackListItem = await GetBlackListItemById(userId, unblockedUserId);
            _context.BlackList.Remove(blackListItem);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAvatarModel>> GetBlackList(Guid userId)
        {
            var blackList = await _context.BlackList
                .Where(x => x.UserId == userId)
                .Include(x => x.BlockedUser).ThenInclude(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x.BlockedUser))
                .ToListAsync();

            return blackList;
        }

        private async Task<bool> CheckUserExistInBlackList(Guid userId, Guid blockedUserId)
        {
            return await _context.BlackList.AnyAsync(x => x.UserId == userId && x.BlockedUserId == blockedUserId);
        }

        private async Task<DAL.Entities.BlackListItem> GetBlackListItemById(Guid userId, Guid blockedUserId)
        {
            var blackListItem = await _context.BlackList.FirstOrDefaultAsync(x => x.UserId == userId && x.BlockedUserId == blockedUserId);
            if (blackListItem == null)
                throw new NotFoundException("user in black list not found");

            return blackListItem;
        }
    }
}
