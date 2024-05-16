namespace WCF_Customers
{
    using System;
    using System.Collections;
    using System.ServiceModel;
    using System.Text;
    using BC_Customer;

    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de interfaz "IWS_Customers" en el código y en el archivo de configuración a la vez.
        [ServiceContract(Namespace = "http://www.praxair.com.co/customer")]
        public interface IWS_Customers
    {
        /// <summary>
        /// Permite el estado de confirmación del proceso en la tabla de Staging
        /// </summary>
        /// <param name="companyID">tipo de dato companyID int</param>
        /// <param name="customerID">tipo de dato customerID string</param>
        /// <param name="interfaceStatus">tipo de dato interfaceStatus string</param>
        /// <param name="an8">tipo de dato an8 string</param>
        [FaultContract(typeof(clsResponse), Name = "InternalException", Namespace = "http://www.praxair.com.co/customer")]
        [OperationContract]
        clsResponse[] Confirmation(string companyID, string customerID, string interfaceStatus, string an8);

        /// <summary>
        /// Listado de Customers por Estado
        /// </summary>
        /// <param name="interfaceStatus">tipo de dato string</param>
        /// <returns>Retorna ArrayList</returns>
        [OperationContract]
        ArrayList Get_Customers(string interfaceStatus);

        
    }
}
