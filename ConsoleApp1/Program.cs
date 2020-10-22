using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine(EventsDateTimeRecipient().Content.ToString());

            //Console.WriteLine(SendSimpleMessage().Content.ToString());


            Console.WriteLine("Hello World!");
            RootSP lstProcedimientos = ProcedimientosJSON();


            /*Procedimientos proc = lstProcedimientos.Find(x => x.procedimientos[0].Nombre.Contains("EmpleadoPerfilDTO"))*/
            ;

            CapaDatos capaDatos = new CapaDatos();
            EmpleadoPerfilDTO emp = new EmpleadoPerfilDTO();
            emp.Nomina = "10001595";

            //capaDatos.Generico(proc, ref emp);


            Type typeEmp = Type.GetType("ConsoleApp1.EmpleadoPerfilDTO");
            Type typePerfiles = Type.GetType("ConsoleApp1.PerfilesDTO");
            Type typePerfil = Type.GetType("ConsoleApp1.PerfilesDTO");

            dynamic dynEmp;
            dynamic dynPerfiles;
            dynamic dynPerfil;

            using (OracleConnection cnn = new OracleConnection(CapaDatos.connectionString))
            {
                cnn.Open();
                var p = new OracleDynamicParameters();
                p.Add("p_empleado", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                p.Add("p_nomina", "10001595", dbType: OracleDbType.Varchar2, direction: ParameterDirection.Input);
                p.Add("p_perfil", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                p.Add("p_perfiles", dbType: OracleDbType.RefCursor, direction: ParameterDirection.Output);
                p.Add("p_estatus", null, dbType: OracleDbType.Int32, direction: ParameterDirection.Output, 20);
                p.Add("p_mensaje", null, dbType: OracleDbType.Varchar2, direction: ParameterDirection.Output, 20);

                using (var multi = cnn.QueryMultiple("ENLACOMS.PKG_EJEMPLO_DAPPER.EMPLEADO_PERFILES",
                        param: p, commandType: CommandType.StoredProcedure))
                {
                    //emp = multi.ReadSingle<Empleado>();
                    //emp.perfiles = multi.Read<Perfiles>().AsList();
                    dynEmp = multi.ReadSingle(typeEmp);
                    dynPerfiles = multi.Read(typePerfiles).AsList();
                    dynPerfil = multi.ReadFirst(typePerfil);

                    emp = dynEmp;
                    emp.Perfil = dynPerfil;

                    emp.Perfiles = dynPerfiles;

                }

            }

        }

        public static IRestResponse SendSimpleMessage()
        {
            RestClient client = new RestClient();
            client.BaseUrl = new Uri("https://api.mailgun.net/v3");
            client.Authenticator =
                new HttpBasicAuthenticator("api",
                                            "key-d7feed7d320252bf49c2b0795fd895e5");
            RestRequest request = new RestRequest();
            request.AddParameter("domain", "sandboxf6f256184c6d4da5b9bc06788cadfa7a.mailgun.org", ParameterType.UrlSegment);
            request.Resource = "{domain}/messages";
            request.AddParameter("from", "Excited User <postmaster@sandboxf6f256184c6d4da5b9bc06788cadfa7a.mailgun.org>");
            request.AddParameter("to", "sygno.jmartinez@proveedores21b.com");
            request.AddParameter("to", "dhernandezma@xxi-banorte.com");
            request.AddParameter("subject", "Hello");
            request.AddParameter("text", "Testing some Mailgun awesomness!");
            request.Method = Method.POST;
            return client.Execute(request);
        }

        //public static IRestResponse EventsDateTimeRecipient()
        //{
        //    RestClient client = new RestClient();
        //    client.BaseUrl = new Uri("https://api.mailgun.net/v3");
        //    client.Authenticator =
        //        new HttpBasicAuthenticator("api",
        //                                    "key-d7feed7d320252bf49c2b0795fd895e5");
        //    RestRequest request = new RestRequest();
        //    request.AddParameter("domain", "sandboxf6f256184c6d4da5b9bc06788cadfa7a.mailgun.org", ParameterType.UrlSegment);
        //    request.Resource = "{domain}/events";
        //    request.AddParameter("begin", "Fri, 3 May 2013 09:00:00 -0000");
        //    request.AddParameter("ascending", "yes");
        //    request.AddParameter("limit", 25);
        //    request.AddParameter("pretty", "yes");
        //    request.AddParameter("recipient", "sygno.jmartinez@proveedores21b.com");
        //    return client.Execute(request);
        //}

        public static Type ListOfWhat(Object list)
        {
            return ListOfWhat2((dynamic)list);
        }

        private static Type ListOfWhat2<T>(IList<T> list)
        {
            return typeof(T);
        }

        private static RootSP ProcedimientosJSON()
        {
            //string archivoSPJson = @"D:\ProcedimientosBD1.json";
            //string archivoSPJson = @"C:\Users\sygno.jmartinez\Downloads\ConsoleApp1 (1)\ConsoleApp1\ConsoleApp1\ProcedimientosBD.json";
            RootSP proc = null;

            CargarJSON();

            return proc;
        }

        private static List<Procedimientos> CargarJSON()
        {

            JArray myArray = new JArray();
            string archivoSPJson = @"C:\Users\sygno.jmartinez\Downloads\ConsoleApp1 (1)\ConsoleApp1\ConsoleApp1\ProcedimientosBD.json";
            string folderPath = @"C:\Users\sygno.jmartinez\Downloads\ConsoleApp1 (1)\ConsoleApp1\ConsoleApp1\jsonSP";
            bool valid = false;
            RootSP proc = null;


            foreach (string file in Directory.EnumerateFiles(folderPath, "*.json"))
            {
                string contents = File.ReadAllText(file);

                using (StreamReader jsonStream = File.OpenText(file))
                using (JsonTextReader reader = new JsonTextReader(jsonStream))
                {
                    JSchema schema = JSchema.Load(reader);

                    JObject sp = JObject.Parse(contents);

                    valid = sp.IsValid(schema);

                    if (valid)
                    {
                        myArray.Add(sp);

                    }
                }
            }

            var lista = myArray.ToObject<List<Procedimientos>>();

            return lista;

        }

        public class CapaDatos
        {
            public const string connectionString = "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)" +
                                                                "(HOST=15.128.25.150)(PORT=1521))" +
                                                                "(CONNECT_DATA=(SERVICE_NAME=solidaD)));" +
                                                                "User ID=fo;Password=afo2015";

            public void Generico(Procedimientos proc, ref EmpleadoPerfilDTO emp)
            {
                using (OracleConnection cnn = new OracleConnection(connectionString))
                {
                    cnn.Open();
                    var p = new OracleDynamicParameters();
                    //Asigna parametro de entrada
                    foreach (Parametros parametro in proc.Parametros)
                    {
                        if (parametro.Direccion == ParameterDirection.InputOutput ||
                            parametro.Direccion == ParameterDirection.Input)
                        {
                            if (parametro.Valor == null)
                            {
                                AtributoEntidad atrEnt = parametro.AtributosEntd;
                                object valorAtributo = emp.GetType().GetProperty(atrEnt.Atributo).GetValue(emp, null);

                                p.Add(parametro.Nombre, valorAtributo, parametro.Tipo, parametro.Direccion, parametro.Tamanio);
                            }
                            else
                            {
                                p.Add(parametro.Nombre, parametro.Valor, parametro.Tipo, parametro.Direccion, parametro.Tamanio);
                            }
                        }
                        else
                        {
                            p.Add(parametro.Nombre, parametro.Valor, parametro.Tipo, parametro.Direccion, parametro.Tamanio);
                        }
                    }

                    using (var multi = cnn.QueryMultiple(proc.Procedimiento,
                            param: p, commandType: CommandType.StoredProcedure))
                    {
                        //Asigna valores de salida a la entidad
                        foreach (Parametros parametro in proc.Parametros)
                        {
                            if (parametro.Direccion == ParameterDirection.InputOutput ||
                                parametro.Direccion == ParameterDirection.Output)
                            {
                                AtributoEntidad atrEnt = parametro.AtributosEntd;
                                if (parametro.Tipo == OracleDbType.RefCursor)
                                {
                                    if (atrEnt.Atributo != null)
                                    {
                                        //Type tipoLista = emp.GetType().GetProperty(atrEnt.Atributo).GetType();
                                        //PerfilesDTO ppp = new PerfilesDTO();
                                        //Type tipoLista2 = ppp.GetType();

                                        Type tipoLista = Type.GetType(atrEnt.AtributoTipo);

                                        if (atrEnt.Coleccion.Equals("Si"))
                                        {
                                            List<PerfilesDTO> dd = multi.Read<PerfilesDTO>().AsList();
                                            //dynamic dd = multi.Read(tipoLista).AsList();
                                            emp.GetType().GetProperty(atrEnt.Atributo).SetValue(emp, dd);
                                            //emp.Perfiles = multi.Read<PerfilesDTO>().AsList();
                                        }
                                        else
                                        {
                                            emp.GetType().GetProperty(atrEnt.Atributo).SetValue(emp, multi.ReadSingle(tipoLista));
                                        }
                                    }
                                    else
                                    {
                                        emp = multi.ReadSingle<EmpleadoPerfilDTO>();
                                    }
                                }
                                else
                                {
                                    if (parametro.Tipo == OracleDbType.Varchar2)
                                    {
                                        emp.GetType().GetProperty(atrEnt.Atributo).SetValue(emp, p.Get<OracleString>(parametro.Nombre).ToString());
                                    }
                                    if (parametro.Tipo == OracleDbType.Decimal)
                                    {
                                        emp.GetType().GetProperty(atrEnt.Atributo).SetValue(emp, p.Get<OracleDecimal>(parametro.Nombre).ToInt32());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
