namespace NetCorePal.Web.UnitTests;

public class AsyncTests
{
    AsyncLocal<Abc> abc = new AsyncLocal<Abc>();

    [Fact]
    public async Task AsyncTest()
    {
        int b = 10;
        string a = $@"""
sdlkfjslf\sdfsd{b}fjs\sdfsdfs
""";
        var c = a;

    }

    class Abc
    {
        public string Name { get; set; }
    }
}