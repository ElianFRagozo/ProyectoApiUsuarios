using ProyectoApiUsuarios.models;
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

    private readonly List<string> _especialidades = new List<string>
{
    "Medicina General",
    "Examen Odontológico de Primera Vez y Control",
    "Orientación Médica",
    "Vacunación Covid 19",
    "Valoración Integral por Médico",
    "Valoración Integral por Enfermera",
    "Atención en Salud Sexual y Reproductiva"
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
            return Ok(_especialidades);
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

            var especialidadesValidas = medico.Especialidades.Select(id => id.ToString()).Intersect(_especialidades).ToList();
            var especialidadesInvalidas = medico.Especialidades.Select(id => id.ToString()).Except(_especialidades).ToList();


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
