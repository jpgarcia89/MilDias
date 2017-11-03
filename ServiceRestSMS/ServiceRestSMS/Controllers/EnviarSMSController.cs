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
        //[HttpPost]
        //public HttpResponseMessage EnviarSMS(HttpRequestMessage Mensaje)
        //{
        //    ProgMilDiasEntities db = new ProgMilDiasEntities();
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://64.76.120.14:6064/minsaludsanjuan");
        //    try
        //    {
        //        string responseText = "";
        //        string xmlString = Mensaje.Content.ReadAsStringAsync().Result;
        //        request.ContentType = "text/xml";
        //        request.Method = "POST";
        //        byte[] postBytes = Encoding.UTF8.GetBytes(xmlString);
        //        request.ContentLength = postBytes.Length;
        //        Stream requestStream = request.GetRequestStream();
        //        requestStream.Write(postBytes, 0, postBytes.Length);
        //        requestStream.Close();
        //        using (var response = (HttpWebResponse)request.GetResponse())
        //        {
        //            var encoding = ASCIIEncoding.ASCII;
        //            using (var reader = new StreamReader(response.GetResponseStream(), encoding))
        //            {
        //                responseText = reader.ReadToEnd();
        //            }
        //            logMensajeRecibido log = new logMensajeRecibido();
        //            log.Fecha = DateTime.Now;
        //            log.Mensaje = responseText;
        //            log.Origen = "EnviarSMS";
        //            log.Procesado = false;
        //            db.logMensajeRecibido.Add(log);
        //            db.SaveChanges();                    
        //            return new HttpResponseMessage(HttpStatusCode.OK);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        request.Abort();
        //        logSMSError logError = new logSMSError();
        //        logError.Fecha = DateTime.Now;
        //        logError.Mensaje = e.Message;
        //        logError.Origen = "EnviarSMS";
        //        db.logSMSError.Add(logError);
        //        db.SaveChanges();
        //        return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        //    }
        //}

        [HttpPost]
        public IHttpActionResult EnviarSMS(SMS ParamSMS)
        {
            MilDiasEntities db = new MilDiasEntities();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://64.76.120.14:6064/minsaludsanjuan");
            try
            {
                string responseText = "";
                request.ContentType = "text/xml";
                request.Method = "POST";
                byte[] postBytes = Encoding.UTF8.GetBytes(ParamSMS.Mensaje);
                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var encoding = ASCIIEncoding.ASCII;
                    using (var reader = new StreamReader(response.GetResponseStream(), encoding))
                    {
                        responseText = reader.ReadToEnd();
                    }

                    LogMensaje log = new LogMensaje();
                    log.FECHA = DateTime.Now;
                    log.MENSAJE = responseText;
                    if (ParamSMS.Es_Control == true)
                    {
                        log.ID_TIPOMENSAJE = 4;
                        
                        var serializer = new XmlSerializer(typeof(MTResponse));
                        MTResponse MT = new MTResponse();
                        using (TextReader readerXML = new StringReader(responseText))
                        {
                            MT = (MTResponse)serializer.Deserialize(readerXML);
                        }                        
                        LogMensajeControl logControl = new LogMensajeControl();
                        logControl.FECHA = DateTime.Now;
                        logControl.ID_INSTANCIA = ParamSMS.ID_Instancia;
                        logControl.ID_TRANSACCION = int.Parse(MT.Transaccion.IdTran);
                        logControl.ID_RESPUESTA = 3; //Por defecto ponemos que no contesto
                        db.LogMensajeControl.Add(logControl);
                        db.SaveChanges();
                    }
                    else
                    {
                        log.ID_TIPOMENSAJE = 1;
                    }
                    db.LogMensaje.Add(log);
                    db.SaveChanges();
                    return Json(true);
                }
            }
            catch (Exception e)
            {
                request.Abort();
                LogMensaje log = new LogMensaje();
                log.FECHA = DateTime.Now;
                log.MENSAJE = e.Message;
                log.ID_TIPOMENSAJE = 6;                
                db.SaveChanges();
                return Json(false);
            }
        }
        
    }
}
