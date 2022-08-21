using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace VueServer.Test.Integration
{
    public static class LoggerUtilities
    {
        public static Mock<ILogger<T>> LoggerMock<T>() where T : class
        {
            return new Mock<ILogger<T>>();
        }

        public static ILogger<T> Logger<T>() where T : class
        {
            return LoggerMock<T>().Object;
        }

        public static void VerifyLog<T>(
            this Mock<ILogger<T>> loggerMock,
            LogLevel level,
            string message,
            Exception e = null,
            string failMessage = null)
        {
            loggerMock.VerifyLog(
                level,
                message,
                Times.Once(),
                failMessage,
                e);
        }

        public static void VerifyLog<T>(
            this Mock<ILogger<T>> loggerMock,
            LogLevel level,
            string message,
            Times times,
            string failMessage = null,
            Exception e = null)
        {
            loggerMock.Verify(l => l.Log<Object>(
                level,
                It.IsAny<EventId>(),
                It.Is<Object>(o => o.ToString() == message),
                e,
                It.IsAny<Func<Object, Exception, String>>()),
                times,
                failMessage);
        }

    }
}
