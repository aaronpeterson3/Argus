using Argus.Core.Infrastructure.Events;

namespace Argus.Features.Users.Infrastructure.Events
{
    public class UserCreatedEvent : EventBase
    {
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    public class UserProfileUpdatedEvent : EventBase
    {
        public string Email { get; set; }
        public Dictionary<string, string> ChangedProperties { get; set; }
    }

    public class UserPasswordChangedEvent : EventBase
    {
        public string Email { get; set; }
        public bool WasReset { get; set; }
    }
}