using System;
using System.IO;
using System.Text;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Nancy;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    public class RequestDeserializeHelperShould
    {
        [Theory]
        [AutoMoqData]
        public void Throw_On_Argument_Null(Request request, string data)
        {
            string var = null;

            Assert.Throws<ArgumentNullException>(() => request.DeserializeTo(var));
            Assert.Throws<ArgumentNullException>(() => RequestDeserializeHelper.DeserializeTo(null, data));
        }

        [Fact]
        public void Deserialize_Json_To_Anonymous_Type()
        {
            var request = new Request("GET", new Url(), new MemoryStream(Encoding.ASCII.GetBytes("{\"Param\":\"value\"}")));

            var type = new
            {
                Param = string.Empty
            };

            var result = request.DeserializeTo(type);

            Assert.NotNull(result);
            Assert.IsAssignableFrom(type.GetType(), result);
            Assert.Equal("value", result.Param);
        }

        [Fact]
        public void Deserialize_Json_To_Object()
        {
            var request = new Request("GET", new Url(), new MemoryStream(Encoding.ASCII.GetBytes("{\"Param\":\"value\"}")));

            var result = request.DeserializeTo<TestObject>();

            Assert.NotNull(result);
            Assert.Equal("value", result.Param);
        }

        private class TestObject
        {
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell",
                "S3459:Unassigned members should be removed", Justification = "Utilisé par la désérialization JSON")]
            public string Param { get; set; }
        }
    }
}
