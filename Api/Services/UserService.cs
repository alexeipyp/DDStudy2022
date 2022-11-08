using Api.Configs;
using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using Common.CustomExceptions;
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
        private readonly AuthConfig _config;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
            Console.WriteLine($"<< User Service: {Guid.NewGuid()} >>");
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

        public async Task<List<UserModel>> GetUsers()
        {
            return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        public async Task<UserModel> GetUser (Guid id)
        {
            var user = await GetUserById(id);

            return _mapper.Map<UserModel>(user);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredention(login, password);
            var session = await _context.UserSessions.AddAsync(new DAL.Entities.UserSession
            {
                Id = Guid.NewGuid(),
                User = user,
                RefreshTokenId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
            });
            await _context.SaveChangesAsync();
            return GenerateTokens(session.Entity);
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if (session == null)
            {
                throw new SessionNotFoundException("session not found");
            }
            return session;
        }

        private async Task<UserSession> GetSessionByRefreshTokenId(Guid id)
        {
            var session = await _context.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshTokenId == id);
            if (session == null)
            {
                throw new SessionNotFoundException("session not found");
            }
            return session;
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParams = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.SymmetricSecuriryKey(),
            };


            if (new JwtSecurityToken(refreshToken) == null)
            {
                throw new SecurityTokenException("invalid token");
            }

            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParams, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken 
                || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("invalid token");
            }

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshTokenId")?.Value is String refreshTokenIdString
                && Guid.TryParse(refreshTokenIdString, out var refreshTokenId)) 
            {
                var session = await GetSessionByRefreshTokenId(refreshTokenId);
                if (!session.IsActive)
                {
                    throw new SessionNotActiveException("session not active");
                }

                session.RefreshTokenId = Guid.NewGuid();
                await _context.SaveChangesAsync();

                return GenerateTokens(session);
            }
            else
            {
                throw new SecurityTokenException("invalid token");
            }    
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await GetUserWithAvatarById(userId);
            if (user != null)
            {
                var avatar = new Avatar
                {
                    Id = new Guid(),
                    Author = user,
                    MimeType = meta.MimeType,
                    FilePath = filePath,
                    Name = meta.Name,
                    Size = meta.Size,
                };

                user.Avatar = avatar;

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

        private async Task<DAL.Entities.User> GetUserByCredention(string login, string pass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.ToLower());
            if (user == null)
                throw new UserNotFoundException("user not found");

            if (!HashHelper.Verify(pass, user.PasswordHash))
                throw new UserNotFoundException("user not found");

            return user;
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

        private TokenModel GenerateTokens(DAL.Entities.UserSession session)
        {
            var dtNow = DateTime.Now;
            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[] {
                    new Claim("id", session.User.Id.ToString()),
                    new Claim("sessionId", session.Id.ToString()),
                },
                expires: DateTime.Now.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecuriryKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[] {
                    new Claim("refreshTokenId", session.RefreshTokenId.ToString()),
                },
                expires: DateTime.Now.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecuriryKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel { 
                AccessToken = encodedJwt, 
                RefreshToken = encodedRefresh 
            };
        }
    }
}
