namespace Selu383.SP25.P02.Api.Features.Users
{
    public class CreateUserDto
    {
        public string UserName { get; set; } = string.Empty;
        public string[] Roles { get; set; } = new string[0];
        public string Password { get; set; } = string.Empty;

    }
}
