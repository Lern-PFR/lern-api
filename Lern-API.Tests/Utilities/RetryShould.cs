using System;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    public class RetryShould
    {
        [Fact]
        public void Throw_On_Invalid_Parameters()
        {
            Assert.Throws<ArgumentNullException>(() => Retry.Do(null, TimeSpan.Zero));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => {}, TimeSpan.Zero, 0));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => false, TimeSpan.Zero, 0));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => {}, TimeSpan.Zero, int.MinValue));
            Assert.Throws<ArgumentException>(() => Retry.Do(() => false, TimeSpan.Zero,int.MinValue));
        }

        [Theory]
        [AutoMoqData]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty")]
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
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S108:Nested blocks of code should not be left empty")]
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
        public void Throw_On_Max_Retry(int maxRetry)
        {
            Assert.Throws<AggregateException>(() => Retry.Do(() =>
            {
                throw new Exception();
            }, TimeSpan.Zero, maxRetry));
        }

        [Theory]
        [AutoMoqData]
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
