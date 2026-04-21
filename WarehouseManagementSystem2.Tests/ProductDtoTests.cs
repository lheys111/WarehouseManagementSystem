using Microsoft.VisualStudio.TestTools.UnitTesting;
using WarehouseManagementSystem.Models;

namespace WarehouseManagementSystem2.Tests
{
    [TestClass]
    public class ProductDtoTests
    {
        [TestMethod]
        public void ProductDto_ShouldSetAndGetProperties()
        {
            // Arrange
            var product = new ProductDto
            {
                Id = 1,
                Article = "PR-001",
                Name = "Гречка",
                PurchasePrice = 45.50m
            };

            // Assert
            Assert.AreEqual(1, product.Id);
            Assert.AreEqual("PR-001", product.Article);
            Assert.AreEqual("Гречка", product.Name);
            Assert.AreEqual(45.50m, product.PurchasePrice);
        }
    }
}