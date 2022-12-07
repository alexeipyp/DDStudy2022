using Api.Configs;
using Api.Models.Token;
using AutoMapper;
using Common;
using Common.CustomExceptions;
using Common.CustomExceptions.ForbiddenExceptions;
using Common.CustomExceptions.UnauthorizedExceptions;
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
    public class AuthService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private readonly AuthConfig _config;

        public AuthService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredentials(login, password);
            var session = _mapper.Map<DAL.Entities.UserSession>(user);
            await _context.UserSessions.AddAsync(session);

            await _context.SaveChangesAsync();
            return GenerateTokens(session);
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if (session == null)
            {
                throw new InvalidSessionException();
            }
            return session;
        }

        private async Task<UserSession> GetSessionByRefreshTokenId(Guid id)
        {
            var session = await _context.UserSessions.Include(x => x.User).FirstOrDefaultAsync(x => x.RefreshTokenId == id);
            if (session == null)
            {
                throw new InvalidSessionException();
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
                    throw new InvalidSessionException();
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

        private async Task<DAL.Entities.User> GetUserByCredentials(string login, string pass)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.ToLower());
            if (user == null)
                throw new UnauthorizedException("user not recognized");

            if (!HashHelper.Verify(pass, user.PasswordHash))
                throw new UnauthorizedException("user not recognized");

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

            return new TokenModel
            {
                AccessToken = encodedJwt,
                RefreshToken = encodedRefresh
            };
        }
    }
}
