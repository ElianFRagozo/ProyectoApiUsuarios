using ProyectoApiUsuarios.models;
using ProyectoApiUsuarios.services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using ProyectoApiUsuarios.Models;
using ProyectoApiUsuarios.Services;
using MongoDB.Bson;

namespace ProyectoApiUsuarios.Controllers
{
    public class MedicoController : Controller
    {
        private static List<Especialidad> especialidades = new List<Especialidad>
    {
        new Especialidad { Id = 1, Nombre = "Medicina General" },
        new Especialidad { Id = 2, Nombre = "Examen Odontologico de Primera Vez y Control" },
        new Especialidad { Id = 3, Nombre = "Orientacion Medica" },
        new Especialidad { Id = 4, Nombre = "Vacunacion Covid 19" },
        new Especialidad { Id = 5, Nombre = "Valoracion integrales por Medico" },
        new Especialidad { Id = 6, Nombre = "Valoracion integrales por Enfermera" },
        new Especialidad { Id = 7, Nombre = "Atencion en Salud Sexual y Reproductiva" }
    };
        private readonly MedicoService  _medicoService;
        private readonly UserService _userService;

        public MedicoController(MedicoService medicoService, UserService userService)
        {
            _userService = userService;
            _medicoService = medicoService;
        }
        private static List<Medico> medicos = new List<Medico>();
      

        [HttpGet("{id}")]
        public IActionResult GetMedicoById(string id)
        {
            var medico = medicos.Find(m => m.Id == id);
            if (medico == null)
            {
                return NotFound();
            }

            return Ok(medico);
        }
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {

            var medicos = await _medicoService.GetMedicosAsync();
            return Ok(medicos);
        }

        [HttpGet("especialidades")]
        public IActionResult GetEspecialidades()
        {
            return Ok(especialidades);
        }

        [HttpPost("medicos")]
        public async Task<IActionResult> CreateMedicoAsync([FromBody] Medico medico)
        {
            UserModel _user = new UserModel();

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newId = ObjectId.GenerateNewId();
            medico.Id = newId.ToString();

            var especialidadesValidas = especialidades.Select(e => e.Id).ToList();
            var especialidadesInvalidas = medico.EspecialidadesIds.Except(especialidadesValidas).ToList();

            if (especialidadesInvalidas.Any())
            {
                return BadRequest($"Las siguientes especialidades no son válidas: {string.Join(", ", especialidadesInvalidas)}");
            }

            var newId2 = ObjectId.GenerateNewId();
            _user.Id = newId2.ToString();
            _user.Email= medico.Email;
            _user.Password = medico.Contraseña;
            _user.Roles = medico.Roles;
            

            await _userService.CreateUserAsync(_user);
            await _medicoService.CreateMedicoAsync(medico);
            CreatedAtAction(nameof(GetMedicoById), new { id = _user.Id }, _user);
            return CreatedAtAction(nameof(GetMedicoById), new { id = medico.Id }, medico) ;
        }
        
    }
}
