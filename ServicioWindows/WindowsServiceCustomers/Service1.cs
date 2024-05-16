namespace WindowsServiceCustomers
{
    using System.Configuration;
    using System.IO;
    using System;
    using System.Timers;
    using System.ServiceProcess;

    /// <content>
    /// Contains auto-generated functionality for the Service1 class.
    /// </content>
    public partial class Service1 : ServiceBase
    {
        /// <summary>
        /// Initializes a new instance of the Service1 class.
        /// </summary>
        public Service1()
        {
            this.InitializeComponent();
            int intervalo = int.Parse(ConfigurationManager.AppSettings["Intervalo"].ToString());
            this.tperiodo = new Timer(intervalo);
            this.tperiodo.Elapsed += new ElapsedEventHandler(this.Proceso);
        }

        /// <summary>
        /// Crea un objeto de tipo Timer global a la aplicación.
        /// </summary>
        private Timer tperiodo = null;
        private bool _iniciarProceso;

        /// <summary>
        /// Evento OnStar
        /// </summary>
        /// <param name="args">tipo de dato string</param>
        protected override void OnStart(string[] args)
        {
            _iniciarProceso = true;
            this.tperiodo.Start();
        }

        /// <summary>
        /// Detiene el Timer 
        /// </summary>
        protected override void OnStop()
        {
            _iniciarProceso = false;
            //this.tperiodo.Stop();
        }

        /// <summary>
        /// Función de almacenamiento del log
        /// </summary>
        /// <param name="estado">tipo de dato string</param>
        private void Log(string estado)
        {
            try
            {
                string path = ConfigurationManager.AppSettings["RutaLog"].ToString();
                TextWriter tw = new StreamWriter(path, true);
                tw.WriteLine(DateTime.Now.ToString() + ", Intervalo: " + this.tperiodo.Interval.ToString() + " " + estado + "\n");
                tw.Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.EventLog.WriteEntry("Application", "Exception: " + ex.Message);
            }
        }

        /// <summary>
        /// Envia todos los clientes por estado, registra en log solo mensajes de error
        /// </summary>
        /// <param name="sender">tipo de dato object</param>
        /// <param name="e">tipo de dato ElapsedEventArgs</param>
        private void Proceso(object sender, ElapsedEventArgs e)
        {
            if (_iniciarProceso)
            {
                _iniciarProceso = false;
                try
                {
                    ServicioCustomers.WS_CustomersClient servCustomer = new ServicioCustomers.WS_CustomersClient();
                    servCustomer.Get_Customers(ConfigurationManager.AppSettings["EstadoCliente"].ToString());
                }
                catch (Exception ex)
                {
                    this.Log("Error " + ex.Message);
                }
                _iniciarProceso = true;
            }
        }
    }
}
