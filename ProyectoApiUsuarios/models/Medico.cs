namespace ProyectoApiUsuarios.models
{
    public class Medico
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Contraseña { get; set; }
        public List<int> EspecialidadesIds { get; set; }
        public List<string> Roles { get; set; }
    }
}
