﻿using MongoDB.Bson;
using MongoDB.Driver;
using ProyectoApiUsuarios.models;

namespace ProyectoApiUsuarios.Services
{
    public class MedicoService
    {
        private readonly IMongoCollection<Medico> _Medico;

        public MedicoService(IMongoClient mongoClient, IMongoDatabaseSettings settings)
    {
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _Medico = database.GetCollection<Medico>("Medicos");
    }


        public async Task<List<Medico>> GetMedicosAsync()
        {
            return await _Medico.Find(patient => true).ToListAsync();
        }

        public async Task CreateMedicoAsync(Medico Medico)
        {
            await _Medico.InsertOneAsync(Medico);
        }

        public async Task<Medico> GetMedicoAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _Medico.Find(m => m.Id == objectId.ToString()).FirstOrDefaultAsync();
        }

        public async Task AssignRoleAsync(string userId, string role)
        {
            var user = await _Medico.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null && (role == "admin" || role == "medico"))
            {
                if (!user.Roles.Contains(role))
                {
                    user.Roles.Add(role);
                    var filter = Builders<Medico>.Filter.Eq(u => u.Id, userId);
                    var update = Builders<Medico>.Update.Set(u => u.Roles, user.Roles);
                    await _Medico.UpdateOneAsync(filter, update);
                }
            }
        }

        public async Task<Medico> GetMedicoByEmailAsync(string email)
        {
            return await _Medico.Find(m => m.Email == email).FirstOrDefaultAsync();
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

}
