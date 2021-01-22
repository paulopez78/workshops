﻿using System;

namespace Meetup.Notifications.Data
{
    public record Notification
    {
        public string           Id               { get; set; }
        public string           UserId           { get; set; }
        public string           Message          { get; set; }
        public string           GroupId          { get; set; }
        public string           MeetupId         { get; set; }
        public string           MemberId         { get; set; }
        public NotificationType NotificationType { get; set; }
    }

    public enum NotificationType
    {
        Message,
        NewGroupCreated,
        MeetupPublished,
        MeetupCancelled,
        MemberJoined,
        MemberLeft,
        Waiting,
        Attending
    }
}