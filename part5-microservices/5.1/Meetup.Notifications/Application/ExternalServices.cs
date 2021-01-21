using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Meetup.Notifications.Application
{
    public delegate Task<IEnumerable<Guid>> GetGroupMembers(string groupSlug);

    public delegate Task<IEnumerable<Guid>> GetMeetupAttendants(Guid meetupId);

    public delegate Task<IEnumerable<Guid>> GetInterestedUsers(Guid groupId);

    public delegate Task<Guid> GetGroupOrganizer(Guid groupId);
}