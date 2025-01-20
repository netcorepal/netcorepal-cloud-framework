using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace NetCorePal.Web;

public class TestAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    private DefaultAuthorizationPolicyProvider BackupPolicyProvider { get; }
    private IHttpContextAccessor _contextAccessor;

    private AuthorizationPolicy _policy;

    public TestAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IHttpContextAccessor contextAccessor)
    {
        BackupPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
        _contextAccessor = contextAccessor;
        var policy = new AuthorizationPolicyBuilder();
        policy.RequireAuthenticatedUser();
        policy.AddAuthenticationSchemes("Aksk"); 
        _policy = policy.Build();
    }


    public async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var context = _contextAccessor.HttpContext;
        if (policyName.StartsWith("abc"))
        {
            //do something
            //return _policy;
        }

        return await BackupPolicyProvider.GetPolicyAsync(policyName);
    }

    public async Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return await BackupPolicyProvider.GetDefaultPolicyAsync();
    }

    public async Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return await BackupPolicyProvider.GetFallbackPolicyAsync();
    }
}