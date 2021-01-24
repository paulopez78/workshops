using System.Threading.Tasks;
using MassTransit;
using Meetup.Scheduling.AttendantList;
using Meetup.Scheduling.Framework;
using static Meetup.Scheduling.Contracts.MeetupDetailsCommands.V1;

namespace Meetup.Scheduling.MeetupDetails
{
    public class MeetupDetailsCommandApiAsync :
        IConsumer<CreateMeetup>,
        IConsumer<UpdateDetails>,
        IConsumer<MakeOnline>,
        IConsumer<MakeOnsite>,
        IConsumer<Schedule>,
        IConsumer<Publish>,
        IConsumer<Cancel>,
        IConsumer<Start>,
        IConsumer<Finish>
    {
        readonly HandleCommand<MeetupDetailsAggregate> HandleCommand;

        public MeetupDetailsCommandApiAsync(HandleCommand<MeetupDetailsAggregate> handleCommand)
            => HandleCommand = handleCommand;

        public Task Consume(ConsumeContext<CreateMeetup> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<UpdateDetails> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<MakeOnline> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<MakeOnsite> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Schedule> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Publish> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Cancel> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Start> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);

        public Task Consume(ConsumeContext<Finish> context)
            => HandleCommand.WithContext(context)
                (context.Message.EventId, context.Message);
    }
}