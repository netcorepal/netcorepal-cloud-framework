using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using NetCorePal.Extensions.DependencyInjection;

namespace NetCorePal.Extensions.Mappers.UnitTests
{
    public class MapperTests
    {

        public MapperTests()
        {

        }


        [Fact]
        public void AddMapperPrivider_Test()
        {
            ServiceCollection services = new();
            var assembly = Assembly.GetExecutingAssembly();
            services.AddMapperPrivider(assembly);
            Assert.Equal(2, services.Count);
            var serviceProvider = services.BuildServiceProvider();
            var mapperProvider = serviceProvider.GetService<IMapperProvider>();
            Assert.NotNull(mapperProvider);
            var myMapper = serviceProvider.GetService<IMapper<From, To>>();
            Assert.NotNull(myMapper);
        }


        [Fact]
        public void MapTo_Test()
        {
            ServiceCollection services = new();
            var assembly = Assembly.GetExecutingAssembly();
            services.AddMapperPrivider(assembly);
            var serviceProvider = services.BuildServiceProvider();
            var mapperProvider = serviceProvider.GetRequiredService<IMapperProvider>();

            var from = new From { Name = "abc" };
            var to = from.MapTo<From, To>(mapperProvider);

            Assert.Equal("abc", to.Name2);
        }



        class From
        {
            public string? Name { get; init; }
        }

        class To
        {
            public string? Name2 { get; init; }
        }


        public class MyMapper : IMapper<From, To>
        {
            To IMapper<From, To>.To(From from)
            {
                ArgumentNullException.ThrowIfNull(from);
                return new To { Name2 = from.Name };
            }
        }
    }
}