namespace NetCorePal.Web.UnitTests;

public class AsyncTests
{
    AsyncLocal<Abc> abc = new AsyncLocal<Abc>();

    [Fact]
    public void AsyncTest()
    {
        int b = 10;
        string a = $@"""
sdlkfjslf\sdfsd{b}fjs\sdfsdfs
""";
        var c = a;

    }

    private class Abc
    {
        public string Name { get; set; } = string.Empty;
    }
}