using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProyectoApiUsuarios.Models
{
    public class MedicoDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public List<string> Especialidades { get; set; }
        public List<string> Roles { get; set; }
    }
}
