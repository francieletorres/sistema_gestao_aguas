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
            _httpClient.BaseAddress = new Uri("https://localhost:44312/api/");
            //_httpClient.BaseAddress = new Uri("http://watermanagementtorres.somee.com/api/");
        }
        
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
            catch(Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch(Exception ex)
            {
                // Trata erros de ligação ou outros problemas.
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };

            }
        }

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
            catch (Exception ex)
            {
                // Trata erros de ligação ou outros problemas.
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };

            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch(Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };

            }
        }


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
            catch(Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            } 
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }

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
            catch (Exception ex)
            {
                return new Response
                {
                    IsSuccess = false,
                    Message = ex.Message
                };
            }
        }
    }
}
