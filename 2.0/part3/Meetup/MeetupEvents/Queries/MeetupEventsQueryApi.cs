using System;
using System.Linq;
using System.Threading.Tasks;
using MeetupEvents.Contracts;
using MeetupEvents.Domain;
using MeetupEvents.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MeetupEvents.Queries
{
    [ApiController]
    [Route("/api/meetup/events")]
    public class MeetupEventsQueryApi : ControllerBase
    {
        readonly MeetupEventDbContext Database;

        public MeetupEventsQueryApi(MeetupEventDbContext database)
        {
            Database = database;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var meetups = await Database.MeetupEvents
                .Include(x => x.Attendants)
                .AsNoTracking()
                .ToListAsync();

            return Ok(meetups.Select(Map));
        }

        [HttpGet("{id:Guid}")]
        public async Task<IActionResult> Get(Guid id) =>
            await Database.MeetupEvents
                    .Include(x => x.Attendants)
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x => x.Id == id)
                switch
                {
                    null       => NotFound($"Meetup event {id} not found"),
                    var meetup => Ok(Map(meetup)),
                };

        ReadModels.V1.MeetupEvent Map(MeetupEventAggregate aggregate) =>
            new(
                aggregate.Id,
                aggregate.Title,
                aggregate.Status.ToString(),
                aggregate.Capacity,
                aggregate.Attendants.Select(x => new ReadModels.V1.Attendant(x.UserId, x.At, x.Waiting)).ToList()
            );
    };
}