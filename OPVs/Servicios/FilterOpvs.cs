using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OPVs.models;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Animations;
using OPVs.Servicios;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;

namespace OPVs.Servicios
{
    internal class FilterOpvs
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public FilterOpvs(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task <List<Opv>> flitrarEnBaseCaida(List<Opv> opvs)
        {

            List<Opv> opvsFiltradas = new List<Opv>();

            foreach (var opv in opvs)
            {
                float percentageDrop = ((opv.precioSalida - opv.precioUltimoCierre) / opv.precioSalida) * 100;

                if (percentageDrop > 60)
                {
                    opvsFiltradas.Add(opv);
                }
            }

            return opvsFiltradas;
        }
        public async Task<List<Opv>> filtrarEnBaseCrecimiento(List<Opv> opvs)
        {

            List<Opv> opvsFiltradas = new List<Opv>();

            ApiService apiService = new ApiService(_configuration);

            string APIKEY = apiService.GetApiKey();

            foreach (var opv in opvs)
            {
                try
                {
                    var response = await _httpClient.GetFromJsonAsync<StockData>($"https://api.stockdata.com/v1/ticker/{opv.ticket}/range/1/week/2023-08-05/2024-08-05?apiKey={APIKEY}");

                    
                }
                catch (Exception ex)
                {
                    // Manejo de errores (puedes agregar logging aquí)
                    Console.WriteLine($"Error obteniendo datos para el ticker {opv.ticket}: {ex.Message}");
                    
                }
            }

            return opvsFiltradas;
        }

    }
}
