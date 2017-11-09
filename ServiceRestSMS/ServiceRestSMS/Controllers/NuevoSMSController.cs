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
                
                if (xmlString != "" && xmlString != "\n")
                {
                    GuardaLog(xmlString, 6, "");

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
                    /*Obtengo la embarazada por unica vez*/
                    Embarazada embarazada = new Embarazada();
                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();


                    mensaje = mensaje.Trim();
                    //Quita los doble-espacios y los convierte en un espacio simple
                    while (mensaje.Contains("  "))
                        mensaje = mensaje.Replace("  ", " ");

                    //Divide el Mensaje en componentes(por espacio en blanco simple)
                    string[] mensajeSplit = mensaje.Split(' ');

                    //Posibles casos de Mensajes
                    switch (mensajeSplit.Count())
                    {
                        case 1:
                            //Es mensaje de control (SI, NO, INFO, BAJA, BEBE)
                            if (embarazada != null)
                            {
                                Inscripcion inscripcion = new Inscripcion();
                                inscripcion = embarazada.Inscripcion.Where(i => i.ACTIVO == true).FirstOrDefault();
                                if (inscripcion != null)
                                {
                                    if (mensaje == "SI" || mensaje == "NO")
                                    {

                                        LogMensajeControl logControl = new LogMensajeControl();
                                        logControl = db.LogMensajeControl
                                            .Where(c => c.ID_INSTANCIA == inscripcion.ID_INSTANCIA)
                                            .OrderByDescending(x => x.ID).FirstOrDefault();

                                        if (logControl != null)
                                        {
                                            //Actualizo segun respuesta en LogMensajeControl
                                            if (mensaje == "SI")
                                            {
                                                logControl.ID_RESPUESTA = 1;
                                            }
                                            else
                                            {
                                                logControl.ID_RESPUESTA = 2;
                                            }
                                            if (inscripcion.ID_TIPOINSTANCIA == 1)
                                            {
                                                WF_SMSGestacion.RespSMSControlClient clientResp = new WF_SMSGestacion.RespSMSControlClient();
                                                clientResp.RespSMSControl(inscripcion.ID_INSTANCIA, logControl.ID_RESPUESTA);
                                            }
                                            else if (inscripcion.ID_TIPOINSTANCIA == 2)
                                            {
                                                WF_SMSNacido.RespSMSControlClient clientResp = new WF_SMSNacido.RespSMSControlClient();
                                                clientResp.RespSMSControl(inscripcion.ID_INSTANCIA, logControl.ID_RESPUESTA);
                                            }
                                            //Update DB
                                            db.SaveChanges();
                                        }
                                        else
                                        {
                                            GuardaLog("LogControl NULL. -- ID_Instancia: " + inscripcion.ID_TIPOINSTANCIA, 6, inscripcion.ID_INSTANCIA);
                                        }


                                    }
                                    else if (mensaje == "INFO")
                                    {
                                        //ENVIO MENSAJE AL TEL EMBARAZADA CON LA INFO DEL SERVICIO DE MENSAJERIA

                                        EnviarSMSController sms = new EnviarSMSController();
                                        SMS smsModel = new SMS();

                                        smsModel.Mensaje = "Mensaje modelo de info";
                                        smsModel.ID_Instancia = inscripcion.ID_INSTANCIA;
                                        smsModel.Es_Control = false;
                                        sms.EnviarSMS(smsModel);

                                        /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                        GuardaLog("MENSAJE DE INFORMACION", 7, inscripcion.ID_INSTANCIA);
                                    }
                                    else if (mensaje == "BAJA")
                                    {
                                        //BAJA EL SERVICIO PARA LA EMBARAZADA 
                                        /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/

                                        GuardaLog("MENSAJE DE BAJA", 3, inscripcion.ID_INSTANCIA);
                                        /*Cambio el campo ACTIVO de la embarazada porque recibi la palabra baja*/
                                        BajaEmbarazada(inscripcion.ID_INSTANCIA, 1);
                                        EnviarMensaje("BAJA DEL SERVICIO DE MENSAJES DE 1000 DIAS CONFIRMADA", MO.Servicio.Id, MO.Telefono.Msisdn);
                                        /******/
                                    }
                                    else if (mensaje == "BEBE")
                                    {
                                        //UPDATE DE EMBARAZADA CON EL INDICADOR DE QUE EL BEBE HA NACIDO

                                        GuardaLog("MENSAJE DE BEBE", 9, inscripcion.ID_INSTANCIA);

                                        /*Cambio el campo ACTIVO de la embarazada porque el bebe nacio*/
                                        //inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                        BajaEmbarazada(inscripcion.ID_INSTANCIA, 4);
                                        /******/
                                    }
                                }
                               
                            }
                            break;

                        case 3:
                            {
                                //Es mensaje de inscripción (MAMA DNI MES)

                                /*Tomo la primera palabras del mensaje entrante para saber si es bebe o mama*/
                                String Palabra = mensajeSplit[0].ToString().Trim();

                                String tmpDNI = mensajeSplit[1].ToString().Trim();
                                String tmpMES = mensajeSplit[2].ToString().Trim();

                                int  DNI = 0;
                                int MES = 0;
                                bool continuar = false;                               

                                //Valido que el DNI sea numerico
                                //Valido que si la palabra es mama y las otras dos palabras son numericas, esten dentro del siguiente rango.
                                if (Palabra == "MAMA" || Palabra == "BEBE")
                                {
                                    if (Int32.TryParse(tmpMES, out MES) && Int32.TryParse(tmpDNI, out DNI))
                                    {
                                        continuar = true;
                                    }                                    
                                }                                            

                                if (continuar)
                                {
                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();
                                    if (embarazada == null)
                                    {
                                        GuardaLog("NUEVA EMBARAZADA", 10, "");
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
                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID && e.ACTIVO == true).FirstOrDefault();
                                    if (inscripcion == null)
                                    {
                                        string InstanciaID = "";
                                        if (Palabra == "MAMA")
                                        {
                                            InstanciaID = WFAltaGestacion(MES);
                                            GuardaLog("MAMA -- Embarazada DNI: "+ embarazada.DNI, 2, InstanciaID);
                                        }
                                        else if (Palabra == "BEBE")
                                        {
                                            InstanciaID = WFAltaNacido(MES);
                                            GuardaLog("BEBE -- Embarazada DNI: " + embarazada.DNI, 2, InstanciaID);
                                        }

                                        //Crear instancia de inscripcion 
                                        inscripcion = new Inscripcion();
                                        inscripcion.ID_EMBARAZADA = embarazada.ID;
                                        inscripcion.ID_TIPOINSTANCIA = (Palabra == "MAMA") ? 1 : 2; // 
                                        inscripcion.MES = MES;
                                        inscripcion.ID_INSTANCIA = InstanciaID;
                                        inscripcion.FECHA_ALTA = DateTime.Now;
                                        inscripcion.FECHA_BAJA = null;
                                        inscripcion.MOTIVO_BAJA = null;
                                        inscripcion.ACTIVO = true;
                                        db.Inscripcion.Add(inscripcion);                                        
                                        db.SaveChanges();
                                        
                                        EnviarMensaje("TE FELICITAMOS. BIENVENIDA A LA INICIATIVA 1000 DIAS", MO.Servicio.Id, MO.Telefono.Msisdn);
                                    }
                                    else
                                    {
                                        //
                                        GuardaLog("la inscripción ya existe. TELEFONO" + embarazada.TELEFONO, 6, inscripcion.ID_INSTANCIA);
                                    }

                                }
                                else
                                {
                                    //continuar = false;
                                    EnviarMensaje("El mensaje no tiene el formato correcto. Recorda que para inscribirte debes enviar MAMA DNI MES. Ejemplo MAMA 30XXXXXX 3", MO.Servicio.Id, MO.Telefono.Msisdn);
                                    GuardaLog("El mensaje no tiene el formato correcto." + MO.Telefono.Msisdn, 6, "");
                                }
                            }

                            break;
                        default:
                            {
                                /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/

                                EnviarMensaje("El mensaje no tiene el formato correcto. Recorda que para inscribirte debes enviar MAMA DNI MES. Ejemplo MAMA 30XXXXXX 3", MO.Servicio.Id, MO.Telefono.Msisdn);
                                GuardaLog("El mensaje no tiene el formato correcto." + MO.Telefono.Msisdn, 6, "");
                            }
                            break;
                    }                   
                }
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                GuardaLog(e.InnerException.Message, 6, "");
                return new HttpResponseMessage(HttpStatusCode.InternalServerError);
            }
        }

        private void EnviarMensaje(String Mensaje, string Carrier,string Telefono)
        {
            EnviarSMSController sms = new EnviarSMSController();
            sms.EnviarSMS(Mensaje,Carrier,Telefono,false,"",0);
        }


        private int getEmpresaID(string ArgCarrier)
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
            catch( Exception e)
            {
                return -1;
            }
        }

        private string quitarAcentos(string ArgMensaje)
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
        [Route("BajaEmbarazada")]
        public IHttpActionResult BajaEmbarazada(string ID_INSTANCIA, int motivo)
        {
            /*
            motivo:
            1 - Recibi un mensaje con la palabra BAJA
            2 - Termina instancia de gestacion (workflow)
            3 - Termina instancia de nacido (workflow)
            4 - Recibi mensaje con la palabra BEBE
            */
            MilDiasEntities db = new MilDiasEntities();
            Embarazada embarazada = new Embarazada();
            Inscripcion inscripcion = new Inscripcion();
            try
            {
                inscripcion = db.Inscripcion.Where(e => e.ID_INSTANCIA == ID_INSTANCIA).FirstOrDefault();
                if (inscripcion != null)
                {
                    switch (motivo)
                    {
                        case 1: //Recibi un mensaje de baja
                            {
                                if (inscripcion.ID_TIPOINSTANCIA == 1)
                                {
                                    StopSMSGestacionClient clientGestacion = new StopSMSGestacionClient();
                                    clientGestacion.StopSMSGestacion(inscripcion.ID_INSTANCIA);
                                }
                                else if (inscripcion.ID_TIPOINSTANCIA == 2)
                                {
                                    StopSMSNacidoClient clientNacido = new StopSMSNacidoClient();
                                    clientNacido.StopSMSNacido(inscripcion.ID_INSTANCIA);
                                }
                                inscripcion.ACTIVO = false;
                                inscripcion.FECHA_BAJA = DateTime.Now;
                                inscripcion.MOTIVO_BAJA = "Recibi un mensaje con la palabra BAJA";
                                db.SaveChanges();
                            }
                            break;
                        case 2: //Termina la gestación (workflow)
                        case 4: //Recibi mensaje con la palabra BEBE
                            {
                                //Fin periodo de gestacion embarazada
                                if (motivo == 2)
                                {
                                    inscripcion.MOTIVO_BAJA = "Termina instancia de gestacion (workflow)";
                                }
                                else
                                {
                                    inscripcion.MOTIVO_BAJA = "Recibi mensaje con la palabra bebe";
                                    if (inscripcion.ID_TIPOINSTANCIA == 1)
                                    {
                                        StopSMSGestacionClient clientGestacion = new StopSMSGestacionClient();
                                        clientGestacion.StopSMSGestacion(inscripcion.ID_INSTANCIA);
                                    }                              
                                }
                                inscripcion.ACTIVO = false;
                                inscripcion.FECHA_BAJA = DateTime.Now;
                                db.SaveChanges();

                                /*Se debera crear una nueva instancia de bebe nacido 
                                y hacer todo el WF deshabilitando la actual*/

                                string InstanciaID = WFAltaNacido(0);

                                //Crear instancia de inscripcion 
                                inscripcion.ID_EMBARAZADA = inscripcion.ID_EMBARAZADA;
                                inscripcion.ID_TIPOINSTANCIA = 2; // WF Nacido
                                inscripcion.MES = 0;
                                inscripcion.ID_INSTANCIA = InstanciaID;
                                inscripcion.FECHA_ALTA = DateTime.Now;
                                inscripcion.FECHA_BAJA = null;
                                inscripcion.MOTIVO_BAJA = null;
                                inscripcion.ACTIVO = true;
                                db.Inscripcion.Add(inscripcion);
                                db.SaveChanges();
                                /**/
                            }
                            break;

                        case 3: //Termina instancia de nacido (workflow)
                            {
                                /*Fin definitivo del WF para la embarazada*/
                                inscripcion.ACTIVO = false;
                                inscripcion.FECHA_BAJA = DateTime.Now;
                                inscripcion.MOTIVO_BAJA = "Termina instancia de nacido (workflow)";
                                db.SaveChanges();
                            }
                            break;
                    }
                }                
                return Json(true);
            }
            catch (Exception e)
            {
                GuardaLog(e.InnerException.Message,6, "");
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
