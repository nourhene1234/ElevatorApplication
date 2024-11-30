using MongoDB.Bson;

namespace crudmongo.Models
{
    public class User
    {
        public ObjectId Id { get; set; }  // MongoDB generates this automatically

        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; // Store hashed password
        public string Role { get; set; } = null;
    }
}
