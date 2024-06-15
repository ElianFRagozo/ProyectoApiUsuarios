using System.Collections.Generic;
using System.Threading.Tasks;
using ProyectoApiUsuarios.Models;
using MongoDB.Driver;
using MongoDB.Bson;

namespace ProyectoApiUsuarios.Services
{
    public class PatientService
    {
        private readonly IMongoCollection<Patient> _patients;

        public PatientService(IMongoDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _patients = database.GetCollection<Patient>("Patient");
        }


        public async Task<List<Patient>> GetPatientsAsync()
        {
            return await _patients.Find(patient => true).ToListAsync();
        }

        public async Task CreatePatientAsync(Patient patient)
        {
            await _patients.InsertOneAsync(patient); 
        }

        public async Task UpdatePatientAsync(string id, Patient patient)
        {
            var objectId = ObjectId.Parse(id);
            await _patients.ReplaceOneAsync(p => p.Id == objectId.ToString(), patient);
        }

        public async Task<Patient> GetPatientAsync(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
            {
                throw new FormatException("Invalid ObjectId format");
            }

            return await _patients.Find<Patient>(patient => patient.Id == objectId.ToString()).FirstOrDefaultAsync();
        }



        public async Task<Patient> GetPatientByUserIdAsync(string userId)
        {
            return await _patients.Find(p => p.Id == userId).FirstOrDefaultAsync();
        }

        public async Task<Patient> ValidateUserCredentialsAsync(string email, string password)
        {
            return await _patients.Find(patient => patient.Email == email && patient.Password == password).FirstOrDefaultAsync();
        }

        public async Task<Patient> GetPacienteByEmailAsync(string email)
        {
            return await _patients.Find(m => m.Email == email).FirstOrDefaultAsync();
        }
    }
}

namespace ProyectoApiUsuario.Services
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

