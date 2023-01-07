namespace Common.Enums
{
    public enum SubscribesFilterOption
    {
        None,
        Only,
        Exclude,
    }

    public enum CurrentUserFilterOption
    {
        None,
        Exclude,
    }

    public class FilterOptions
    {
        public SubscribesFilterOption SubscribesFilter { get; set; }
        public CurrentUserFilterOption CurrentUserFilter { get; set; }
    }
}