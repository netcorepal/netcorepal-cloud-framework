namespace NetCorePal.Extensions.Repository.EntityFrameworkCore;

public interface ITenantToDatabaseConvertor
{
    string Convert(string tenantId);
}