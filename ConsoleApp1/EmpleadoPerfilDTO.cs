using System.Collections.Generic;

namespace ConsoleApp1
{
    public class EmpleadoPerfilDTO
    {
        //public EmpleadoPerfilDTO() {
        //    Perfiles = new List<PerfilesDTO>();
        //}
        public string Nomina { get; set; }
        public string Num_consar { get; set; }
        public string Nomina_sap { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Nombres { get; set; }
        public IList<PerfilesDTO> Perfiles { get; set; }
        public PerfilesDTO Perfil { get; set; }
        public int Estatus { get; set; }
        public string Mensaje { get; set; }
    }
}