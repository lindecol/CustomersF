//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Softmanagement S.A">
//     Copyright (c) Sprocket Enterprises. All rights reserved.
// </copyright>
// <author>Ecamargo</author>
//-----------------------------------------------------------------------

namespace AppDisparadorCustomers
{
    using System;

    /// <summary>
    /// Contiene el proceso que envía customers por estado al WebServices de fusion
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Permite lanzar los procesos por estados de customers a fusión, el estado por defecto es 'C', 
        /// envía todos los customers creados, cuando no se envían parámetros.
        /// </summary>
        /// <param name="args">Estado tabla stagging</param>
        private static void Main(string[] args)
        {
            string parametro = string.Empty;
                       
            try
            {
                if (args.Length != 0)
                {
                    parametro = args[0];
                }
                else
                {
                    ////Por defecto al diaparar el proceso se enviaran los datos con estado C, 'Creados'.
                    parametro = "C";
                }

                ServicioCustomers.WS_CustomersClient serCustomers = new ServicioCustomers.WS_CustomersClient();
                serCustomers.Get_Customers(parametro);
            }
            catch(Exception ex)
            {
                ////Error de conexion al Web Services
                System.IO.StreamWriter arc = new System.IO.StreamWriter(@"LogDisparador.txt", true);
                arc.WriteLine("Error de Conexion " + ex.Message);
                parametro = string.Empty;
            }
        }
    }
}
