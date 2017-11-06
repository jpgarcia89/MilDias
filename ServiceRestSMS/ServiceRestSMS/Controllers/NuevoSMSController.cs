﻿using ServiceRestSMS.Models;
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
                xmlString = xmlString.Trim();

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
                    
                    /*Obtengo la embarazada por unica vez*/
                    Embarazada embarazada = new Embarazada();
                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();

                    switch (mensaje.Split(' ').Count())
                    {
                        case 1:
                            //Es mensaje de control (SI, NO, INFO, BAJA, BEBE)
                            if (embarazada != null)
                            {
                                if (mensaje == "SI" || mensaje == "NO")
                                {
                                    LogMensajeControl logControl = new LogMensajeControl();
                                    logControl = db.LogMensajeControl
                                        .Where(c => c.ID_INSTANCIA == embarazada.Inscripcion.First().ID_INSTANCIA)
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
                                    LogMensaje logSMS = new LogMensaje();
                                    logSMS.MENSAJE = "INFO";
                                    logSMS.ID_TIPOMENSAJE = 7; //Mensaje de informacion
                                    db.LogMensaje.Add(logSMS);
                                    db.SaveChanges();


                                }
                                else if (mensaje == "BAJA")
                                {
                                    //BAJA EL SERVICIO PARA LA EMBARAZADA 
                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                    LogMensaje logSMS = new LogMensaje();
                                    logSMS.MENSAJE = "BAJA";
                                    logSMS.ID_TIPOMENSAJE = 3; // Mensaje de baja
                                    db.LogMensaje.Add(logSMS);
                                    db.SaveChanges();


                                    /*Cambio el campo ACTIVO de la embarazada porque recibi la palabra baja*/
                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                    BajaEmbarazada(inscripcion.ID_INSTANCIA, 1);
                                    /******/
                                }
                                else if (mensaje == "BEBE")
                                {
                                    //UPDATE DE EMBARAZADA CON EL INDICADOR DE QUE EL BEBE HA NACIDO
                                    LogMensaje logSMS = new LogMensaje();
                                    logSMS.MENSAJE = "BAJA";
                                    logSMS.ID_TIPOMENSAJE = 3; // Mensaje de baja
                                    db.LogMensaje.Add(logSMS);
                                    db.SaveChanges();


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
                                String Palabra = mensaje[0].ToString();

                                String tmpDNI = mensaje[1].ToString();
                                String tmpMES = mensaje[2].ToString();

                                int  DNI = 0;
                                int MES = 0;
                                bool continuar = false;

                                //Valido que el DNI sea numerico
                                if (Int32.TryParse(tmpDNI, out DNI))
                                {
                                    continuar = true;
                                }
                                else
                                {
                                    EnviarMensajeMalformado();
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
                                    EnviarMensajeMalformado();
                                }

                                if (continuar)
                                {
                                    /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                    LogMensaje logSMS = new LogMensaje();
                                    logSMS.MENSAJE = (Palabra == "MAMA" ) ? "ALTA MAMA" : "ALTA BEBE";
                                    logSMS.ID_TIPOMENSAJE = 2; //Mensaje de Alta
                                    db.LogMensaje.Add(logSMS);
                                    db.SaveChanges();

                                    embarazada = db.Embarazada.Where(e => e.TELEFONO == MO.Telefono.Msisdn).FirstOrDefault();
                                    if (embarazada == null)
                                    {
                                        /*Creo el registro de la embarazada*/

                                    }

                                    Inscripcion inscripcion = new Inscripcion();
                                    inscripcion = db.Inscripcion.Where(e => e.ID_EMBARAZADA == embarazada.ID).FirstOrDefault();
                                    if (inscripcion == null)
                                    {
                                        //Crear instancia de inscripcion 
                                        inscripcion.ID_EMBARAZADA = embarazada.ID;
                                        inscripcion.ID_TIPOINSTANCIA = (Palabra == "MAMA") ? 1 : 2; // 
                                        inscripcion.MES = 0;
                                        db.SaveChanges();
                                    }

                                    /*Doy inicio al WF segun palabra*/
                                    /********************************/
                                }
                            }

                            break;
                        default:
                            {
                                //Respondemos que no tiene el formato correcto


                                /*Guardo el mensaje de info saliente en el log de mensajes como tipo de mensaje enviado*/
                                LogMensaje logSMS = new LogMensaje();
                                logSMS.MENSAJE = "ERROR";
                                logSMS.ID_TIPOMENSAJE = 8; //Mensaje mal formado
                                db.LogMensaje.Add(logSMS);
                                db.SaveChanges();
                            }
                            break;
                    }

                    //}
                    //else
                    //{
                    //    //Envio Mensaje informando que el tel no esta registrado.

                    //}

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

        public void EnviarMensajeMalformado()
        {

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

                            //Crear instancia de inscripcion 
                            inscripcion.ID_EMBARAZADA = embarazada.ID;
                            inscripcion.ID_TIPOINSTANCIA = 2; // WF Nacido
                            inscripcion.MES = 0;
                            db.SaveChanges();
                            /**/

                            /*Se debera crear una nueva instancia de bebe nacido 
                            y hacer todo el WF deshabilitando la actual*/

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
    }
}
