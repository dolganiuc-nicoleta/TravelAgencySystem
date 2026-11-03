using System;
using System.Linq;
using TravelAgency.Core.Data.Repositories;
using TravelAgency.Core.Models.Users;
using TravelAgency.Core.Models.Users.Access;

namespace TravelAgency.Core.Services
{
    /// <summary>
    /// Creates demo accounts for local development. Login is by e-mail, so every demo account needs one.
    /// </summary>
    public class UserSeedService
    {
        public const string DemoAgentEmail = "agent1@travelagency.demo";
        public const string DemoClientEmail = "client1@travelagency.demo";

        private readonly IUserRepository _userRepository;

        public UserSeedService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public void SeedDefaultUsers()
        {
            EnsureUser("agent1", DemoAgentEmail, "Agent123!", "Agent", () => new Agent());
            EnsureUser("client1", DemoClientEmail, "Client123!", "Client", () => new Client());
        }

        private void EnsureUser(string username, string email, string password, string roleName, Func<User> create)
        {
            var existing = _userRepository.GetAll().FirstOrDefault(x => x.Username == username);

            if (existing == null)
            {
                var user = create();
                user.Username = username;
                user.Email = email;
                user.PasswordHash = PasswordHasher.Hash(password);
                user.Role = new Role { Name = roleName };
                user.Session = new UserSession { IsActive = false };

                _userRepository.Add(user);
                return;
            }

            // Accounts seeded by earlier versions had no e-mail and therefore could not log in.
            if (string.IsNullOrWhiteSpace(existing.Email) && _userRepository.GetByEmail(email) == null)
            {
                existing.Email = email;
                _userRepository.Update(existing);
            }
        }
    }
}
