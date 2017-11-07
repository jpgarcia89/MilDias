using ServiceRestSMS.Models;
using ServiceRestSMS.Models.MovilGates;
using ServiceRestSMS.WF_SMSGestacion;
using ServiceRestSMS.WF_SMSNacido;
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
                string xmlString = request.Content.ReadAsStringAsync().Result;
                xmlString = xmlString.Trim();


                GuardaLog(xmlString,6);
                if (xmlString != "" && xmlString != "\n")
                {
                    string mensaje = "";
                    //log.FECHA = DateTime.Now;
                    //log.MENSAJE = xmlString;

                    var serializer = new XmlSerializer(typeof(MORequest));
                    MORequest MO = new MORequest();
                    using (TextReader readerXML = new StringReader(xmlString))
                    {
                        MO = (MORequest)serializer.Deserialize(readerXML);
                    }

                    mensaje = quitarAcentos(MO.Contenido);
                    GuardaLog(mensaje, 6);
                    /*Obtengo la embarazada por unica vez*/
                    Embarazada embarazada = new Embarazada();
                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();
                    string[] mensajeSplit = mensaje.Split(' ');
                    switch (mensajeSplit.Count())
                    {
                        case 1:
                            //Es mensaje de control (SI, NO, INFO, BAJA, BEBE)
                            if (embarazada != null)
                            {
                                if (mensaje == "SI" || mensaje == "NO")
                                {
                                    LogMensajeControl logControl = new LogMensajeControl();
                                    logControl = db.LogMensajeControl
                                        .Where(c => c.ID_INSTANCIA == embarazada.Inscripcion.First().ID_INSTANCIA 
                                                                   && embarazada.Inscripcion.First().ACTIVO == true )
                                        .OrderByDescending(x => x.ID).FirstOrDefault();

                                    //Actualizo segun respuesta en LogMensajeControl
                                    if (mensaje == "SI")
                                    {
                                        logControl.ID_RESPUESTA = 1;
                                    }
                                    else
                                    {
                                        logControl.ID_RESPUESTA = 2;
                                    }

                                    if (embarazada.Inscripcion.Where(e => e.ACTIVO).FirstOrDefault().ID_TIPOINSTANCIA == 1)
                                    {
                                        WF_SMSGestacion.RespSMSControlClient clientResp = new WF_SMSGestacion.RespSMSControlClient();
                                        clientResp.RespSMSControl(embarazada.Inscripcion.Where(e => e.ACTIVO).FirstOrDefault().ID_INSTANCIA);
                                    }
                                    else if (embarazada.Inscripcion.Where(e => e.ACTIVO).FirstOrDefault().ID_TIPOINSTANCIA  == 2)
                                    {
                                        WF_SMSNacido.RespSMSControlClient clientResp = new WF_SMSNacido.RespSMSControlClient();
                                        clientResp.RespSMSControl(embarazada.Inscripcion.Where(e => e.ACTIVO).FirstOrDefault().ID_INSTANCIA);
                                    }

                                    //Update DB
                                    db.SaveChanges();
                                }
                                else if (mensaje == "INFO")
                                {
                                    //ENVIO MENSAJE AL TEL EMBARAZADA CON LA INFO DEL SERVICIO DE MENSAJERIA

                                    EnviarSMSController sms = new EnviarSMSController();
                                    SMS smsModel = new SMS();
                                                                        
                                    smsModel.Mensaje = "Mensaje modelo de info";
                                    smsModel.ID_Instancia = embarazada.Inscripcion.Where(e => e.ACTIVO).FirstOrDefault().ID_INSTANCIA;
                                    smsModel.Es_Control = false;
                                    sms.EnviarSMS(smsModel);

                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                    GuardaLog("MENSAJE DE INFORMACION", 7);


                                }
                                else if (mensaje == "BAJA")
                                {
                                    //BAJA EL SERVICIO PARA LA EMBARAZADA 
                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/

                                    GuardaLog("MENSAJE DE BAJA", 3);


                                    /*Cambio el campo ACTIVO de la embarazada porque recibi la palabra baja*/
                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                    BajaEmbarazada(inscripcion.ID_INSTANCIA, 1);
                                    /******/
                                }
                                else if (mensaje == "BEBE")
                                {
                                    //UPDATE DE EMBARAZADA CON EL INDICADOR DE QUE EL BEBE HA NACIDO

                                    GuardaLog("MENSAJE DE BAJA", 3);

                                    /*Cambio el campo ACTIVO de la embarazada porque el bebe nacio*/
                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                    BajaEmbarazada(inscripcion.ID_INSTANCIA, 2);
                                    /******/
                                }
                            }
                            break;

                        case 3:
                            {
                                //Es mensaje de inscripción (MAMA DNI MES)

                                /*Tomo la primera palabras del mensaje entrante para saber si es bebe o mama*/
                                String Palabra = mensajeSplit[0].ToString();

                                String tmpDNI = mensajeSplit[1].ToString();
                                String tmpMES = mensajeSplit[2].ToString();

                                int  DNI = 0;
                                int MES = 0;
                                bool continuar = false;
                                GuardaLog("Entro al caso MAMA", 6);

                                //Valido que el DNI sea numerico
                                if (!Int32.TryParse(tmpDNI, out DNI))
                                {
                                    //continuar = false;
                                    EnviarMensaje("El mensaje no tiene el formato correcto. Recorda que para inscribirte debes enviar MAMA DNI MES. Ejemplo MAMA 30XXXXXX 3", MO.Servicio.Id, MO.Telefono.Msisdn);
                                }

                                //Valido que si la palabra es mama y las otras dos palabras son numericas, esten dentro del siguiente rango.
                                if (Int32.TryParse(tmpMES, out MES)) 
                                {
                                    if (Palabra == "MAMA" && (MES >= 0 || MES < 10))
                                    {
                                        continuar = true;
                                    }
                                    if (Palabra == "BEBE" && (MES >= 0 || MES < 25))
                                    {
                                        continuar = true;
                                    }
                                }
                                else
                                {
                                    continuar = false;
                                    EnviarMensaje("El mensaje no tiene el formato correcto. Recorda que para inscribirte debes enviar MAMA DNI MES. Ejemplo MAMA 30XXXXXX 3", MO.Servicio.Id, MO.Telefono.Msisdn);
                                }

                                if (continuar)
                                {
                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                    GuardaLog(Palabra , 2);

                                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();
                                    if (embarazada == null)
                                    {
                                        /*Creo el registro de la embarazada*/
                                        embarazada = new Embarazada();
                                        embarazada.DNI = DNI;
                                        embarazada.TELEFONO = MO.Telefono.Msisdn;
                                        embarazada.ID_EMPRESA = getEmpresaID(MO.Servicio.Id);
                                        db.Embarazada.Add(embarazada);
                                        db.SaveChanges();
                                    }

                                    /*Doy inicio al WF segun palabra*/
                                    /********************************/
                                    string InstanciaID = "";
                                    if (Palabra == "MAMA")
                                    {
                                        InstanciaID = WFAltaGestacion(MES);
                                    }
                                    else if (Palabra == "BEBE")
                                    {
                                        InstanciaID = WFAltaNacido(MES);
                                    }

                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                    if (inscripcion == null)
                                    {
                                        //Crear instancia de inscripcion 
                                        inscripcion = new Inscripcion();
                                        inscripcion.ID_EMBARAZADA = embarazada.ID;
                                        inscripcion.ID_TIPOINSTANCIA = (Palabra == "MAMA") ? 1 : 2; // 
                                        inscripcion.MES = 0;
                                        inscripcion.ID_INSTANCIA = InstanciaID;
                                        inscripcion.FECHA_ALTA = DateTime.Now;
                                        inscripcion.FECHA_BAJA = null;
                                        db.Inscripcion.Add(inscripcion);
                                        db.SaveChanges();
                                    }

                                }
                            }

                            break;
                        default:
                            {
                                /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                GuardaLog("MENSAJE MAL FORMADO", 8);
                                //Respondemos que no tiene el formato correcto
                                EnviarMensaje("El mensaje no tiene el formato correcto. Recorda que para inscribirte debes enviar MAMA DNI MES. Ejemplo MAMA 30XXXXXX 3", MO.Servicio.Id, MO.Telefono.Msisdn);
                            }
                            break;
                    }                   
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                GuardaLog(e.Message, 6);
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        public void EnviarMensaje(String Mensaje, string Carrier,string Telefono)
        {
            EnviarSMSController sms = new EnviarSMSController();
            sms.EnviarSMS(Mensaje,Carrier,Telefono,false,"",0);
        }


        int getEmpresaID(string ArgCarrier)
        {
            MilDiasEntities db = new MilDiasEntities();
            int rslt = 0;
            try
            {
                if (ArgCarrier.ToUpper().Contains("PERSONAL"))
                {
                    rslt = 3;
                }
                else if(ArgCarrier.ToUpper().Contains("MOVISTAR")) {
                    rslt = 2;
                }
                else if (ArgCarrier.ToUpper().Contains("CTI"))
                {
                    rslt = 4;
                }
                else if (ArgCarrier.ToUpper().Contains("NEXTEL"))
                {
                    rslt = 5;
                }
                return rslt;
            }
            catch (Exception e)
            {
                return -1;
            }

        }
        /*Guardo el log con el id tipo de mensaje que corresponda segun db*/
        public int GuardaLog(String Mensaje, byte IDTipoMensaje)
        {
            MilDiasEntities db = new MilDiasEntities();

            try
            {
                LogMensaje logSMS = new LogMensaje();
                logSMS.MENSAJE = Mensaje;
                logSMS.ID_TIPOMENSAJE = IDTipoMensaje;
                logSMS.FECHA = DateTime.Now;
                db.LogMensaje.Add(logSMS);
                db.SaveChanges();

                return 0;
            }
            catch( Exception e)
            {
                return -1;
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

        [HttpPost]
        public IHttpActionResult BajaEmbarazada(string ID_INSTANCIA, int motivo)
        {
            /*
            motivo:
            1 - recibi un mensaje con la palabra baja
            2 - termina gestacion
            3 - Termina instancia de nacido
            */
            MilDiasEntities db = new MilDiasEntities();
            Embarazada embarazada = new Embarazada();
            Inscripcion inscripcion = new Inscripcion();
            try
            {
                inscripcion = db.Inscripcion.Where(e => e.ID_INSTANCIA == ID_INSTANCIA).FirstOrDefault();

                switch (motivo)
                {
                    case 1: //Recibi un mensaje de baja
                        {
                            if (inscripcion.ID_TIPOINSTANCIA == 1) {
                                StopSMSGestacionClient clientGestacion = new StopSMSGestacionClient();
                                clientGestacion.StopSMSGestacion(inscripcion.ID_INSTANCIA);
                            }
                            else if (inscripcion.ID_TIPOINSTANCIA == 2)
                            {
                                StopSMSNacidoClient clientNacido = new WF_SMSNacido.StopSMSNacidoClient();
                                clientNacido.StopSMSNacido(inscripcion.ID_INSTANCIA);
                            }
                            inscripcion.ACTIVO = false;
                            inscripcion.FECHA_BAJA = DateTime.Now;
                            db.SaveChanges();                            
                        }
                        break;

                    case 2: //Termina la gestación
                        {
                            //Fin periodo de gestacion embarazada
                            inscripcion.ACTIVO = false;
                            inscripcion.FECHA_BAJA = DateTime.Now;
                            db.SaveChanges();

                            embarazada = db.Embarazada.Where(e => e.ID == inscripcion.ID_EMBARAZADA).FirstOrDefault();


                            /*Se debera crear una nueva instancia de bebe nacido 
                            y hacer todo el WF deshabilitando la actual*/

                            string InstanciaID = WFAltaNacido(0);

                            //Crear instancia de inscripcion 
                            inscripcion.ID_EMBARAZADA = embarazada.ID;
                            inscripcion.ID_TIPOINSTANCIA = 2; // WF Nacido
                            inscripcion.MES = 0;
                            inscripcion.ID_INSTANCIA = InstanciaID;
                            db.SaveChanges();
                            /**/
                        }
                        break;

                    case 3: //Termina instancia de nacido
                        {
                            /*Fin definitivo del WF para la embarazada*/
                            inscripcion = db.Inscripcion.Where(e => e.ID_INSTANCIA == ID_INSTANCIA).FirstOrDefault();
                            inscripcion.ACTIVO = false;
                            inscripcion.FECHA_BAJA = DateTime.Now;
                            db.SaveChanges();
                        }
                        break;

                }
                return Json(true);
            }
            catch (Exception e)
            {
                return Json(false);
            }


        }

        public string WFAltaGestacion(int ArgMes)
        {
            StartSMSGestacionClient client = new StartSMSGestacionClient();
            return client.StartSMSGestacion(ArgMes);
        }

        public string WFAltaNacido(int ArgMes)
        {
            StartSMSNacidoClient client = new StartSMSNacidoClient();
            return client.StartSMSNacido(ArgMes);
        }
    }
}
