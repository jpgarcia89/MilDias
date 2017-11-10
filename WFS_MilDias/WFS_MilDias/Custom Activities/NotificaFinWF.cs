using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WFS_MilDias
{

    public sealed class NotificaFinWF : CodeActivity<bool>
    {
        // Define an activity input argument of type string
        public InArgument<Int32> Motivo { get; set; }
        public InArgument<string> InstanciaID { get; set; }


        //Motivo:
        //1 - Recibi un mensaje con la palabra BAJA
        //2 - Termina instancia de gestacion (workflow)
        //3 - Termina instancia de nacido (workflow)
        //4 - Recibi mensaje con la palabra BEBE


        // If your activity returns a value, derive from CodeActivity<TResult>
        // and return the value from the Execute method.
        protected override bool Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            Int32 Motivo = context.GetValue(this.Motivo);
            string InstanciaID = context.GetValue(this.InstanciaID);


            try
            {
                HttpClient cliente = new HttpClient();
                //cliente.BaseAddress = new Uri("http://localhost:46836");
                cliente.BaseAddress = new Uri("http://10.64.65.200");
                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var data = new
                {                    
                    ID_INSTANCIA = InstanciaID,
                    motivo = Motivo,
                };

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                //var respuesta = cliente.PostAsync("/ServiceRestSMS/api/EnviarSMS", content).Result;
                var respuesta = cliente.PostAsync("/ServiceRestSMS/BajaEmbarazada", content).Result;

                var resultContent = respuesta.Content.ReadAsStringAsync().Result;

                var id = context.WorkflowInstanceId;

                //Debug.WriteLine(id.ToString());


                return JsonConvert.DeserializeObject<bool>(resultContent);
            }
            catch (Exception ex)
            {
                return false;
            }



        }
    }
}
