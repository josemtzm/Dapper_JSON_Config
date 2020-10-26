using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class Procedimientos
    {
        //[JsonProperty("Nombre")]
        public string Nombre { get; set; }
        //[JsonProperty("Procedimiento")]
        public string Procedimiento { get; set; }
        //[JsonProperty("Parametros")]
        public IList<Parametros> Parametros { get; set; }

    }
}
