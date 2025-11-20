using api.Payloads;

namespace api.Entity
{
    public sealed class CustomerEntity
    {
        public int Id { get; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string[] Email { get; private set; }

        public CustomerEntity(CustomerPayload payload)
        {
            Id = payload.Id;
            FirstName = payload.FirstName;
            LastName = payload.LastName;
            Email = payload.Email;
        }

        public bool Patch(CustomerPayload payload)
        {
            bool updated = false;

            if (!string.IsNullOrWhiteSpace(payload.FirstName) && payload.FirstName != FirstName)
            {
                FirstName = payload.FirstName;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(payload.LastName) && payload.LastName != LastName)
            {
                LastName = payload.LastName;
                updated = true;
            }

            if (payload.Email != null && !payload.Email.SequenceEqual(Email))
            {
                Email = payload.Email;
                updated = true;
            }

            return updated;
        }
    }
}