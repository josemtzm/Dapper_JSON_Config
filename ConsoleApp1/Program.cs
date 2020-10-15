﻿using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
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
            Console.WriteLine("Hello World!");
            RootSP lstProcedimientos = ProcedimientosJSON();
            
            
            /*Procedimientos proc = lstProcedimientos.Find(x => x.procedimientos[0].Nombre.Contains("EmpleadoPerfilDTO"))*/;
            
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
            string archivoSPJson = @"C:\Users\sygno.jmartinez\Downloads\ConsoleApp1 (1)\ConsoleApp1\ConsoleApp1\ProcedimientosBD.json";
            RootSP proc = null;


            using (StreamReader jsonStream = File.OpenText(archivoSPJson))
            {
                var jsonstring = jsonStream.ReadToEnd();

                if (EsquemaValido(jsonstring))
                {
                    var jsonObject = JsonConvert.DeserializeObject(jsonstring);
                    proc = JsonConvert.DeserializeObject<RootSP>(jsonstring);
                }
            }

            return proc;
        }

        private static bool EsquemaValido(string json)
        {
            bool valid = false;
            string archivoSPJson = @"C:\Users\sygno.jmartinez\Downloads\ConsoleApp1 (1)\ConsoleApp1\ConsoleApp1\ProcedimientosBD.json";

            //archivoSPJson = archivoSPJson.Replace("\r\n", "").Replace("\"", "")

            //string archivoSPJson = json.Replace("\r\n", "").Replace("[", "{").Replace("]", "}");

            //JSchema schema = JSchema.Parse(archivoSPJson);

            //JSchema schema = JSchema.Parse(json);


            using (StreamReader file = File.OpenText(archivoSPJson))
            using (JsonTextReader reader = new JsonTextReader(file))
            {
                JSchema schema = JSchema.Load(reader);

                // validate JSON


                JObject sp = JObject.Parse(json);

                valid = sp.IsValid(schema);
            }

            return valid;
        }

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