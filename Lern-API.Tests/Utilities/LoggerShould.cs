﻿using System.Globalization;
using System.Text.RegularExpressions;
using Lern_API.Tests.Attributes;
using Lern_API.Utilities;
using log4net;
using Moq;
using Nancy;
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

        [Theory]
        [AutoMoqData]
        public void Use_Debug_Level(Mock<ILog> logger, string message)
        {
            logger.Setup(l => l.DebugFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Debug(message);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Use_Info_Level(Mock<ILog> logger, string message)
        {
            logger.Setup(l => l.InfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Info(message);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Use_Warn_Level(Mock<ILog> logger, string message)
        {
            logger.Setup(l => l.WarnFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Warning(message);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Use_Error_Level(Mock<ILog> logger, string message)
        {
            logger.Setup(l => l.ErrorFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Error(message);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Wire_Requests_To_Info_Level(Mock<ILog> logger, int id, string method, string route, string address, string useragent)
        {
            logger.Setup(l => l.InfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Request(id, method, route, address, useragent);

            logger.VerifyAll();
        }

        [Theory]
        [AutoMoqData]
        public void Wire_Responses_To_Info_Level(Mock<ILog> logger, int id, HttpStatusCode status)
        {
            logger.Setup(l => l.InfoFormat(CultureInfo.InvariantCulture, It.IsAny<string>(), It.IsAny<object[]>()));

            var log = new Logger(logger.Object);
            log.Response(id, status);

            logger.VerifyAll();
        }
    }
}
