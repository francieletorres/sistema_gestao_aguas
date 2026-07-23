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
            _httpClient.BaseAddress = new Uri("https://localhost:44312/");
        }
        
        public async Task<Response> GetCustomers()
        {
            try
            {
                HttpResponseMessage response = await _httpClient.GetAsync("api/customers");

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

    }
}
