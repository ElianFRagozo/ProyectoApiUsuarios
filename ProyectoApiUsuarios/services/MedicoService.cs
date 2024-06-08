using MongoDB.Bson;
using MongoDB.Driver;
using ProyectoApiUsuarios.Controllers;
using ProyectoApiUsuarios.models;

namespace ProyectoApiUsuarios.services
{
    public class MedicoService
    {
        private readonly IMongoCollection<Medico> _Medico;

        public MedicoService(IMongoDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _Medico = database.GetCollection<Medico>("Medico");
        }


        public async Task<List<Medico>> GetMedicosAsync()
        {
            return await _Medico.Find(patient => true).ToListAsync();
        }

        public async Task CreateMedicoAsync(Medico Medico)
        {
            await _Medico.InsertOneAsync(Medico);
        }


       

        public async Task<Medico> GetPatientAsync(string id)
        {
            var objectId = ObjectId.Parse(id);
            return await _Medico.Find(m => m.Id == objectId.ToString()).FirstOrDefaultAsync();
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
