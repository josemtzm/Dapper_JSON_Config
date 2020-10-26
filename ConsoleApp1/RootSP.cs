using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleApp1
{
    public class RootSP
    {
        //[JsonProperty("data")]
        public List<Procedimientos> data { get; set; }
    }
}
