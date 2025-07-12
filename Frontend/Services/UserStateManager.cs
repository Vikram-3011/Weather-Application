using Frontend.Models;
using System;

namespace Frontend.Services
{
    public class UserStateManager
    {
        public User? User { get; private set; }

        public string? UserEmail => User?.Email;

        public event Action? OnChange;

        public void SetUser(User? user)
        {
            User = user;
            NotifyStateChanged();
            Console.WriteLine($"User state updated: {user?.Email}");

        }

        public void ClearUser()
        {
            User = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();

        public bool IsLoggedIn() => User != null;
    }
}
