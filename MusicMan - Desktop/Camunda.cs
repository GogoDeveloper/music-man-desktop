using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace MusicMan___Desktop
{
    class Camunda
    {
        RestClient client = new RestClient("http://localhost:8080/engine-rest/process-definition/");

        public Camunda()
        {
            RestRequest request = new RestRequest(Method.GET);

            var response = client.Execute(request);
        }

    }
}
