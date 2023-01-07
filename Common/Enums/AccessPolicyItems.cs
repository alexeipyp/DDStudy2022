namespace Common.Enums
{
    [Flags]
    public enum AccessPolicyItems
    {
        None = 0,
        ApplyBlackList = 1 << 0,
        ApplyMuteList = 1 << 1,
        ExcludeUnsubscribedPrivateUsers = 1 << 2,
        ExcludePrivateUsers = 1 << 3,
    }
}