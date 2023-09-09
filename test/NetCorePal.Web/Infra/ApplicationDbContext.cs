using MediatR;
using Microsoft.EntityFrameworkCore;
using NetCorePal.Extensions.Repository.EntityframeworkCore;

namespace NetCorePal.Web.Infra
{
    public class ApplicationDbContext : EFContext
    {
        public ApplicationDbContext(DbContextOptions options, IMediator mediator, IServiceProvider provider) : base(options, mediator, provider)
        {
        }
    }
}
