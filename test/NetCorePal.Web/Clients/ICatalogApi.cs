using Refit;

namespace NetCorePal.Web.Clients;

public interface ICatalogApi
{
    [Get("/api/v1/catalog")]
    Task<IEnumerable<CatalogItem>> GetCatalogItemsAsync();
}

public class CatalogItem
{
    
}