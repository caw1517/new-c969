using C969_Project.Database;
using Xunit;

namespace C969_Project.Tests
{
    public class SmokeTests
    {
        [Fact]
        public void TestProjectIsWiredUp()
        {
            Assert.True(true);
        }

        [Fact]
        public void FullAddress_OmitsAddress2_WhenItIsBlank()
        {
            var customer = new CustomerDisplay
            {
                Address = "123 Main St",
                Address2 = ""
            };

            Assert.Equal("123 Main St", customer.FullAddress);
        }

        [Fact]
        public void FullAddress_JoinsBothLines_WhenAddress2IsPresent()
        {
            var customer = new CustomerDisplay
            {
                Address = "123 Main St",
                Address2 = "Apt 4"
            };

            Assert.Equal("123 Main St, Apt 4", customer.FullAddress);
        }
    }
}
