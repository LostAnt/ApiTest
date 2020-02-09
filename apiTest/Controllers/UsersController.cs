using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using apiTest.Services;
using AutoMapper;
using apiTest.Helpers;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using apiTest.Models;
using apiTest.Entities;
using System.Collections.Generic;

namespace apiTest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UsersController: ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController(
            IUserService userService,
            IMapper mapper,
            IOptions<AppSettings> appSettings)
        {
            _userService = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody]AuthenticateModel model)
        {
            var user = _userService.Authenticate(model.Login, model.Password);

            if (user == null)
                return BadRequest(new { message = "Username or password is incorrect" });

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                user.Id,
                user.Login,
                user.Name,
                user.Surname,
                user.Patronymic,
                Token = tokenString,
                user.Role
            });
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]RegisterModel model)
        {
            var user = _mapper.Map<User>(model);

            try
            {
                _userService.Create(user, model.Password);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            var model = _mapper.Map<IList<UserModel>>(users);

            return Ok(model);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var currentUserId = int.Parse(User.Identity.Name);

            if (id != currentUserId && !User.IsInRole(Role.Admin))
                return Forbid();

            var user = _userService.GetById(id);

            var model = _mapper.Map<UserModel>(user);

            return Ok(model);
        }

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UpdateModel model)
        {
            var currentUserId = int.Parse(User.Identity.Name);

            if (id != currentUserId && !User.IsInRole(Role.Admin))
                return Forbid();

            var user = _mapper.Map<User>(model);

            user.Id = id;

            try
            {
                _userService.Update(user, model.Password);

                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Roles = Role.Admin)]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userService.Delete(id);

            return Ok();
        }
    }
}
