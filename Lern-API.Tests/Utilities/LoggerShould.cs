using System.Globalization;
using System.Text.RegularExpressions;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using log4net;
using Moq;
using Xunit;

namespace Lern_API.Tests.Utilities
{
    public class LoggerShould
    {
        [Fact]
        public void Have_Valid_Default_Format()
        {
            var format = Logger.DefaultFormat;

            Assert.NotNull(format);
            Assert.Matches(new Regex(@"\{0\}"), format);
            Assert.Matches(new Regex(@"\{1\}"), format);
            Assert.Matches(new Regex(@"\{2\}"), format);
            Assert.Matches(new Regex(@"\{3\}"), format);
        }

        [Theory]
        [AutoMoqData]
        public void Respect_Given_Format(string format)
        {
            Logger.Format = format;

            Assert.Equal(format, Logger.Format);
        }

        [Fact]
        public void Return_Valid_Instance_Logger()
        {
            var logger = Logger.GetLogger(this);

            Assert.NotNull(logger);
        }

        [Fact]
        public void Return_Valid_Type_Logger()
        {
            var logger = Logger.GetLogger(GetType());

            Assert.NotNull(logger);
        }

#pragma warning disable S3236 // Caller information arguments should not be provided explicitly

        [Theory]
        [AutoMoqData]
        public void Use_Info_Level(Mock<ILog> logger, string file, string memberName, int line, string message)
        {
            logger.Setup(l => l.InfoFormat(CultureInfo.InvariantCulture, Logger.Format, file, memberName, line, message));

            var log = new Logger(logger.Object);

            log.Info(message, memberName, file, line);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Use_Warn_Level(Mock<ILog> logger, string file, string memberName, int line, string message)
        {
            logger.Setup(l => l.WarnFormat(CultureInfo.InvariantCulture, Logger.Format, file, memberName, line, message));

            var log = new Logger(logger.Object);
            log.Warning(message, memberName, file, line);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Use_Error_Level(Mock<ILog> logger, string file, string memberName, int line, string message)
        {
            logger.Setup(l => l.ErrorFormat(CultureInfo.InvariantCulture, Logger.Format, file, memberName, line, message));

            var log = new Logger(logger.Object);
            log.Error(message, memberName, file, line);

            logger.VerifyAll();
        }

#pragma warning restore S3236 // Caller information arguments should not be provided explicitly

    }
}
