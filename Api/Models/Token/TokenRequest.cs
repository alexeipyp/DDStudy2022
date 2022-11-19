namespace Api.Models.Token
{
    public class TokenRequest
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
