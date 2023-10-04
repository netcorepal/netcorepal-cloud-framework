using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace NetCorePal.Web.Controllers;

public class UserController
{
    public async Task<ResponseData<string>> CreateUser([FromServices] ApplicationDbContext dbContext)
    {
        var user = new IdentityUser("test");
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
        return new ResponseData<string>(user.Id);
    }
}