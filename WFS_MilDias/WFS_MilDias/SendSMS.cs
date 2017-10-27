using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Activities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace WFS_MilDias
{

    public sealed class SendSMS : CodeActivity<bool>
    {
        // Define an activity input argument of type string
        public InArgument<string> Text { get; set; }

        //public OutArgument<bool> res { get; set; }

        // If your activity returns a value, derive from CodeActivity<v>
        // and return the value from the Execute method.
        protected override bool Execute(CodeActivityContext context)
        {
            // Obtain the runtime value of the Text input argument
            string text = context.GetValue(this.Text);

            try
            {
                HttpClient cliente = new HttpClient();
                cliente.BaseAddress = new Uri("http://localhost:1941");
                cliente.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                SMS data = new SMS();
                data.mensaje = text;

                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                var respuesta = cliente.PostAsync("/api/MilDias/Prueba", content).Result;
                var resultContent = respuesta.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<bool>(resultContent);
            }
            catch (Exception)
            {
                return false;
            }
            
        }
    }

    public class SMS
    {
        public string mensaje { get; set; }
    }

}
