using System;
using System.Diagnostics.CodeAnalysis;
using Lern_API.Helpers.Database;
using Lern_API.Tests.Attributes;
using Xunit;

namespace Lern_API.Tests.Helpers
{
    public class RetryShould
    {
        [Fact]
        public void Throw_On_Invalid_Parameters()
        {
            Assert.Throws<ArgumentNullException>(() => Retry.Do(null, TimeSpan.Zero));
            Assert.Throws<ArgumentNullException>(() => Retry.Do(null, TimeSpan.Zero, 5));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => {}, TimeSpan.Zero, 0));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => false, TimeSpan.Zero, 0));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => {}, TimeSpan.Zero, int.MinValue));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => false, TimeSpan.Zero,int.MinValue));
        }

        [Theory]
        [AutoMoqData]
        [SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty")]
        public void Retry_Given_Times(int maxRetry)
        {
            var retried = 0;

            try
            {
                Retry.Do(() =>
                {
                    retried++;

                    if (retried > maxRetry)
                        Assert.True(false);

                    throw new Exception();
                }, TimeSpan.Zero, maxRetry);
            }
            catch (Exception)
            {
                // ignored
            }

            Assert.Equal(maxRetry, retried);
        }

        [Theory]
        [AutoMoqData]
        [SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty")]
        public void Retry_Given_Times_With_Return_Value(int maxRetry)
        {
            var retried = 0;

            try
            {
                Retry.Do(() =>
                {
                    retried++;

                    if (retried > maxRetry)
                        Assert.True(false);

                    throw new Exception();

#pragma warning disable CS0162 // Code inaccessible détecté
                    return "";
#pragma warning restore CS0162 // Code inaccessible détecté
                }, TimeSpan.Zero, maxRetry);
            }
            catch (Exception)
            {
                // ignored
            }

            Assert.Equal(maxRetry, retried);
        }

        [Theory]
        [AutoMoqData]
        public void Return_Given_Value(string returnValue, int maxRetry)
        {
            var result = Retry.Do(() => returnValue, TimeSpan.Zero, maxRetry);

            Assert.Equal(returnValue, result);
        }

        [Theory]
        [AutoMoqData]
        [SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant", Justification = "")]
        public void Throw_On_Max_Retry(int maxRetry)
        {
            Assert.Throws<AggregateException>(() => Retry.Do(() => throw new Exception(), TimeSpan.Zero, maxRetry));
        }

        [Theory]
        [AutoMoqData]
        [SuppressMessage("Minor Code Smell", "S3626:Jump statements should not be redundant", Justification = "")]
        public void Throw_On_Max_Retry_With_Return_Value(int maxRetry)
        {
            Assert.Throws<AggregateException>(() => Retry.Do(() =>
            {
                throw new Exception();

#pragma warning disable CS0162 // Code inaccessible détecté
                return "";
#pragma warning restore CS0162 // Code inaccessible détecté
            }, TimeSpan.Zero, maxRetry));
        }
    }
}
