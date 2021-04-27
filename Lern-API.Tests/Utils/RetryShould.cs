using System;
using Lern_API.Tests.Attributes;
using Lern_API.Utils;
using Xunit;

namespace Lern_API.Tests.Utils
{
    public class RetryShould
    {
        [Theory]
        [AutoMoqData]
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
            Assert.Throws<AggregateException>(() => Retry.Do(() => throw new Exception(), TimeSpan.Zero, maxRetry));
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