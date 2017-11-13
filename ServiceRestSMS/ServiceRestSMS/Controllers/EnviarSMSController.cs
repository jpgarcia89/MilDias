using ServiceRestSMS.Models;
using ServiceRestSMS.Models.MovilGates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Serialization;

namespace ServiceRestSMS.Controllers
{
    public class EnviarSMSController : ApiController
    {
        private string quitarAcentos(string ArgMensaje)
        {
            string rslt = "";
            string consignos = "áàäéèëíìïóòöúùuñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ�";
            string sinsignos = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcCa";
            for (int v = 0; v < sinsignos.Length; v++)
            {
                string i = consignos.Substring(v, 1);
                string j = sinsignos.Substring(v, 1);
                rslt = ArgMensaje.Replace(i, j);
            }
            return rslt;
        }

        [HttpPost]
        public IHttpActionResult EnviarSMS(SMS ArgSMS)
        {
            MilDiasEntities db = new MilDiasEntities();
            var embarazada = from e in db.Embarazada
                             join i in db.Inscripcion on e.ID equals i.ID_EMBARAZADA
                             where i.ACTIVO == true && i.ID_INSTANCIA == ArgSMS.ID_Instancia
                             select new
                             {
                                 telefono = e.TELEFONO,
                                 carrier = e.Empresa.Carrier
                             };

            

            if (embarazada.ToList().Count > 0)
            {
                GuardaLog("MSJ: " + ArgSMS.Mensaje + " -- Mes: " + ArgSMS.Mes + " -- Tel: " + embarazada.First().telefono + " -- Tel: " + embarazada.First().carrier, 1, ArgSMS.ID_Instancia);
                return Json(EnviarSMS(ArgSMS.Mes + " -- " + ArgSMS.Mensaje, embarazada.First().carrier, embarazada.First().telefono, ArgSMS.Es_Control, ArgSMS.ID_Instancia,ArgSMS.Mes));
            }
            else
            {
                GuardaLog("ERROR EnviarSMS -- mensaje: " + ArgSMS.Mensaje + "countEmb: " + embarazada.ToList().Count, 6, ArgSMS.ID_Instancia);
                return Json(false);
            }
        }

        internal bool EnviarSMS(string ArgMensaje, string ArgCarrier, string ArgTelefono, bool ArgEsControl, string ArgInstancia,int ArgMes)
        {
            MilDiasEntities db = new MilDiasEntities();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://64.76.120.14:6064/minsaludsanjuan");
            
            try
            {
                string responseText = "";
                request.ContentType = "text/xml";
                request.Method = "POST";

                MTRequest MT = new MTRequest();
                MT.Proveedor.Id = "minsaludsanjuan";
                MT.Proveedor.Password = "mgs4ludsju4n";
                MT.Servicio.Id = ArgCarrier;
                MT.Servicio.ContentType = "0";
                MT.Telefono.Msisdn = ArgTelefono;
                MT.Contenido.Text = quitarAcentos(ArgMensaje);

                var body = new StringWriter();                
                var serializerXML = new XmlSerializer(typeof(MTRequest));
                serializerXML.Serialize(body, MT);

                byte[] postBytes = Encoding.UTF8.GetBytes(body.ToString());
                //byte[] postBytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(body.ToString());
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var encoding = ASCIIEncoding.ASCII;
                    //Encoding encoding = Encoding.GetEncoding("ISO-8859-1");

                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseText = reader.ReadToEnd();
                    }

                    LogMensaje log = new LogMensaje();
                    log.FECHA = DateTime.Now;
                    log.MENSAJE = responseText;
                    log.ID_INSTANCIA = ArgInstancia;
                    if (ArgEsControl == true)
                    {
                        log.ID_TIPOMENSAJE = 4;

                        var deserializerXML = new XmlSerializer(typeof(MTResponse));
                        MTResponse MTResp = new MTResponse();
                        using (TextReader readerXML = new StringReader(responseText))
                        {
                            MTResp = (MTResponse)deserializerXML.Deserialize(readerXML);
                        }
                        LogMensajeControl logControl = new LogMensajeControl();
                        logControl.FECHA = DateTime.Now;
                        logControl.ID_INSTANCIA = ArgInstancia;
                        logControl.ID_TRANSACCION = int.Parse(MTResp.Transaccion.IdTran);
                        logControl.ID_RESPUESTA = 3; //Por defecto ponemos que no contesto
                        logControl.MES = ArgMes;
                        db.LogMensajeControl.Add(logControl);
                        db.SaveChanges();
                    }
                    else
                    {
                        log.ID_TIPOMENSAJE = 1;
                    }
                    db.LogMensaje.Add(log);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                request.Abort();
                LogMensaje log = new LogMensaje();
                log.FECHA = DateTime.Now;
                log.MENSAJE = e.Message;
                log.ID_TIPOMENSAJE = 6;
                db.SaveChanges();
                return false;
            }           
        }

        private int GuardaLog(String Mensaje, byte IDTipoMensaje, string IDInstancia)
        {
            MilDiasEntities db = new MilDiasEntities();

            try
            {
                LogMensaje logSMS = new LogMensaje();
                logSMS.MENSAJE = Mensaje;
                logSMS.ID_TIPOMENSAJE = IDTipoMensaje;
                logSMS.FECHA = DateTime.Now;
                logSMS.ID_INSTANCIA = IDInstancia;
                db.LogMensaje.Add(logSMS);
                db.SaveChanges();

                return 0;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

    }
}
