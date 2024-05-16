namespace WCF_Customers
{
    using System;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.Text;
    using BC_Customer;
    using System.Data;
    using System.Xml.Serialization;
    using System.IO;
    using System.Xml;
    using System.Threading;
    using System.Collections;
    using System.Data.OracleClient;
    using System.Collections.Generic;

    // NOTA: puede usar el comando "Rename" del menú "Refactorizar" para cambiar el nombre de clase "WS_Customers" en el código, en svc y en el archivo de configuración a la vez.

    /// <summary>
    /// Servicio Web de Customers
    /// </summary>
    [ServiceBehavior(Namespace = "http://www.praxair.com.co/customer")]
    public class WS_Customers : IWS_Customers
    {
        /// <summary>
        /// Identifcador de registro unico en la tabla S_PX_CUSTOMERS.
        /// </summary>
        private int CustomerId { get; set; }

        /// <summary>
        /// Permite tomar el estado de la ejecución del proceso en fussion
        /// </summary>
        /// <param name="companyID">tipo de dato companyID</param>
        /// <param name="customerID">tipo de dato customerID</param>
        /// <param name="interfaceStatus">tipo de dato interfaceStatus</param>
        /// <param name="an8">tipo de dato an8</param>
        /// <returns>Retorna ArrayList</returns>
        public clsResponse[] Confirmation(string companyID, string customerID, string interfaceStatus, string an8)
        {
            try
            {
                clsComunes Validar = new clsComunes();
                
                if (companyID.Length != 2)
                {
                    throw new ArgumentException("El valor del COMPANYID debe poseer una longitud de 2 caracteres");
                }

                if (!Validar.IsNumeric(companyID))
                {
                    throw new ArgumentException("El valor del COMPANYID debe representar un dato numérico");
                }
                
                if (customerID.Length != 7)
                {
                    throw new ArgumentException("El valor del CUSTOMERID debe poseer una longitud de 7 caracteres");
                }

                if (!Validar.IsNumeric(customerID))
                {
                    throw new ArgumentException("El valor del CUSTOMERID debe representar un dato numérico");
                }

                if (interfaceStatus.Length != 1)
                {
                    throw new ArgumentException("El valor del INTERFACESTATUS debe poseer una longitud de 1 caracteres");
                }
                                           
                if (interfaceStatus.Trim() == "S" || interfaceStatus.Trim() == "s")
                {
                    if (an8.Length > 8)
                    {
                        throw new ArgumentException("El valor del AN8 debe poseer una longitud de 8 caracteres");
                    }

                    if (!Validar.IsNumeric(an8))
                    {
                        throw new ArgumentException("El valor del AN8 debe representar un dato numérico");
                    }
                }

                clsCustomers cust = new clsCustomers();
                cust.INTERFACE_STATUS = interfaceStatus;
                cust.LEGACY_COMPANY = int.Parse(companyID);
                cust.LEGACY_CUSTOMER_ID = customerID;
                cust.AN8 = an8;

                cust.Confirmation();

                using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                {
                    logError.InsertError(0, 23, ServicioLog.ProcessId.Customers, "Ok Confirmación de Fusion Estado: LEGACY_COMPANY: " + companyID + " LEGACY_CUSTOMER_ID: " + customerID + " INTERFACE_STATUS: " + interfaceStatus + " AN8: " + an8);
                }

                List<clsResponse> Resp = new List<clsResponse>();
                Resp.Add(new clsResponse { TypeDesc = "OK", MessageDesc = "Almacenado Con Exito" });
                return Resp.ToArray();
            }

            catch (OracleException ex1)//TYPE ERROR = CONNECTION THEN THIS 
            {
                using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                {
                    logError.InsertError(0, 23, ServicioLog.ProcessId.Customers, "ERROR BASE DE DATOS " + ex1.Message);
                }

                throw new FaultException<clsResponse>(new clsResponse { MessageDesc = ex1.Message, TypeDesc = "REPLICATION" }, "");
            }

            catch (Exception e)
            {
                 using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                {
                    logError.InsertError(0, 23, ServicioLog.ProcessId.Customers,e.Message);
                }

                 throw new FaultException<clsResponse>(new clsResponse { MessageDesc = e.Message, TypeDesc = "EXCEPTION" }, "");
            }

        }

        /// <summary>
        /// Valida si el valor ingresado es de tipo Date
        /// </summary>
        /// <param name="sdate">tipo de dato string</param>
        /// <returns>Retorna bool</returns>
        public bool IsDate(string sdate)
        {
            DateTime dt;
            bool isDate = true;
            try
            {
                dt = DateTime.Parse(sdate);
            }
            catch
            {
                isDate = false;
            }

            return isDate;
        }

        /// <summary>
        /// Valida si el valor ingresado es de tipo Decimal
        /// </summary>
        /// <param name="sdate">tipo de dato string</param>
        /// <returns>Retorna bool</returns>
        public bool IsDecimal(string sdate)
        {
            decimal dt;
            bool isDate = true;
            try
            {
                dt = decimal.Parse(sdate);
            }
            catch
            {
                isDate = false;
            }

            return isDate;
        }

        /// <summary>
        /// Valida si el valor ingresado es de tipo Int
        /// </summary>
        /// <param name="sdate">tipo de dato string</param>
        /// <returns></returns>
        public bool IsInt(string sdate)
        {
            int dt;
            bool isDate = true;
            try
            {
                dt = int.Parse(sdate);
            }
            catch
            {
                isDate = false;
            }

            return isDate;
        }

        /// <summary>
        /// Envía los datos de customers de la tabla de Staging al Servicio Web de fusión.
        /// </summary>
        /// <param name="interfaceStatus">tipo de dato string</param>
        /// <returns>Retorna ArrayList</returns>
        public ArrayList Get_Customers(string interfaceStatus)
        {
            ArrayList Estado = new ArrayList();

            clsCustomers cust = new clsCustomers();
            cust.INTERFACE_STATUS = interfaceStatus;
            ArrayList Get_CustomersSalida = new ArrayList();

            foreach (var item in cust.Get_Customers())
            {
                try
                {
                    ServicioCustomerPraxReqABCSImp.SyncCustomerPraxReqABCSImplClient CustomerPraxReqABCSImp = new ServicioCustomerPraxReqABCSImp.SyncCustomerPraxReqABCSImplClient();
                    ServicioCustomerPraxReqABCSImp.CUSTOMERS objCustomers = new ServicioCustomerPraxReqABCSImp.CUSTOMERS();

                    this.CustomerId = item.S_PX_CUSTOMERS_ID;

                    cust.LEGACY_CUSTOMER_ID = item.LEGACY_CUSTOMER_ID;
                    cust.LEGACY_COMPANY = item.LEGACY_COMPANY;

                    DataSet ds = new DataSet();
                    ds = cust.Get_ListaCustomers();

                    objCustomers.ADDRESSBOOK = new ServicioCustomerPraxReqABCSImp.CUSTOMERSADDRESSBOOK();
                    objCustomers.COLOMBIA_ADICIONAL_INFORMACION = new ServicioCustomerPraxReqABCSImp.CUSTOMERSCOLOMBIA_ADICIONAL_INFORMACION();
                    objCustomers.CUSTOMERMASTER = new ServicioCustomerPraxReqABCSImp.CUSTOMERSCUSTOMERMASTER();

                    objCustomers.ADDRESSBOOK.AN8 = ds.Tables[0].Rows[0]["AN8"].ToString();
                    objCustomers.ADDRESSBOOK.CO = ds.Tables[0].Rows[0]["CO"].ToString();
                    objCustomers.ADDRESSBOOK.EXR1 = ds.Tables[0].Rows[0]["EXR1"].ToString();
                    objCustomers.ADDRESSBOOK.ADD1 = ds.Tables[0].Rows[0]["ADD1"].ToString();
                    objCustomers.ADDRESSBOOK.ADD2 = ds.Tables[0].Rows[0]["ADD2"].ToString();
                    objCustomers.ADDRESSBOOK.MCU = ds.Tables[0].Rows[0]["MCU"].ToString();
                    objCustomers.ADDRESSBOOK.TAX = ds.Tables[0].Rows[0]["TAX"].ToString();
                    objCustomers.ADDRESSBOOK.ALPH = ds.Tables[0].Rows[0]["ALPH"].ToString();
                    objCustomers.ADDRESSBOOK.AT1 = ds.Tables[0].Rows[0]["AT1"].ToString();
                    objCustomers.ADDRESSBOOK.ALKY = ds.Tables[0].Rows[0]["ALKY"].ToString();
                    objCustomers.ADDRESSBOOK.SIC = ds.Tables[0].Rows[0]["SIC"].ToString();
                    objCustomers.ADDRESSBOOK.LNGP = ds.Tables[0].Rows[0]["LNGP"].ToString();
                    objCustomers.ADDRESSBOOK.TAXC = ds.Tables[0].Rows[0]["TAXC"].ToString();
                    objCustomers.ADDRESSBOOK.AC01 = ds.Tables[0].Rows[0]["AC01"].ToString();
                    objCustomers.ADDRESSBOOK.AC04 = ds.Tables[0].Rows[0]["AC04"].ToString();
                    objCustomers.ADDRESSBOOK.AC16 = ds.Tables[0].Rows[0]["AC16"].ToString();
                    objCustomers.ADDRESSBOOK.AC11 = ds.Tables[0].Rows[0]["AC11"].ToString();
                    objCustomers.ADDRESSBOOK.AC12 = ds.Tables[0].Rows[0]["AC12"].ToString();
                    objCustomers.ADDRESSBOOK.AC13 = ds.Tables[0].Rows[0]["AC13"].ToString();
                    objCustomers.ADDRESSBOOK.MLNM = ds.Tables[0].Rows[0]["MLNM"].ToString();
                    objCustomers.ADDRESSBOOK.CTY1 = ds.Tables[0].Rows[0]["CTY1"].ToString();
                    objCustomers.ADDRESSBOOK.CTR = ds.Tables[0].Rows[0]["CTR"].ToString();
                    objCustomers.ADDRESSBOOK.ADDS = ds.Tables[0].Rows[0]["ADDS"].ToString();
                    objCustomers.ADDRESSBOOK.COUN = ds.Tables[0].Rows[0]["COUN"].ToString();
                    objCustomers.ADDRESSBOOK.PH1 = ds.Tables[0].Rows[0]["PH1"].ToString();
                    objCustomers.ADDRESSBOOK.PA8 = ds.Tables[0].Rows[0]["PA8"].ToString();
                    objCustomers.ADDRESSBOOK.AC18 = ds.Tables[0].Rows[0]["AC18"].ToString();

                    objCustomers.COLOMBIA_ADICIONAL_INFORMACION.V076CTPT = ds.Tables[1].Rows[0]["V076CTPT"].ToString();
                    objCustomers.COLOMBIA_ADICIONAL_INFORMACION.V076CCIIU = ds.Tables[1].Rows[0]["V076CCIIU"].ToString();

                    if (ds.Tables[1].Rows[0]["V0EFDJ"].ToString().Length != 0 && this.IsDate(ds.Tables[1].Rows[0]["V0EFDJ"].ToString()))
                    {
                        objCustomers.COLOMBIA_ADICIONAL_INFORMACION.V0EFDJ = DateTime.Parse(ds.Tables[1].Rows[0]["V0EFDJ"].ToString());
                    }

                    objCustomers.CUSTOMERMASTER.CRCA = ds.Tables[2].Rows[0]["CRCA"].ToString();
                    objCustomers.CUSTOMERMASTER.CRCD = ds.Tables[2].Rows[0]["CRCD"].ToString();

                    if (ds.Tables[2].Rows[0]["ACL"].ToString().Length != 0 && this.IsInt(ds.Tables[2].Rows[0]["ACL"].ToString()))
                    {
                        objCustomers.CUSTOMERMASTER.ACL = int.Parse(ds.Tables[2].Rows[0]["ACL"].ToString());
                    }

                    objCustomers.CUSTOMERMASTER.TRAR = ds.Tables[2].Rows[0]["TRAR"].ToString();
                    objCustomers.CUSTOMERMASTER.TXA1 = ds.Tables[2].Rows[0]["TXA1"].ToString();
                    objCustomers.CUSTOMERMASTER.RYIN = ds.Tables[2].Rows[0]["RYIN"].ToString();
                    objCustomers.CUSTOMERMASTER.CLMG = ds.Tables[2].Rows[0]["CLMG"].ToString();

                    if (ds.Tables[2].Rows[0]["DAOJ"].ToString().Length != 0 && this.IsDate(ds.Tables[2].Rows[0]["DAOJ"].ToString()))
                    {
                        objCustomers.CUSTOMERMASTER.DAOJ = DateTime.Parse(ds.Tables[2].Rows[0]["DAOJ"].ToString());
                    }

                    objCustomers.CUSTOMERMASTER.AC30 = ds.Tables[2].Rows[0]["AC30"].ToString();
                    objCustomers.CUSTOMERMASTER.ABC1 = ds.Tables[2].Rows[0]["ABC1"].ToString();
                    objCustomers.CUSTOMERMASTER.BADT = ds.Tables[2].Rows[0]["BADT"].ToString();
                    objCustomers.CUSTOMERMASTER.ACTN = ds.Tables[2].Rows[0]["ACTN"].ToString();

                    System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(objCustomers.GetType());
                    System.Text.StringBuilder sb = new System.Text.StringBuilder();
                    System.IO.StringWriter writer = new System.IO.StringWriter(sb);

                    System.Xml.Serialization.XmlSerializerNamespaces espacioDeNombres = new System.Xml.Serialization.XmlSerializerNamespaces();
                    espacioDeNombres.Add("cus", @"http://www.praxair.com.co/customer");

                    ser.Serialize(writer, objCustomers, espacioDeNombres);

                    string objCustomer = sb.ToString();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(objCustomer);

                    objCustomer = objCustomer.Replace("utf-16", "utf-8");

                    XmlSerializer xmlSerz = new XmlSerializer(objCustomers.GetType());
                    StringReader strReader = new StringReader(objCustomer);

                    object obj = xmlSerz.Deserialize(strReader);

                    try
                    {
                        CustomerPraxReqABCSImp.SyncCustomer((ServicioCustomerPraxReqABCSImp.CUSTOMERS)obj);
                        CustomerPraxReqABCSImp = null;

                        clsCustomers EstdoCustomer = new clsCustomers();
                        EstdoCustomer.S_PX_CUSTOMERS_ID = item.S_PX_CUSTOMERS_ID;
                        EstdoCustomer.INTERFACE_STATUS = "G";
                        EstdoCustomer.LEGACY_COMPANY = item.LEGACY_COMPANY;
                        EstdoCustomer.CambioEstado();

                        using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                        {
                            logError.InsertError(this.CustomerId, 23, ServicioLog.ProcessId.Customers, "OK LEGACY_CUSTOMER_ID: " + cust.LEGACY_CUSTOMER_ID + " LEGACY_COMPANY: " + cust.LEGACY_COMPANY);
                        }

                    }
                    catch (System.ServiceModel.EndpointNotFoundException e)
                    {
                        clsCustomers EstdoCustomer = new clsCustomers();
                        EstdoCustomer.S_PX_CUSTOMERS_ID = item.S_PX_CUSTOMERS_ID;
                        EstdoCustomer.INTERFACE_STATUS = "E";
                        EstdoCustomer.LEGACY_COMPANY = item.LEGACY_COMPANY;
                        EstdoCustomer.CambioEstado();

                        using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                        {
                            logError.InsertError(this.CustomerId, 23, ServicioLog.ProcessId.Customers, "ERROR DE CONEXIÓN " + e.Message);
                        }
                    }

                }
                catch (Exception e)
                {
                    using (ServicioLog.UtilitiesClient logError = new ServicioLog.UtilitiesClient())
                    {
                        logError.InsertError(this.CustomerId, 23, ServicioLog.ProcessId.Customers, e.Message + " LEGACY_CUSTOMER_ID: " + cust.LEGACY_CUSTOMER_ID + " LEGACY_COMPANY: " + cust.LEGACY_COMPANY);
                    }
                }
            }

            return Estado;
        }

        /// <summary>
        /// Permite exponer el objeto clsCustomers a través de WCF.
        /// </summary>
        /// <returns>clsCustomers testCustomers</returns>
        public clsCustomers testCustomers()
        {
            return new clsCustomers();
        }


    }
}
