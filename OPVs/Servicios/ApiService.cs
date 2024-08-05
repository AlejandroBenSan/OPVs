using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OPVs.Servicios
{
    internal class ApiService
    {
        private readonly string _apiKeyPolygon;

        public ApiService(IConfiguration configuration)
        {
            _apiKeyPolygon = configuration["ApiKeys:ApiKeyPolygon"];
        }

        public void PrintApiKey()
        {
            Console.WriteLine($"Polygon API Key: {_apiKeyPolygon}");
        }

        public string GetApiKey()
        {
            return _apiKeyPolygon;
        }
    }
}
