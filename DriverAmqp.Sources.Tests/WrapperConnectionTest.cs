using System;
using Xunit;
//using DriverAmqp.Sources;

namespace DriverAmqp.Sources.Tests
{
    public class WrapperConnectionTest
    {
        [Fact]
        public void GetInstance()
        {
            //Arrange

            //Act
            var result = WrapperConnection.GetInstance();

            //Assert
            Assert.NotNull(result);
        }
    }
}
