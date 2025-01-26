using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Dto;
using NetCorePal.Extensions.Jwt;

namespace NetCorePal.Web.Controllers;

/// <summary>
/// 
/// </summary>
public class UserController
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dbContext"></param>
    /// <returns></returns>
    public async Task<ResponseData<string>> CreateUser([FromServices] ApplicationDbContext dbContext)
    {
        var user = new IdentityUser("test");
        //dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return new ResponseData<string>(user.Id);
    }

    [HttpGet("/login")]
    public async Task<ResponseData<bool>> Login([FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext!;
        var principal = new ClaimsPrincipal();

        var identity = new ClaimsIdentity(authenticationType: "test");
        identity.AddClaim(new Claim(ClaimTypes.Name, "test"));
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, "test"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "User"));
        principal.AddIdentity(identity);
        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        return true.AsResponseData();
    }

    [HttpGet("/empty")]
    public Task<ResponseData<bool>> Empty([FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext!;
        return Task.FromResult(true).AsResponseData();
    }

    [HttpPost("/jwtlogin")]
    public async Task<ResponseData<string>> JwtLogin(string name, [FromServices] IJwtProvider provider)
    {
        var claims = new[]
        {
            new Claim("uid", "111"),
            new Claim("type", "client"),
            new Claim("email", "abc@efg.com"),
        };
        var jwt = await provider.GenerateJwtToken(new JwtData("issuer-x", "audience-y",
            claims,
            DateTime.Now,
            DateTime.Now.AddMinutes(1)));
        return jwt.AsResponseData();
    }

    [HttpGet("/jwt")]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public Task<ResponseData<bool>> Jwt([FromServices] IHttpContextAccessor httpContextAccessor)
    {
        var context = httpContextAccessor.HttpContext!;
        return Task.FromResult(true).AsResponseData();
    }
}