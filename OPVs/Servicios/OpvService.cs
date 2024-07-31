using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OPVs.models;

namespace OPVs.Servicios
{
    public class OpvService
    {
        private readonly HttpClient _httpClient;

        public OpvService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Opv>> GetOpvsAsync(DateTime startDate, DateTime endDate)
        {
            var opvs = new List<Opv>();
            try
            {
                var url = "https://es.investing.com/ipo-calendar/";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                request.Headers.Add("Accept-Language", "en-US,en;q=0.9,es;q=0.8");
                request.Headers.Add("Connection", "keep-alive");

                var response = await _httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
                var pageContents = await response.Content.ReadAsStringAsync();

                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(pageContents);

                var table = htmlDocument.DocumentNode.SelectSingleNode("//table[@id='ipoCalendarData']");
                if (table == null)
                {
                    throw new Exception("No se pudo encontrar la tabla con los datos de OPVs.");
                }

                var rows = table.SelectNodes(".//tr");
                if (rows == null || rows.Count == 0)
                {
                    throw new Exception("No se encontraron filas en la tabla de datos de OPVs.");
                }

                foreach (var row in rows.Skip(1)) // Skip header row
                {
                    var cells = row.SelectNodes(".//td");
                    if (cells == null || cells.Count < 6)
                        continue;

                    var fecha = DateTime.ParseExact(cells[0].InnerText.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                    if (fecha < startDate || fecha > endDate)
                        continue;

                    var empresa = cells[1].SelectSingleNode(".//span[@class='elp']").InnerText.Trim();
                    var mercado = cells[2].InnerText.Trim();
                    var valor = cells[3].InnerText.Trim();
                    var precioSalida = float.Parse(cells[4].InnerText.Trim(), CultureInfo.InvariantCulture);
                    var precioUltimoCierre = float.Parse(cells[5].InnerText.Trim(), CultureInfo.InvariantCulture);

                    opvs.Add(new Opv
                    {
                        fecha = fecha,
                        empresa = empresa,
                        mercado = mercado,
                        valor = valor,
                        precioSalida = precioSalida,
                        precioUltimoCierre = precioUltimoCierre
                    });
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Manejo de errores específicos de solicitudes HTTP
                Console.WriteLine($"Error en la solicitud HTTP: {httpEx.Message}");
            }
            catch (FormatException formatEx)
            {
                // Manejo de errores de formato
                Console.WriteLine($"Error en el formato de los datos: {formatEx.Message}");
            }
            catch (Exception ex)
            {
                // Manejo de cualquier otro tipo de error
                Console.WriteLine($"Ocurrió un error: {ex.Message}");
            }

            return opvs;
        }
    }
}