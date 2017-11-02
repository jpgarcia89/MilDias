using ServiceRestSMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace ServiceRestSMS.Controllers
{
    public class NotificacionSMSController : ApiController
    {
        [HttpPost]
        public HttpResponseMessage NotificacionSMS(HttpRequestMessage request)
        {
            ProgMilDiasEntities db = new ProgMilDiasEntities();
            try
            {
                string xmlString = request.Content.ReadAsStringAsync().Result;
                logNotificacion log = new logNotificacion();
                log.Fecha = DateTime.Now;
                log.Mensaje = xmlString;
                log.Procesado = false;

                db.logNotificacion.Add(log);
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logSMSError logError = new logSMSError();
                logError.Fecha = DateTime.Now;
                logError.Mensaje = e.Message;
                logError.Origen = "NotificacionSMS";
                db.logSMSError.Add(logError);
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }
    }
}
