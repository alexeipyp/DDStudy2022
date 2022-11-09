using Api.Models.Token;
using Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }


        [HttpPost]
        public async Task<TokenModel> Token(TokenRequestModel model) => await _authService.GetToken(model.Login, model.Password);

        [HttpPost]
        public async Task<TokenModel> RefreshToken(RefreshTokenRequestModel model) 
            => await _authService.GetTokenByRefreshToken(model.RefreshToken);
    }
}
