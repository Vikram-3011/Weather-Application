namespace Frontend.Models
{
    public class User
    {
        public string Email { get; set; }

        public User(string email)
        {
            Email = email;
        }
    }
}
