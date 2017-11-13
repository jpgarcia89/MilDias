using ServiceRestSMS.Models;
using ServiceRestSMS.Models.MovilGates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Serialization;

namespace ServiceRestSMS.Controllers
{
    public class NotificacionSMSController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage NotificacionSMS(HttpRequestMessage request)
        {
            MilDiasEntities db = new MilDiasEntities();
            try
            {
                string xmlString = request.Content.ReadAsStringAsync().Result;

                var deserializerXML = new XmlSerializer(typeof(MTRequestNotify));
                MTRequestNotify MTReqNot = new MTRequestNotify();
                using (TextReader readerXML = new StringReader(xmlString))
                {
                    MTReqNot = (MTRequestNotify)deserializerXML.Deserialize(readerXML);
                }

                Inscripcion inscripcion = db.Embarazada.Where(e => e.TELEFONO == MTReqNot.Telefono.Msisdn).FirstOrDefault().Inscripcion.Where(i => i.ACTIVO == true).FirstOrDefault();               

                LogMensaje log = new LogMensaje();
                log.FECHA = DateTime.Now;
                log.MENSAJE = xmlString;
                log.ID_INSTANCIA = inscripcion.ID_INSTANCIA;
                log.ID_TIPOMENSAJE = 11;
                db.LogMensaje.Add(log);
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                LogMensaje log = new LogMensaje();
                log.FECHA = DateTime.Now;
                log.MENSAJE = e.InnerException.Message;
                log.ID_TIPOMENSAJE = 6;
                db.LogMensaje.Add(log);
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
