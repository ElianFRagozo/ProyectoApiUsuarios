using System;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ProyectoApiUsuarios.Models
{
    public class Patient
    {
        [BsonId] 
        [BsonRepresentation(BsonType.ObjectId)] 
        public string Id { get; set; }

        [Required(ErrorMessage = "El tipo de identificación es obligatorio.")]
        public string IdentificationType { get; set; }

        [Required(ErrorMessage = "El número de identificación es obligatorio.")]
        public string IdentificationNumber { get; set; }

        [Required(ErrorMessage = "Los nombres son obligatorios.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Los apellidos son obligatorios.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
        [DataType(DataType.Date)]
        [BsonElement("DateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "El numero de celular es obligatorio.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "El correo electronico es obligatorio.")]
        public string Email {  get; set; }

        [Required(ErrorMessage = "El correo electronico es obligatorio.")]
        public string ConfirmEmail { get; set; }


        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password {  get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string ConfirmPassword { get; set; }

        public List<string> Roles { get; set; }



        [BsonIgnore]
        public string Identification
        {
            get { return $"{IdentificationType}-{IdentificationNumber}"; }
        }
    }
}
