using Api.Configs;
using Api.Models.Attachments;
using Api.Models.User;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.CustomExceptions;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.NotFoundExceptions;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Api.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _context = context;
        }
        public async Task CreateUser(CreateUserModel model)
        {
            if (await CheckUserExist(model.Email))
            {
                throw new UserCreationException("user with the same email exists");
            }
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();
        }

        public async Task<List<UserAvatarModel>> GetUsers()
        {
            return await _context.Users.AsNoTracking()
                .Include(x => x.Avatar)
                .Select(x => _mapper.Map<UserAvatarModel>(x))
                .ToListAsync();
        }

        public async Task<UserAvatarModel> GetUser (Guid id)
        {
            var user = await GetUserWithAvatarById(id);

            return _mapper.Map<UserAvatarModel>(user);
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel model)
        {
            var user = await GetUserWithAvatarById(userId);
            if (user != null)
            {
                var request = _mapper.Map<AddAvatarRequest>(model, o => o.AfterMap((s, d) => d.AuthorId = userId));
                var withPathModel = _mapper.Map<AddAvatarRequest, MetaPathModel>(request);
                user.Avatar = _mapper.Map<DAL.Entities.Avatar>(withPathModel);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await GetUserWithAvatarById(userId);
            var attach = _mapper.Map<AttachModel>(user.Avatar);
            return attach;
        }

        private async Task<bool> CheckUserExist(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        } 

        private async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new UserNotFoundException("user not found");

            return user;
        }

        private async Task<DAL.Entities.User> GetUserWithAvatarById(Guid id)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                throw new UserNotFoundException("user not found");

            return user;
        }
    }
}
