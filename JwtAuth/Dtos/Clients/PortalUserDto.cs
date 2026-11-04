namespace JwtAuth.Dtos.Clients
{
    public class PortalUserDto
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public bool IsAssigned { get; set; }
    }
}