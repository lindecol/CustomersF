namespace WindowsServiceCustomers
{
    using System.ServiceProcess;

    /// <summary>
    /// Clase principal Program
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        public static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
            { 
               new Service1() 
            };

            ServiceBase.Run(ServicesToRun);
        }
    }
}
