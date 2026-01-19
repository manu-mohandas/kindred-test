using KindredTest.Models;
using System.Text.Json;

namespace KindredTest.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer> GetCustomerAsync(int customerId);
    }
    public class CustomerRepository : ICustomerRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public CustomerRepository(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<Customer> GetCustomerAsync(int customerId)
        {
            var url = $"{_configuration["customerBaseUrl"]}/customer?customerId={customerId}&candidateId={_configuration["candidateId"]}";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var customer =  JsonSerializer.Deserialize<Customer>(content, 
                new JsonSerializerOptions 
                {
                    PropertyNameCaseInsensitive = true 
                });

            if (customer == null)
            {
                throw new InvalidOperationException("Failed to de-serialize customer json");
            }
            return customer;
        }
    }
}
