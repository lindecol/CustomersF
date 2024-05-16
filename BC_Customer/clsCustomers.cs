namespace BC_Customer
{
    using System;
    using System.Xml;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data;
    using Microsoft.Practices.EnterpriseLibrary.Common;
    using Microsoft.Practices.EnterpriseLibrary.Data;

    /// <summary>
    /// Contiene toda la logica de clsCustomers
    /// </summary>
    public class clsCustomers
    {
        /// <summary>
        /// Initializes a new instance of the clsCustomers class.
        /// </summary>
        public clsCustomers()
        {
        }

        /// <summary>
        /// Initializes a new instance of the clsCustomers class.
        /// </summary>
        /// <param name="spxCustomersId">Retorna spxCustomersId</param>
        /// <param name="legacyCompany">Retorna legacyCompany</param>
        /// <param name="legacyCustomer">Retorna legacyCustomer</param>
        /// <param name="addressNumber">Retorna addressNumber</param>
        /// <param name="interfaceStatus">Retorna interfaceStatus</param>
        public clsCustomers(int spxCustomersId, string legacyCompany, string legacyCustomer, string addressNumber, string interfaceStatus)
        {
            this.S_PX_CUSTOMERS_ID = spxCustomersId;

            if (legacyCompany.Length != 0)
            {
                this.LEGACY_COMPANY = int.Parse(legacyCompany);
            }

            this.LEGACY_CUSTOMER_ID = legacyCustomer;

            if (addressNumber.Length != 0)
            {
                this.ADDRESS_NUMBER = int.Parse(addressNumber);
            }

            this.INTERFACE_STATUS = interfaceStatus;
        }

        // Identificador unico de la entidad
        public int S_PX_CUSTOMERS_ID { get; set; }

        // Identificador de la compañia
        public int? LEGACY_COMPANY { get; set; }

        // Identificador customer
        public string LEGACY_CUSTOMER_ID { get; set; } 

        // Identificador addres 
        public int? ADDRESS_NUMBER { get; set; }

        // Identificador de status
        public string INTERFACE_STATUS { get; set; }

        // Identificador AN8
        public string AN8 { get; set; }
        
       /// <summary>
        ///  Genera un listado con todos los registros por estado 
       /// </summary>
        /// <returns>Retorna Lista</returns>
        public List<clsCustomers> Get_Customers()
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                DbCommand selectcomand = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_CUSTOMERS");
                db.AddInParameter(selectcomand, "P_INTERFACE_STATUS", DbType.String, this.INTERFACE_STATUS);

                List<clsCustomers> listaDatos = new List<clsCustomers>();

                using (IDataReader dato = db.ExecuteReader(selectcomand))
                {
                    while (dato.Read())
                    {
                        listaDatos.Add(new clsCustomers(int.Parse(dato.GetValue(0).ToString()), dato.GetValue(1).ToString(), dato.GetValue(2).ToString(), dato.GetValue(3).ToString(), dato.GetValue(4).ToString()));
                    }
                }

                return listaDatos;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Permite almacenar el estado de la la confirmación de Fussion 
        /// </summary>
        public void Confirmation()
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                DbCommand comand = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_CONFIRMATION");
                db.AddInParameter(comand, "P_INTERFACE_STATUS", DbType.String, this.INTERFACE_STATUS);
                db.AddInParameter(comand, "P_LEGACY_COMPANY", DbType.Int32, this.LEGACY_COMPANY);
                db.AddInParameter(comand, "P_LEGACY_CUSTOMER_ID", DbType.String, this.LEGACY_CUSTOMER_ID);
                db.AddInParameter(comand, "P_AN8", DbType.String, this.AN8);
                db.ExecuteNonQuery(comand);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// Cambia el estado de la tabla de staging
        /// </summary>
        /// <returns></returns>
        public void CambioEstado()
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase();
                DbCommand comand = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_STATUS_CUSTOMERS");
                db.AddInParameter(comand, "P_INTERFACE_STATUS", DbType.String, this.INTERFACE_STATUS);
                db.AddInParameter(comand, "P_LEGACY_COMPANY", DbType.Int32, this.LEGACY_COMPANY);
                db.AddInParameter(comand, "P_S_PX_CUSTOMERS_ID", DbType.Int16, this.S_PX_CUSTOMERS_ID);
                db.ExecuteNonQuery(comand);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

       /// <summary>
        /// Permite generar el documento xml con toda la estructura de datos de customers.
       /// </summary>
       /// <returns>Retorna Dataset</returns>
        public DataSet Get_ListaCustomers()
        {
            Database db = DatabaseFactory.CreateDatabase();

            using (DbConnection connection = db.CreateConnection())
            {
                connection.Open();
                DbTransaction transaction = connection.BeginTransaction();
             
                try
                {
                    DataSet datDatos = new DataSet("CUSTOMERS");

                    DbCommand comandAddresBook = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_ADDRESBOOK");
                    db.AddInParameter(comandAddresBook, "P_CLIENT_CLI", DbType.String, this.LEGACY_CUSTOMER_ID);
                    db.AddInParameter(comandAddresBook, "P_LEGACY_COMPANY", DbType.Int32, this.LEGACY_COMPANY);

                    datDatos.Tables.Add("ADDRESBOOK");
                    datDatos.Tables[0].Merge(db.ExecuteDataSet(comandAddresBook, transaction).Tables[0]);
                                        
                    DbCommand comandAdicionalInfo = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_COLOMBIAADICIONALINFO");
                    db.AddInParameter(comandAdicionalInfo, "P_CLIENT_CLI", DbType.String, this.LEGACY_CUSTOMER_ID);
                    db.AddInParameter(comandAdicionalInfo, "P_LEGACY_COMPANY", DbType.Int32, this.LEGACY_COMPANY);

                    datDatos.Tables.Add("COLOMBIA ADICIONAL INFORMACION");
                    datDatos.Tables[1].Merge(db.ExecuteDataSet(comandAdicionalInfo, transaction).Tables[0]);

                    DbCommand comandCustomerMaster = db.GetStoredProcCommand("SPPQ_PRAX_CUSTOMERS.SPSP_CUSTOMERMASTER");
                    db.AddInParameter(comandCustomerMaster, "P_CLIENT_CLI", DbType.String, this.LEGACY_CUSTOMER_ID);
                    db.AddInParameter(comandCustomerMaster, "P_LEGACY_COMPANY", DbType.Int32, this.LEGACY_COMPANY);

                    datDatos.Tables.Add("CUSTOMERMASTER");
                    datDatos.Tables[2].Merge(db.ExecuteDataSet(comandCustomerMaster, transaction).Tables[0]);
          
                    transaction.Commit();

                    return datDatos;
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    throw e;
                }
            }
        }
    }
}
