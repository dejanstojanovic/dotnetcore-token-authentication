using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sample.Core.Identity.Data.Enities;

namespace Sample.Core.Identity.Api.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AccountController : ControllerBase
    {
        readonly UserManager<ApplicationUser> userManager;
        readonly SignInManager<ApplicationUser> signInManager;
        readonly IConfiguration configuration;
        readonly ILogger<AccountController> logger;


        public AccountController(
           UserManager<ApplicationUser> userManager,
           SignInManager<ApplicationUser> signInManager,
           IConfiguration configuration,
           ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.configuration = configuration;
            this.logger = logger;
        }


        [HttpPost]
        [Route("token")]
        public async Task<IActionResult> CreateToken([FromBody] Credentials credentials)
        {


            var loginResult = await signInManager.PasswordSignInAsync(credentials.Username, credentials.Password, isPersistent: false, lockoutOnFailure: false);

            if (!loginResult.Succeeded)
            {
                return BadRequest();
            }


            var user = await userManager.FindByNameAsync(credentials.Username);

            return Ok(GetToken(user));

        }

        [Authorize]
        [HttpPost]
        [Route("refreshtoken")]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await userManager.FindByNameAsync(
                User.Identity.Name ??
                User.Claims.Where(c => c.Properties.ContainsKey("unique_name")).Select(c => c.Value).FirstOrDefault()
                );
            return Ok(GetToken(user));
        }


        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] Credentials credentials)
        {
            var user = new IdentityUser
            {
                UserName = credentials.Username
            };

            var identityResult = await this.userManager.CreateAsync(user, credentials.Password);
            if (identityResult.Succeeded)
            {
                await signInManager.SignInAsync(user, isPersistent: false);
                return Ok(GetToken(user));
            }
            else
            {
                return BadRequest(identityResult.Errors);
            }

        }

        private String GetToken(IdentityUser user)
        {
            var utcNow = DateTime.UtcNow;

            var claims = new Claim[]
            {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                    new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, utcNow.ToString())
            };

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Tokens:Key"]));
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var jwt = new JwtSecurityToken(
                signingCredentials: signingCredentials,
                claims: claims,
                notBefore: utcNow,
                expires: utcNow.AddSeconds(this.configuration.Get<int>("Tokens:Lifetime")),
                audience: this.configuration["Tokens:Audience"],
                issuer: this.configuration["Tokens:Issuer"]
                );

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }




    }
}