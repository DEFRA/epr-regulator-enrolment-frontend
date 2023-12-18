namespace FrontendRegulatorAccountEnrollment.Web.Extensions;

public static class LoggerExtensions
{

    private const byte ErrorId = 0;
    private const byte InfoId = 1;

    private static Action<ILogger, string, Exception> _loggerMessageError =
        LoggerMessage.Define<String>(
            LogLevel.Error,
            eventId: new EventId(id: ErrorId, name: "ERROR"),
            formatString: "{Message}");

    private static Action<ILogger, string, Exception> _loggerMessageInfo =
        LoggerMessage.Define<String>(
            LogLevel.Information,
            eventId: new EventId(id: InfoId, name: "INFO"),
            formatString: "{Message}");

    public static void LogErrors<T>(this ILogger<T> logger, Exception ex) => _loggerMessageError(logger, ex.ToString(), ex);

    public static void LogInfo<T>(this ILogger<T> logger, string message) => _loggerMessageInfo(logger, message, null!);
}