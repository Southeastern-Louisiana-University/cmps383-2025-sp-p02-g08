namespace Selu383.SP25.P02.Api.Features.Users
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = new string[0];
    }
}
