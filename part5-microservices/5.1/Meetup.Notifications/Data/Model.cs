namespace Meetup.Notifications.Data
{
    public record Notification
    {
        public string           Id               { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationType
    {
        NewGroupCreated,
        MemberJoined,
        MemberLeft,
        MovedToWaitingList,
        Attending
    }

    public record MemberJoinedGroupNotification
    {
    }
}