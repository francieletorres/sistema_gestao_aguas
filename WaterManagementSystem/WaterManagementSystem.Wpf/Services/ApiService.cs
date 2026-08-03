using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using WaterManagementSystem.Wpf.Models;
using Newtonsoft.Json;

namespace WaterManagementSystem.Wpf.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService()
        {
            _httpClient = new HttpClient();
            //_httpClient.BaseAddress = new Uri("https://localhost:44312/api/");
            _httpClient.BaseAddress = new Uri("http://watermanagementtorres.somee.com/api/");
        }

        /// <summary>
        ///  Gets all customers from the API.
        /// </summary>
        /// <returns>A response containing the customer list or an error message.</returns>
        public async Task<Response> GetCustomers()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("customers");

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var customers = JsonConvert.DeserializeObject<List<Customer>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = customers
                };
            }
            catch(Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        ///  Sends a new customer to the API for registration.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>A response with the result of the customer creation.</returns>
        public async Task<Response> CreateCustomer(Customer customer)
        {
            try
            {
               var json = JsonConvert.SerializeObject(customer);

               var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("customers", content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }


        /// <summary>
        /// Sends updated customer data to the API.
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>A response with the result of the customer updated.</returns>
        public async Task<Response> UpdateCustomer(Customer customer)
        {
            try
            {
                var json = JsonConvert.SerializeObject(customer);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync("customers/" + customer.CustomerId, content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }
                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };

            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Requests the deletion of the customer from the API.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>A response with the result of the customer deleted.</returns>
        public async Task<Response> DeleteCustomer(int customerId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync("customers/" + customerId);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Gets all meters from the API.
        /// </summary>
        /// <returns>A response containing the meter list or an error message.</returns>
        public async Task<Response> GetMeters()
        {
            try
            {   //envia um pedido get para buscar todos os contadores na api
                HttpResponseMessage response = await _httpClient.GetAsync("meters"); 

                //lê o contéudo json que a api devolveu
                var result = await response.Content.ReadAsStringAsync();

                //verifica se a api devolveu algum erro
                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                //converte o Json em uma lista de objetos meter
                var meters = JsonConvert.DeserializeObject<List<Meter>>(result);

                //devolve a lista de contadores dentro do result
                return new Response
                {
                    IsSuccess = true,
                    Result = meters
                };
            }
            catch(Exception)
            {
                // Trata erros de ligação ou outros problemas.
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };

            }
        }

        /// <summary>
        /// Gets the meters associated with the specified customer.
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns>A response containing the customer's meters or an error message.</returns>
        public async Task<Response> GetMetersByCustomer(int customerId)
        {
            try
            {   //envia um pedido get para buscar o contador de um cliente especifico
                HttpResponseMessage response = await _httpClient.GetAsync("meters/customer/" + customerId);

                //lê o contéudo json que a api devolveu
                var result = await response.Content.ReadAsStringAsync();

                //verifica se a api devolveu algum erro
                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                //converte o Json em uma lista de objetos meter
                var meters = JsonConvert.DeserializeObject<List<Meter>>(result);

                //devolve a lista de contadores dentro do result
                return new Response
                {
                    IsSuccess = true,
                    Result = meters
                };
            }
            catch (Exception)
            {
                // Trata erros de ligação ou outros problemas.
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };

            }
        }

        /// <summary>
        /// Sends a new meter to the API for registration.
        /// </summary>
        /// <param name="meter"></param>
        /// <returns>A response with the result of the meter creation.</returns>
        public async Task<Response> CreateMeter(Meter meter)
        {
            try
            {
                var json = JsonConvert.SerializeObject(meter);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("meters", content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Sends updated meter data to the API.
        /// </summary>
        /// <param name="meter"></param>
        /// <returns>A response with the result of the meter updated.</returns>
        public async Task<Response> UpdateMeter(Meter meter)
        {  
            try
            {
                var json = JsonConvert.SerializeObject(meter);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync("meters/" + meter.MeterId, content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Requests the deletion of the meter from the API.
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>A response with the result of the meter deleted.</returns>
        public async Task<Response> DeleteMeter(int meterId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync("meters/" + meterId);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Gets all consumptions from the API.
        /// </summary>
        /// <returns>A response containing the consumption list or an error message.</returns>
        public async Task<Response> GetConsumptions()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("consumptions/");

                var result = await response.Content.ReadAsStringAsync();

                if(!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var consumptions = JsonConvert.DeserializeObject<List<Consumption>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = consumptions
                };

            }
            catch(Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        ///  Gets the consumptions associated with the specified meter.
        /// </summary>
        /// <param name="meterId"></param>
        /// <returns>A response containing the meter's consumptions or an error message.</returns>
        public async Task<Response> GetConsumptionsByMeter(int meterId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("consumptions/meter/" + meterId);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var consumptions = JsonConvert.DeserializeObject<List<Consumption>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = consumptions
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };

            }
        }


        /// <summary>
        /// Sends a new consumption to the API for registration.
        /// </summary>
        /// <param name="consumption"></param>
        /// <returns>A response with the result of the consumption creation.</returns>
        public async Task<Response> CreateConsumption(Consumption consumption)
        {
            try
            {
                var json = JsonConvert.SerializeObject(consumption);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("consumptions", content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch(Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            } 
        }

        /// <summary>
        ///  Sends updated consumption data to the API.
        /// </summary>
        /// <param name="consumption"></param>
        /// <returns>A response with the result of the consumption updated.</returns>
        public async Task<Response> UpdateConsumption(Consumption consumption)
        {
            try
            {
                var json = JsonConvert.SerializeObject(consumption);

                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PutAsync("consumptions/" + consumption.ConsumptionId, content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }
                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Requests the deletion of the consumption from the API.
        /// </summary>
        /// <param name="consumptionId"></param>
        /// <returns>A response with the result of the consumption deleted.</returns>
        public async Task<Response> DeleteConsumption(int consumptionId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.DeleteAsync("consumptions/" + consumptionId);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }
                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        /// Gets all invoices from the API.
        /// </summary>
        /// <returns>A response containing the invoice list or an error message.</returns>
        public async Task<Response> GetInvoices()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("invoices/");

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var invoices = JsonConvert.DeserializeObject<List<Invoice>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = invoices
                };
            }
            catch(Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        ///  Gets a specific invoice by its identifier.
        /// </summary>
        /// <param name="invoiceId"></param>
        /// <returns>A response containing the requested invoice or an error message.</returns>
        public async Task<Response> GetInvoiceById(int invoiceId)
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("invoices/" + invoiceId);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var invoice = JsonConvert.DeserializeObject<Invoice>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = invoice
                };
            }

            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }

        /// <summary>
        ///  Searches for invoices using optional customer, date, payment,
        /// and cancellation filters.
        /// </summary>
        /// <returns> A response containing the matching invoices or an error message.</returns>
        public async Task<Response> SearchInvoices(
            int? customerId,
            DateTime? startDate,
            DateTime? endDate,
            bool? isPaid,
            bool? isCancelled)
        {
            try
            {
                List<string> filters = new List<string>();

                if (customerId.HasValue)
                {
                    filters.Add("customerId=" + customerId.Value);
                }

                if (startDate.HasValue)
                {
                    filters.Add("startDate=" + startDate.Value.ToString("yyyy-MM-dd"));
                }

                if (endDate.HasValue)
                {
                    filters.Add("endDate=" + endDate.Value.ToString("yyyy-MM-dd"));
                }

                if (isPaid.HasValue)
                {
                    filters.Add("isPaid=" + isPaid.Value.ToString().ToLower());
                }

                if (isCancelled.HasValue)
                {
                    filters.Add("isCancelled=" + isCancelled.Value.ToString().ToLower());
                }

                string url = "Invoices/Search";

                if (filters.Count > 0)
                {
                    url += "?" + string.Join("&", filters);
                }

                HttpResponseMessage response = await _httpClient.GetAsync(url);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                var invoices = JsonConvert.DeserializeObject<List<Invoice>>(result);

                return new Response
                {
                    IsSuccess = true,
                    Result = invoices
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }


        /// <summary>
        /// Sends a new invoice to the API for registration.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns>A response with the result of the invoice creation.</returns>
        public async Task<Response> CreateInvoice(Invoice invoice)
        {
            try
            {
                string json = JsonConvert.SerializeObject(invoice);

                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await _httpClient.PostAsync("invoices", content);

                string result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }


        /// <summary>
        ///  Sends updated invoice data to the API.
        /// </summary>
        /// <param name="invoice"></param>
        /// <returns>A response with the result of the invoice updated.</returns>
        public async Task<Response> UpdateInvoice(Invoice invoice)
        {
            try
            {
                string json = JsonConvert.SerializeObject(invoice);

                StringContent content = new StringContent(json,Encoding.UTF8,"application/json");

                HttpResponseMessage response = await _httpClient.PutAsync("invoices/" + invoice.InvoiceId, content);

                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new Response
                    {
                        IsSuccess = false,
                        Message = result
                    };
                }

                return new Response
                {
                    IsSuccess = true,
                    Message = result
                };
            }
            catch (Exception)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = "Não foi possível comunicar com o servidor. Verifique a ligação à internet ou tente novamente."
                };
            }
        }
    }
}
