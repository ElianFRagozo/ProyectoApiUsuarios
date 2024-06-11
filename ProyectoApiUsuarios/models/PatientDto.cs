namespace ProyectoApiUsuarios.Models
{
        public class PatientDto
        {
            public string IdentificationType { get; set; }
            public string IdentificationNumber { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime DateOfBirth { get; set; }
            public string Phone {  get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string ConfirmEmail { get; set; }
            public string ConfirmPassword { get; set;}
            public string UserId {get; set;}
    }
    }
