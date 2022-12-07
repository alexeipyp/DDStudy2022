using Api.Models.BlackList;
using Api.Models.MuteList;
using Api.Models.User;
using AutoMapper;
using Common.Consts;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.UnauthorizedExceptions;
using Common.CustomExceptions.NotFoundExceptions;
using DAL;
using DAL.Entities;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Services
{
    public class MuteListService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        public MuteListService(IMapper mapper, DataContext context)
        {
            _mapper = mapper;
            _context = context;
        }

        public async Task AddUserToMuteList(Guid userId, AddUserToMuteListRequest request)
        {
            if (await CheckUserExistInMuteList(userId, request.MutedUserId))
                throw new ForbiddenException("user already in mute list");

            var dbMuteListItem = _mapper.Map<DAL.Entities.MuteListItem>(request, o => o.AfterMap((s, d) => d.UserId = userId));
            await _context.MuteList.AddAsync(dbMuteListItem);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveUserFromMuteList(Guid userId, Guid unmutedUserId)
        {
            var muteListItem = await GetMuteListItemById(userId, unmutedUserId);
            _context.MuteList.Remove(muteListItem);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserAvatarModel>> GetMuteList(Guid userId)
        {
            var mutedList = await _context.MuteList
                .Where(x => x.UserId == userId)
                .Include(x => x.MutedUser).ThenInclude(x => x.Avatar)
                .AsNoTracking()
                .Select(x => _mapper.Map<UserAvatarModel>(x.MutedUser))
                .ToListAsync();

            return mutedList;
        }

        private async Task<bool> CheckUserExistInMuteList(Guid userId, Guid mutedUserId)
        {
            return await _context.MuteList
                .AsNoTracking()
                .AnyAsync(x => x.UserId == userId && x.MutedUserId == mutedUserId);
        }

        private async Task<DAL.Entities.MuteListItem> GetMuteListItemById(Guid userId, Guid mutedUserId)
        {
            var muteListItem = await _context.MuteList
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId && x.MutedUserId == mutedUserId);
            if (muteListItem == null)
                throw new NotFoundException("user in mute list not found");

            return muteListItem;
        }
    }
}
