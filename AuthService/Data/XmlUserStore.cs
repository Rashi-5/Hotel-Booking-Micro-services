using HotelBookingSystem.Models.Auth;
using System.Xml.Linq;
using HotelBookingSystem.Helper;

namespace HotelBookingSystem.Services
{
  public class XmlUserStore : IUserStore
    {
        private readonly string _xmlFilePath;
        private List<User> _users;

        public XmlUserStore(string xmlFilePath)
        {
            _xmlFilePath = xmlFilePath;
            LoadUsers();
        }

        private void LoadUsers()
        {
            try
            {
                if (!File.Exists(_xmlFilePath))
                {
                    CreateDefaultXmlFile();
                }

                var doc = XDocument.Load(_xmlFilePath);
                _users = doc.Descendants("User").Select((x, index) => new User
                {
                    Id = index + 1,
                    Username = x.Element("Username")?.Value,
                    Password = x.Element("Password")?.Value,
                    Role = x.Element("Role")?.Value
                }).ToList();
            }
            catch (Exception)
            {
                CreateDefaultXmlFile();
                LoadUsers();
            }
        }

        private void CreateDefaultXmlFile()
        {
            var defaultUsers = new XDocument(
                new XElement("Users",
                    new XElement("User",
                        new XElement("Username", "admin"),
                        new XElement("Password", "admin123"),
                        new XElement("Role", "Admin")
                    ),
                    new XElement("User",
                        new XElement("Username", "user1"),
                        new XElement("Password", "user123"),
                        new XElement("Role", "User")
                    )
                )
            );
            defaultUsers.Save(_xmlFilePath);
        }

        public User ValidateUser(string username, string password)
        {
            var hashedPassword = PasswordHasher.Hash(password);
            return _users.FirstOrDefault(u => 
                u.Username == username && u.Password == hashedPassword);
        }

        public User GetUserByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        public IEnumerable<User> GetAllUsers()
        {
            return _users;
        }
        public void AddUser(User user)
        {
            user.Password = PasswordHasher.Hash(user.Password);
            _users.Add(user);
            SaveUsersToXml();
        }

        public bool UserExists(string username)
        {
            return _users.Any(u => u.Username == username);
        }
         private void SaveUsersToXml()
    {
        var doc = new XDocument(
            new XElement("Users",
                _users.Select(u =>
                    new XElement("User",
                        new XElement("Username", u.Username),
                        new XElement("Password", u.Password),
                        new XElement("Role", u.Role)
                    )
                )
            )
        );
        doc.Save(_xmlFilePath);
    }
    }
}
