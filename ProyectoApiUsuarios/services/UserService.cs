using MongoDB.Driver;
using ProyectoApiUsuarios.Models;
using ProyectoApiUsuarios.Controllers;

namespace ProyectoApiUsuarios.Services
{
    public class UserService
    {
        private readonly IMongoCollection<UserModel> _users;

        public UserService(IMongoDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<UserModel>("Users");
        }

        public async Task CreateUserAsync(UserModel user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<UserModel> GetUserByEmailAsync(string email)
        {
            return await _users.Find(user => user.Email == email).FirstOrDefaultAsync();
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task<UserModel> ValidateUserCredentialsAsync(string email, string password)
        {
            return await _users.Find(user => user.Email == email && user.Password == password).FirstOrDefaultAsync();
        }
    }
}

namespace ProyectoApiUsuarios.Services
{
    public interface IMongoDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string UsersCollectionName { get; set; }
    }

    public class MongoDatabaseSettings : IMongoDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCollectionName { get; set; }
    }
}

