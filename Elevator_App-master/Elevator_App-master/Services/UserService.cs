using crudmongo.Configurations;
using crudmongo.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace crudmongo.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _userCollection;

        public UserService(IOptions<DatabaseSettings> databaseSettings)
        {
            var settings = databaseSettings.Value;
            var mongoClient = new MongoClient(databaseSettings.Value.ConnectionString);
            var mongoDb = mongoClient.GetDatabase(databaseSettings.Value.DatabaseName);
            _userCollection = mongoDb.GetCollection<User>(settings.UsersCollection);

        }

        public async Task<List<User>> GetAllAsync() =>
            await _userCollection.Find(_ => true).ToListAsync();

        public async Task<User?> GetByUsernameAsync(string username) =>
            await _userCollection.Find(x => x.Username == username).FirstOrDefaultAsync();

        public async Task<User?> GetByEmailAsync(string email) =>
            await _userCollection.Find(x => x.Email == email).FirstOrDefaultAsync();


        public async Task<User?> GetByRoleAsync(string role) =>
            await _userCollection.Find(x => x.Role == role).FirstOrDefaultAsync();


        public async Task RegisterUserAsync(User user)
        {
            // Check if username or email already exists
            if (await GetByUsernameAsync(user.Username) != null)
                throw new Exception("Username already taken.");
            if (await GetByEmailAsync(user.Email) != null)
                throw new Exception("Email already registered.");

               await GetByRoleAsync(user.Role);
               

            // Hash password before saving
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            // Insert the user into the database
            await _userCollection.InsertOneAsync(user);
        }

        public async Task<bool> ValidateUserCredentials(string username, string password)
        {
            var user = await GetByUsernameAsync(username);
            if (user == null) return false;

            // Verify the password
            return BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        }
    }
}
