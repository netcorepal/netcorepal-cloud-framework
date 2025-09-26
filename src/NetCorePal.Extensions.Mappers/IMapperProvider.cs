namespace NetCorePal.Extensions.Mappers
{

    public interface IMapperProvider
    {
        IMapper<TFrom, TTo> GetMapper<TFrom, TTo>() where TFrom : class where TTo : class;
        object GetMapper(Type mapperType);
    }
}
