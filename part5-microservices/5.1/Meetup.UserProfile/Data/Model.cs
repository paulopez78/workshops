﻿namespace Meetup.UserProfile.Data
{
    public record UserProfile
    {
        public string Id        { get; set; }
        public string FirstName { get; set; }
        public string LastName  { get; set; }
        public string Email     { get; set; }
    }
}