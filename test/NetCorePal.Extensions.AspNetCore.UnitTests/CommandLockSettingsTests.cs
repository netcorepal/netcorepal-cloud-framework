using NetCorePal.Extensions.AspNetCore.CommandLocks;

namespace NetCorePal.Extensions.AspNetCore.UnitTests;

public class CommandLockSettingsTests
{
    [Fact]
    public void SingeKey_NotEmpty_Should_Success()
    {
        var key = "abc";
        var settings = new CommandLockSettings(key);
        Assert.NotNull(settings);
        Assert.NotNull(settings.LockKey);
        Assert.Null(settings.LockKeys);
        Assert.Equal(key, settings.LockKey);
    }

    [Fact]
    public void SingeKey_Empty_Should_Throw()
    {
        Assert.Throws<ArgumentNullException>(() => new CommandLockSettings(string.Empty));
    }

    [Fact]
    public void MultipleKeys_NotEmpty_Should_Success()
    {
        var keys = new[] { "abc", "def" };
        var settings = new CommandLockSettings(keys);
        Assert.NotNull(settings);
        Assert.Null(settings.LockKey);
        Assert.NotNull(settings.LockKeys);
        Assert.Equal(keys.Length, settings.LockKeys.Count);
        Assert.Equal(keys[0], settings.LockKeys[0]);
        Assert.Equal(keys[1], settings.LockKeys[1]);
    }

    [Fact]
    public void MultipleKeys_Empty_Should_Throw()
    {
        Assert.Throws<ArgumentException>(() => new CommandLockSettings([string.Empty]));
    }

    [Fact]
    public void MultipleKeys_ContainsEmpty_Should_Throw()
    {
        Assert.Throws<ArgumentException>(() => new CommandLockSettings([string.Empty, "abc"]));
    }

    [Fact]
    public void MultipleKeys_ContainsEmptyString_Should_Throw()
    {
        Assert.Throws<ArgumentException>(() => new CommandLockSettings([null!, "abc"]));
    }

    [Fact]
    public void MultipleKeys_Contains_SameKey_Should_Throw()
    {
        Assert.Throws<ArgumentException>(() => new CommandLockSettings(["abc", "abc"]));
    }

    [Fact]
    public void MultipleKeys_Should_Ordered()
    {
        var keys = new[] { "def", "abc" };
        var settings = new CommandLockSettings(keys);
        Assert.NotNull(settings);
        Assert.Null(settings.LockKey);
        Assert.NotNull(settings.LockKeys);
        Assert.Equal(keys.Length, settings.LockKeys.Count);
        Assert.Equal("abc", settings.LockKeys[0]);
        Assert.Equal("def", settings.LockKeys[1]);
    }
}