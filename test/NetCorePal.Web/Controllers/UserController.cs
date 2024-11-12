using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NetCorePal.Extensions.Dto;

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
}