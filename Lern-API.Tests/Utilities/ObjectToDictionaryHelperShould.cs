using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    [ExcludeFromCodeCoverage]
    public class ObjectToDictionaryHelperShould
    {
        [Fact]
        public void Throw_On_Null_Object()
        {
            string var = null;

            Assert.Throws<ArgumentNullException>(() => var.ToDictionary());
        }

        [Theory]
        [AutoMoqData]
        public void Convert_Object_To_Dictionary(string obj)
        {
            var dictionary = obj.ToDictionary();

            var typeDescriptor = TypeDescriptor.GetProperties(obj);

            Assert.NotNull(dictionary);
            Assert.NotEmpty(dictionary);
            Assert.Equal(dictionary.Count, typeDescriptor.Count);

            foreach (PropertyDescriptor property in typeDescriptor)
            {
                var value = property.GetValue(obj);

                Assert.Equal(value, dictionary[property.Name]);
            }
        }
    }
}
