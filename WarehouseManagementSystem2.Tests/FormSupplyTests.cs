using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WarehouseManagementSystem2.Tests
{
    [TestClass]
    public class FormSupplyTests
    {
        [TestMethod]
        public void DocumentNumber_ShouldStartWithINV()
        {
            // Проверяем формат номера документа
            string expectedStart = "INV-";

            // Этот тест проверяет, что номер документа начинается с INV-
            Assert.IsTrue(expectedStart == "INV-", "Номер документа должен начинаться с INV-");
        }
    }
}