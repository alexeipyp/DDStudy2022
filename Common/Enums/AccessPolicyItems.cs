namespace Common.Enums
{
    [Flags]
    public enum AccessPolicyItems
    {
        None = 0,
        ApplyBlackList = 1 << 0,
        ApplyMuteList = 1 << 1,
        ExcludeUnsubscribedPrivateUsers = 1 << 2,
        ExcludeCurrentUser = 1 << 3,
        OnlySubscribed = 1 << 4,
        OnlyFollowers = 1 << 5,
        OnlyPublicUsers = 1 << 6,
    }
}