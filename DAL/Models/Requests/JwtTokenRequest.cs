namespace DAL.Models.Requests
{
    public class JwtTokenRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
