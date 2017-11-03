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
        
        [HttpPost]
        public HttpResponseMessage NuevoSMS(HttpRequestMessage request)
        {
            MilDiasEntities db = new MilDiasEntities();
            try
            {
                LogMensaje log = new LogMensaje();
                string xmlString = request.Content.ReadAsStringAsync().Result;
                if (xmlString != "" && xmlString != "\n")
                {
                    string mensaje = "";
                    log.FECHA = DateTime.Now;
                    log.MENSAJE = xmlString;
                    
                    var serializer = new XmlSerializer(typeof(MORequest));
                    MORequest MO = new MORequest();
                    using (TextReader readerXML = new StringReader(xmlString))
                    {
                        MO = (MORequest)serializer.Deserialize(readerXML);
                    }
                    mensaje = quitarAcentos(MO.Contenido);
                    switch (mensaje.Split(' ').Count())
                    {
                        case 1:
                            //Es mensaje de control (SI, NO, INFO)
                            if(mensaje == "SI" || mensaje == "NO") {

                                LogMensajeControl logControl = new LogMensajeControl();
                            }
                            else
                            {
                                if(mensaje == "INFO")
                                {

                                }
                            }
                            break;
                        case 3:
                            //Es mensaje de inscripción (MAMA DNI MES)

                            break;
                        default:
                            //Respondemos que no tiene el formato correcto

                            break;
                    }



                    db.LogMensaje.Add(log);
                    db.SaveChanges();
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                LogMensaje log = new LogMensaje();
                log.FECHA = DateTime.Now;
                log.MENSAJE = e.Message;
                log.ID_TIPOMENSAJE = 6;
                db.SaveChanges();
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public string quitarAcentos(string ArgMensaje)
        {
            string rslt = "";
            string consignos = "áàäéèëíìïóòöúùuñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
            string sinsignos = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";
            for (int v = 0; v < sinsignos.Length; v++)
            {
                string i = consignos.Substring(v, 1);
                string j = sinsignos.Substring(v, 1);
                rslt = ArgMensaje.Replace(i, j);
            }
            return rslt.ToUpper();
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
