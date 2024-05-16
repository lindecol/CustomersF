namespace WCF_Customers
{
    /// <summary>
    /// Almacena el mensaje de error
    /// </summary>
    public class clsEstado 
    {
        /// <summary>
        /// Identificador Código de error
        /// </summary>
        public string CodError { get; set; }

        /// <summary>
        /// Descripción del mensaje
        /// </summary>
        public string Mensaje { get; set; }
    }
}