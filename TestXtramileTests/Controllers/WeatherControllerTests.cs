
using NUnit.Framework;
using Moq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using TestXtramile.Controllers;
using Microsoft.AspNetCore.Mvc;
using System;
using Moq.Protected;
using System.Threading;
using Newtonsoft.Json;

namespace TestXtramile.Tests
{
    [TestFixture]
    public class WeatherControllerTests
    {
        [Test]
        public async Task CallAPI_ReturnsOkResult_WhenApiResponseIsSuccess()
        {
            // Arrange
            string city = "London";
            string expectedContentTimezone = "3600";
            //string expectedContent = "{ \"temperature\": 20 }";
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(expectedContentTimezone)
                });
            var httpClient = new HttpClient(mockHandler.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.CallAPI(city);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var contentResult = (OkObjectResult)result;
            var response = JsonConvert.DeserializeObject<dynamic>(contentResult.Value.ToString());
            string timezone = response.timezone;

            Assert.AreEqual(expectedContentTimezone, timezone);
        }

        [Test]
        public async Task CallAPI_ReturnsOkResult_WhenApiResponseIsError()
        {
            // Arrange
            string city = "London 123";
            string expectedContent = "city not found";
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent(expectedContent)
                });
            var httpClient = new HttpClient(mockHandler.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.CallAPI(city);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var contentResult = (OkObjectResult)result;
            var response = JsonConvert.DeserializeObject<dynamic>(contentResult.Value.ToString());
            string message = response.message;

            Assert.AreEqual(expectedContent, message);
        }

        [Test]
        public async Task CallAPI_ReturnsOkResult_WhenApiThrowsException()
        {
            // Untuk mengetes ini url di client.BaseAddress = new Uri("http://api.openweathermap.org"); harus di buat salan url nya
            string city = "London";
            string expectedContent = "No such host is known.";
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            mockHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new Exception());
            var httpClient = new HttpClient(mockHandler.Object);
            var controller = new WeatherController(httpClient);

            // Act
            var result = await controller.CallAPI(city);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var contentResult = (OkObjectResult)result;
            Assert.AreEqual(expectedContent, contentResult.Value.ToString());
        }



    }
}
