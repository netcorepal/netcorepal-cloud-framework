namespace NetCorePal.Extensions.AspNetCore.CommandLocks;

class CommandLockedKeysHolder
{
    private static readonly AsyncLocal<KeysHolder> KeysHolderCurrent = new AsyncLocal<KeysHolder>();

    public KeysHolder LockedKeys => KeysHolderCurrent.Value ??= new KeysHolder();
}

class KeysHolder
{
    public HashSet<string> Keys { get; set; } = new HashSet<string>();
}