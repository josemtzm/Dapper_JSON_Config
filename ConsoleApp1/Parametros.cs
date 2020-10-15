using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ConsoleApp1
{
    public class Parametros
    {
        public string Nombre { get; set; }
        public object Valor { get; set; }
        public OracleDbType Tipo { get; set; }
        public ParameterDirection Direccion { get; set; }
        public int? Tamanio { get; set; }
        public AtributoEntidad AtributosEntd { get; set; }
    }
}