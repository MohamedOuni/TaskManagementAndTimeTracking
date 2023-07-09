using EY.TaskShare.Entities;
using EY.TaskShare.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace EY.TaskShare.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        public AuthenticateService _authenticateService = default!;
        public AuthenticateController(AuthenticateService authenticateService)
        {
            _authenticateService = authenticateService;

        }
        [HttpPost("Register")]
        public ActionResult<User> Register(User user)
        {
            _authenticateService.CreateUser(user);

            return Ok(user);
        }

        [HttpPost("Login")]
        public ActionResult<User> LoginUser(UserDetails userDetails)
        {

            var user = _authenticateService.LoginUser(userDetails);

            return Ok(JsonConvert.SerializeObject(user));

        }

    }
}