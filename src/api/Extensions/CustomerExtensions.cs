using api.Entity;
using api.Payloads;

namespace api.Extensions
{
    public static class CustomerExtensions
    {
        public static IEnumerable<CustomerPayload> ToPayloads(this IEnumerable<CustomerEntity> entities)
        {
            return entities.Select(entity => new CustomerPayload
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                Email = entity.Email
            });
        }
    }
}

