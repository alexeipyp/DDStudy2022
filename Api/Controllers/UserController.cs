using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Api.Models;
using AutoMapper;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using Api.Services;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;

        public UserController(UserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task CreateUser(CreateUserModel model) => await _userService.CreateUser(model);

        [HttpGet]
        [Authorize]
        public async Task<List<UserModel>> GetUsers() => await _userService.GetUsers();

        [HttpGet]
        [Authorize]
        public async Task<UserModel> GetCurrentUser()
        {
            var userIdString = User.Claims.FirstOrDefault(x => x.Type == "id")?.Value;
            if (Guid.TryParse(userIdString, out Guid userId))
            {
                return await _userService.GetUser(userId);
            }
            else
                throw new Exception("you are not authorized");
        }

    }
}
