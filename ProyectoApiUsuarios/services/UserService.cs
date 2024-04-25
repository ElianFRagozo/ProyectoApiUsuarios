using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using ProyectoApiUsuarios.Controllers;
using ProyectoApiUsuarios.Models;

namespace ProyectoApiUsuarios.Services
{
    public class UserService
    {
        private readonly IMongoCollection<UserModel> _users;

        public UserService(IMongoDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _users = database.GetCollection<UserModel>("users");
        }

        public async Task<List<UserModel>> GetUsersAsync()
        {
            return await _users.Find(user => true).ToListAsync();
        }

        public async Task CreateUserAsync(UserModel user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task<UserModel> AuthenticateAsync(string username, string password)
        {
            var user = await _users.Find(u => u.Username == username && u.Password == password).FirstOrDefaultAsync();
            return user;
        }

        public async Task<UserModel> GetUserByUsernameAsync(string username)
        {
            return await _users.Find(u => u.Username == username).FirstOrDefaultAsync();
        }


        public async Task<bool> IsAuthorizedAsync(string userId, string role)
        {
            var user = await _users.Find(u => u.Id == userId && u.Roles.Contains(role)).FirstOrDefaultAsync();
            return user != null;
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null && (role == "admin" || role == "medico" || role == "paciente"))
            {
                if (!user.Roles.Contains(role))
                {
                    user.Roles.Add(role); // Cambiar a user.Roles.Add(role)
                    var filter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
                    var update = Builders<UserModel>.Update.Set(u => u.Roles, user.Roles); // Cambiar a u.Roles
                    await _users.UpdateOneAsync(filter, update);
                }
            }
        }


        public async Task RevokeRoleAsync(string userId, string role)
        {
            var user = await _users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null && user.Roles.Contains(role))
            {
                user.Roles.Remove(role);
                var filter = Builders<UserModel>.Filter.Eq(u => u.Id, userId);
                var update = Builders<UserModel>.Update.Set(u => u.Roles, user.Roles);
                await _users.UpdateOneAsync(filter, update);
            }
        }
    }
 }


namespace ProyectoApiUsuarios.Controllers
{
    public interface IMongoDatabaseSettings
    {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string NotesCollectionName { get; set; }
    }

    public class MongoDatabaseSettings : IMongoDatabaseSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string NotesCollectionName { get; set; }
    }
}



