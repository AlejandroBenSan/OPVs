using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;
using HtmlAgilityPack;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
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
            var tcs = new TaskCompletionSource<List<Opv>>();

            try
            {
                await Task.Run(() =>
                {
                    var opvs = new List<Opv>();
                    var url = "https://es.investing.com/ipo-calendar/";

                    var chromeDriverService = ChromeDriverService.CreateDefaultService();
                    chromeDriverService.HideCommandPromptWindow = true;
                    var options = new ChromeOptions();
                    options.AddArguments("--headless", "--no-sandbox", "--disable-web-security", "--disable-gpu", "--incognito", "--proxy-bypass-list=*", "--proxy-server='direct://'", "--log-level=3", "--hide-scrollbars");
                    using (var driver = new ChromeDriver(chromeDriverService, options))
                    {
                        driver.Navigate().GoToUrl(url);

                        // Handle cookies or other overlays
                        try
                        {
                            var cookieButton = driver.FindElement(By.Id("onetrust-accept-btn-handler"));
                            if (cookieButton != null)
                            {
                                cookieButton.Click();
                            }
                        }
                        catch (NoSuchElementException)
                        {
                            // If the element is not found, continue
                        }

                        WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

                        // Click the date picker toggle button
                        var datePickerToggleBtn = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id("datePickerToggleBtn")));
                        datePickerToggleBtn.Click();

                        // Find the start date and end date input fields and set their values
                        var startDateInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("startDate")));
                        startDateInput.Clear();
                        startDateInput.SendKeys(startDate.ToString("dd/MM/yyyy"));

                        // Re-locate the end date input field just before interacting with it
                        var endDateInput = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("endDate")));
                        endDateInput.Clear();
                        endDateInput.SendKeys(endDate.ToString("dd/MM/yyyy"));

                        // Find the apply button and click it
                        var applyButton = driver.FindElement(By.Id("applyBtn"));
                        applyButton.Click();

                        // Wait for the page to reload and ensure the table is present
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id("ipoCalendarData")));

                        // Parse the page content after the form submission
                        var pageContentsFinal = driver.PageSource;
                        var htmlDocumentFinal = new HtmlAgilityPack.HtmlDocument();
                        htmlDocumentFinal.LoadHtml(pageContentsFinal);

                        var table = htmlDocumentFinal.DocumentNode.SelectSingleNode("//table[@id='ipoCalendarData']");
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

                            string baseUrl = "https://es.investing.com";

                            var empresa = cells[1].SelectSingleNode(".//span[@class='elp']").InnerText.Trim();

                            // OBTENER EL ENLACE
                            var enlaceNode = cells[1].SelectSingleNode(".//a[@class='bold']");
                            var enlace = enlaceNode != null ? enlaceNode.GetAttributeValue("href", string.Empty) : string.Empty;

                            var enlaceCompleto = baseUrl + enlace;

                            //TICKET
                            var ticker = enlaceNode != null ? enlaceNode.InnerText.Trim() : string.Empty;

                            var mercado = cells[2].InnerText.Trim();
                            var valor = cells[3].InnerText.Trim();

                            var precioSalida = 0.0f;

                            if (string.IsNullOrEmpty(cells[4].InnerText.Trim()) || cells[4].InnerText.Trim() == "-")
                            {
                                precioSalida = 0.0f;
                            }
                            else
                            {
                                precioSalida = float.Parse(cells[4].InnerText.Trim(), NumberStyles.Float, new CultureInfo("es-ES"));
                            }

                            var precioUltimoCierre = 0.0f;

                            if (string.IsNullOrEmpty(cells[5].InnerText.Trim()) || cells[5].InnerText.Trim() == "-")
                            {
                                precioSalida = 0.0f;
                            }
                            try
                            {
                                precioUltimoCierre = float.Parse(cells[5].InnerText.Trim(), NumberStyles.Float, new CultureInfo("es-ES"));
                            }
                            catch (FormatException)
                            {
                                // Replace '.' with ',' and try to parse again
                                string correctedInput = cells[5].InnerText.Trim().Replace('.', ',');
                                try
                                {
                                    precioUltimoCierre = float.Parse(correctedInput, NumberStyles.Float, new CultureInfo("es-ES"));
                                }
                                catch (FormatException innerEx)
                                {
                                    // Handle the case where the input is still not valid after replacement
                                    Console.WriteLine($"Unable to parse the input: {cells[0]}. Exception: {innerEx.Message}");
                                    precioUltimoCierre = 0.0f; // Set a default value or handle the error as needed
                                }
                            }

                            opvs.Add(new Opv
                            {
                                fecha = fecha,
                                empresa = empresa,
                                ticket = ticker,
                                enlace = enlaceCompleto,
                                mercado = mercado,
                                valor = valor,
                                precioSalida = precioSalida,
                                precioUltimoCierre = precioUltimoCierre
                            });
                        }
                    }

                    tcs.SetResult(opvs);
                });
            }
            catch (StaleElementReferenceException ex)
            {
                Console.WriteLine($"Stale Element Reference: {ex.Message}");
                tcs.SetResult(new List<Opv>());
            }
            catch (WebDriverTimeoutException ex)
            {
                Console.WriteLine($"Timeout: {ex.Message}");
                tcs.SetResult(new List<Opv>());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                tcs.SetResult(new List<Opv>());
            }

            return await tcs.Task;
        }

    }
}
