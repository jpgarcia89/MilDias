using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Activities.Statements;

namespace WFS_MilDias
{

    public sealed class SendSMS : CodeActivity<bool>
    {
        // Define an activity input argument of type string
        //public InArgument<string> Text { get; set; }

        //public InArgument<Int32> Telefono { get; set; }

        //public InArgument<string> DNI { get; set; }

        //public InArgument<string> Carrier { get; set; }

        public InArgument<string> Mensaje { get; set; }

        public InArgument<string> ID_Instancia { get; set; }

        public InArgument<bool> Es_Control { get; set; }

        public InArgument<Int32> Mes { get; set; }





        //public OutArgument<bool> res { get; set; }

        // If your activity returns a value, derive from CodeActivity<v>
        // and return the value from the Execute method.
        protected override bool Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            //string text = context.GetValue(this.Text);
            //Int32 telfono = context.GetValue(this.Telefono);
            //string dni = context.GetValue(this.DNI);
            //string carrier = context.GetValue(this.Carrier);

            DateTime dt09AM = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 09, 0, 0);
            DateTime dt01PM = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 13, 0, 0);
            //var delay = new Delay();


            if ((DateTime.Now < dt09AM) || (DateTime.Now > dt01PM))
            {
                TimeSpan tiempoEspera = dt09AM.Subtract(DateTime.Now);
                

                var seq = new Sequence
                {
                    Activities = { new Delay { Duration = tiempoEspera } }
                };

                WorkflowInvoker.Invoke(seq);
            }




            try
            {
                HttpClient cliente = new HttpClient();
                cliente.BaseAddress = new Uri("http://localhost:1941");
                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                SMS data = new SMS
                {
                    //Mensaje = context.GetValue(this.Text),
                    //Telefono = context.GetValue(this.Telefono),
                    //DNI = context.GetValue(this.DNI),
                    //Carrier = context.GetValue(this.Carrier),

                    Mensaje = context.GetValue(this.Mensaje),
                    ID_Instancia = context.GetValue(this.ID_Instancia),
                    Es_Control = context.GetValue(this.Es_Control),
                    Mes = context.GetValue(this.Mes),
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var respuesta = cliente.PostAsync("/api/MilDias/Prueba", content).Result;
                var resultContent = respuesta.Content.ReadAsStringAsync().Result;

                var id = context.WorkflowInstanceId;

                Debug.WriteLine(id.ToString());


                return JsonConvert.DeserializeObject<bool>(resultContent);
            }
            catch (Exception ex)
            {
                return false;
            }

        }
    }

    public class SMS
    {
        public string Mensaje { get; set; }
        public string ID_Instancia { get; set; }
        public bool Es_Control { get; set; }
        public int Mes { get; set; }
    }
    //public class SMS
    //{
    //    public string Mensaje { get; set; }

    //    public Int32 Telefono { get; set; }

    //    public string DNI { get; set; }

    //    public string Carrier { get; set; }
    //}




}
