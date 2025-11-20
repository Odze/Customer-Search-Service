using api.Enums;

namespace api.Payloads
{
    public readonly struct RepoUpdatePayload
    {
        public RepoUpdateType Type { get; init; }
        public CustomerPayload? Payload { get; init; }
    }
}