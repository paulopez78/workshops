using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;

namespace Meetup.Scheduling.Queries
{
    public class MeetupEventPostgresQueries
    {
        public MeetupEventPostgresQueries(IDocumentStore documentStore)
            => DocumentStore = documentStore;

        private readonly IDocumentStore DocumentStore;

        public async Task<MeetupEvent?> Handle(V1.GetById query)
        {
            using var session = DocumentStore.QuerySession();
            return await session.LoadAsync<MeetupEvent>(query.EventId);
        }

        public async Task<IEnumerable<MeetupEvent>> Handle(V1.GetByGroup query)
        {
            using var session = DocumentStore.QuerySession();
            return await session.Query<MeetupEvent>().Where(x => x.Group == query.Group).ToListAsync();
        }
    }
}