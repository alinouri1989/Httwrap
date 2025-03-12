using Httwrap.Interface;
using System.Net;

namespace Httwrap.Test
{
    [TestClass]
    public class ProductApiTests
    {
        private const string BaseAddress = "http://localhost:9000/api/";
        private IHttwrapClient _client;

        [TestInitialize]
        public void Setup()
        {
            IHttwrapConfiguration configuration = new HttwrapConfiguration(BaseAddress);
            _client = new HttwrapClient(configuration);
        }

        [TestMethod]
        public async Task GetProducts_ShouldReturnOk()
        {
            // Act  
            var response = await _client.GetAsync<IEnumerable<Product>>("products");

            // Assert  
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Expected OK status code.");
            Assert.IsNotNull(response.Data, "Product list should not be null.");
        }

        [TestMethod]
        public async Task CreateProduct_ShouldReturnCreated()
        {
            // Arrange  
            var newProduct = new Product { Name = "Test Product", Price = 29.99M };

            // Act  
            var response = await _client.PostAsync("products", newProduct);

            // Assert  
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "Expected Created status code.");
        }

        [TestMethod]
        public async Task DeleteProduct_ShouldReturnNoContent()
        {
            // Arrange  
            // This assumes that you have previously created a product with ID 1  
            const int productIdToDelete = 1;

            // Act  
            var response = await _client.DeleteAsync($"products/{productIdToDelete}");

            // Assert  
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode, "Expected No Content status code.");
        }
    }
}
