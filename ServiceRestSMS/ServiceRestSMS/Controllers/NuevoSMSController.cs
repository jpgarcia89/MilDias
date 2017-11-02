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
    public class NuevoSMSController : ApiController
    {
        // POST api/values
        [HttpPost]
        public HttpResponseMessage NuevoSMS(HttpRequestMessage request)
        {
            ProgMilDiasEntities db = new ProgMilDiasEntities();
            try
            {
                logMensajeRecibido log = new logMensajeRecibido();
                string xmlString = request.Content.ReadAsStringAsync().Result;
                if (xmlString != "" && xmlString != "\n")
                {
                    log.Fecha = DateTime.Now;
                    log.Mensaje = xmlString;
                    log.Origen = "NuevoSMS";
                    log.Procesado = false;
                    db.logMensajeRecibido.Add(log);
                    db.SaveChanges();          
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                logSMSError logError = new logSMSError();
                logError.Fecha = DateTime.Now;
                logError.Mensaje = e.Message;
                logError.Origen = "NuevoSMS";
                db.logSMSError.Add(logError);
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public static string[] splitContenido(string contenido)
        {
            string[] retString;
            try
            {
                while (contenido.Contains("  "))
                    contenido = contenido.Replace("  ", " ");

                retString = contenido.Split(' ');
                if (retString.Length != 3)//Si viene con formato incorrecto, devuelvo el array por defecto
                {
                    retString = "0 0 0".Split(' ');
                }
                return retString;
            }
            catch (Exception)
            {
                retString = "0 0 0".Split(' ');
                return retString;
            }

        }
    }
}
