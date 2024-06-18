using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProyectoApiUsuarios.Models
{
      public class UpdatePatientDto
{
    public string IdentificationType { get; set; }
    public string IdentificationNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Phone { get; set; }
}

    }
