using System;
using System.Net.WebSockets;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Sockets.Internal
{
    internal static class SocketLoggerExtensions
    {
        // Category: Shared with LongPollingTransport, WebSocketsTransport and ServerSentEventsTransport
        private static readonly Action<ILogger, DateTime, Exception> _startTransport =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 0, "{time}: Starting transport.");

        private static readonly Action<ILogger, DateTime, Exception> _transportStopped =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 1, "{time}: Transport stopped.");

        private static readonly Action<ILogger, DateTime, Exception> _startReceive =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 2, "{time}: Starting receive loop.");

        private static readonly Action<ILogger, DateTime, Exception> _receiveStopped =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 8, "{time}: Receive loop stopped.");

        private static readonly Action<ILogger, DateTime, Exception> _receiveCanceled =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 7, "{time}: Receive loop canceled.");

        private static readonly Action<ILogger, DateTime, Exception> _transportStopping =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 3, "{time}: Transport is stopping.");

        private static readonly Action<ILogger, DateTime, Exception> _sendStarted =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 9, "{time}: Starting the send loop.");

        private static readonly Action<ILogger, DateTime, Exception> _sendStopped =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 14, "{time}: Send loop stopped.");

        private static readonly Action<ILogger, DateTime, Exception> _sendCanceled =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 13, "{time}: Send loop canceled.");

        // Category: WebSocketsTransport

        private static readonly Action<ILogger, DateTime, WebSocketCloseStatus?, Exception> _webSocketClosed =
            LoggerMessage.Define<DateTime, WebSocketCloseStatus?>(LogLevel.Information, 4, "{time}: Websocket closed by the server. Close status {closeStatus}.");

        private static readonly Action<ILogger, DateTime, WebSocketMessageType, int, bool, Exception> _messageReceived =
            LoggerMessage.Define<DateTime, WebSocketMessageType, int, bool>(LogLevel.Debug, 5, "{time}: Message received. Type: {messageCount}, size: {count}, EndOfMessage: {endOfMessage}.");

        private static readonly Action<ILogger, DateTime, int, Exception> _messageToApp =
            LoggerMessage.Define<DateTime, int>(LogLevel.Information, 6, "{time}: Passing message to application. Payload size: {count}.");

        private static readonly Action<ILogger, DateTime, int, Exception> _receivedFromApp =
            LoggerMessage.Define<DateTime, int>(LogLevel.Debug, 10, "{time}: Received message from application. Payload size: {count}.");

        private static readonly Action<ILogger, DateTime, Exception> _sendMessageCanceled =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 11, "{time}: Sending a message canceled.");

        private static readonly Action<ILogger, DateTime, Exception> _errorSendingMessage =
            LoggerMessage.Define<DateTime>(LogLevel.Error, 12, "{time}: Error while sending a message.");

        private static readonly Action<ILogger, DateTime, Exception> _closingWebSocket =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 15, "{time}: Closing WebSocket.");

        private static readonly Action<ILogger, DateTime, Exception> _closingWebSocketFailed =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 16, "{time}: Closing webSocket failed.");

        // Category: ServerSentEventsTransport
        private static readonly Action<ILogger, DateTime, Exception> _eventStreamEnded =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 6, "{time}: Server-Sent Event Stream ended.");

        // Category: LongPollingTransport
        private static readonly Action<ILogger, DateTime, Exception> _closingConnection =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 6, "{time}: The server is closing the connection.");

        private static readonly Action<ILogger, DateTime, Exception> _receivedMessages =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 6, "{time}: Received messages from the server.");

        private static readonly Action<ILogger, DateTime, Uri, Exception> _errorPolling =
            LoggerMessage.Define<DateTime, Uri>(LogLevel.Error, 6, "{time}: Error while polling '{pollUrl}'.");

        // Category: HttpConnection
        private static readonly Action<ILogger, DateTime, Exception> _httpConnectionStarting =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 0, "{time}: Starting connection.");

        private static readonly Action<ILogger, DateTime, Exception> _httpConnectionClosed =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 1, "{time}: Connection was closed from a different thread.");

        private static readonly Action<ILogger, DateTime, string, Uri, Exception> _startingTransport =
            LoggerMessage.Define<DateTime, string, Uri>(LogLevel.Debug, 2, "{time}: Starting transport '{transport}' with Url: {url}.");

        private static readonly Action<ILogger, DateTime, Exception> _raiseConnected =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 3, "{time}: Raising Connected event.");

        private static readonly Action<ILogger, DateTime, Exception> _processRemainingMessages =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 4, "{time}: Ensuring all outstanding messages are processed.");

        private static readonly Action<ILogger, DateTime, Exception> _drainEvents =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 5, "{time}: Draining event queue.");

        private static readonly Action<ILogger, DateTime, Exception> _raiseClosed =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 6, "{time}: Raising Closed event.");

        private static readonly Action<ILogger, DateTime, Uri, Exception> _establishingConnection =
            LoggerMessage.Define<DateTime, Uri>(LogLevel.Debug, 7, "{time}: Establishing Connection at: {url}.");

        private static readonly Action<ILogger, DateTime, Uri, Exception> _errorWithNegotiation =
            LoggerMessage.Define<DateTime, Uri>(LogLevel.Error, 8, "{time}: Failed to start connection. Error getting negotiation response from '{url}'.");

        private static readonly Action<ILogger, DateTime, string, Exception> _errorStartingTransport =
            LoggerMessage.Define<DateTime, string>(LogLevel.Error, 9, "{time}: Failed to start connection. Error starting transport '{transport}'.");

        private static readonly Action<ILogger, DateTime, Exception> _httpReceiveStarted =
            LoggerMessage.Define<DateTime>(LogLevel.Trace, 10, "{time}: Beginning receive loop.");

        private static readonly Action<ILogger, DateTime, Exception> _skipRaisingReceiveEvent =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 11, "{time}: Message received but connection is not connected. Skipping raising Received event.");

        private static readonly Action<ILogger, DateTime, Exception> _scheduleReceiveEvent =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 12, "{time}: Scheduling raising Received event.");

        private static readonly Action<ILogger, DateTime, Exception> _raiseReceiveEvent =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 13, "{time}: Raising Received event.");

        private static readonly Action<ILogger, DateTime, Exception> _failedReadingMessage =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 14, "{time}: Could not read message.");

        private static readonly Action<ILogger, DateTime, Exception> _errorReceiving =
            LoggerMessage.Define<DateTime>(LogLevel.Error, 15, "{time}: Error receiving message.");

        private static readonly Action<ILogger, DateTime, Exception> _endReceive =
            LoggerMessage.Define<DateTime>(LogLevel.Trace, 16, "{time}: Ending receive loop.");

        private static readonly Action<ILogger, DateTime, Exception> _sendingMessage =
            LoggerMessage.Define<DateTime>(LogLevel.Debug, 17, "{time}: Sending message.");

        private static readonly Action<ILogger, DateTime, Exception> _stoppingClient =
            LoggerMessage.Define<DateTime>(LogLevel.Information, 18, "{time}: Stopping client.");

        public static void StartTransport(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _startTransport(logger, DateTime.Now, null);
            }
        }

        public static void TransportStopped(this ILogger logger, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _transportStopped(logger, DateTime.Now, exception);
            }
        }

        public static void StartReceive(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _startReceive(logger, DateTime.Now, null);
            }
        }

        public static void TransportStopping(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _transportStopping(logger, DateTime.Now, null);
            }
        }

        public static void WebSocketClosed(this ILogger logger, WebSocketCloseStatus? closeStatus)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _webSocketClosed(logger, DateTime.Now, closeStatus, null);
            }
        }

        public static void MessageReceived(this ILogger logger, WebSocketMessageType messageType, int count, bool endOfMessage)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _messageReceived(logger, DateTime.Now, messageType, count, endOfMessage, null);
            }
        }

        public static void MessageToApp(this ILogger logger, int count)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _messageToApp(logger, DateTime.Now, count, null);
            }
        }

        public static void ReceiveCanceled(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _receiveCanceled(logger, DateTime.Now, null);
            }
        }

        public static void ReceiveStopped(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _receiveStopped(logger, DateTime.Now, null);
            }
        }

        public static void SendStarted(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _sendStarted(logger, DateTime.Now, null);
            }
        }

        public static void ReceivedFromApp(this ILogger logger, int count)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _receivedFromApp(logger, DateTime.Now, count, null);
            }
        }

        public static void SendMessageCanceled(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _sendMessageCanceled(logger, DateTime.Now, null);
            }
        }

        public static void ErrorSendingMessage(this ILogger logger, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorSendingMessage(logger, DateTime.Now, exception);
            }
        }

        public static void SendCanceled(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _sendCanceled(logger, DateTime.Now, null);
            }
        }

        public static void SendStopped(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _sendStopped(logger, DateTime.Now, null);
            }
        }

        public static void ClosingWebSocket(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _closingWebSocket(logger, DateTime.Now, null);
            }
        }

        public static void ClosingWebSocketFailed(this ILogger logger, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _closingWebSocketFailed(logger, DateTime.Now, exception);
            }
        }

        public static void EventStreamEnded(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _eventStreamEnded(logger, DateTime.Now, null);
            }
        }

        public static void ClosingConnection(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _closingConnection(logger, DateTime.Now, null);
            }
        }

        public static void ReceivedMessages(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _receivedMessages(logger, DateTime.Now, null);
            }
        }

        public static void ErrorPolling(this ILogger logger, Uri pollUrl, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorPolling(logger, DateTime.Now, pollUrl, exception);
            }
        }

        public static void HttpConnectionStarting(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _httpConnectionStarting(logger, DateTime.Now, null);
            }
        }

        public static void HttpConnectionClosed(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _httpConnectionClosed(logger, DateTime.Now, null);
            }
        }

        public static void StartingTransport(this ILogger logger, string transport, Uri url)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _startingTransport(logger, DateTime.Now, transport, url, null);
            }
        }

        public static void RaiseConnected(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _raiseConnected(logger, DateTime.Now, null);
            }
        }

        public static void ProcessRemainingMessages(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _processRemainingMessages(logger, DateTime.Now, null);
            }
        }

        public static void DrainEvents(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _drainEvents(logger, DateTime.Now, null);
            }
        }

        public static void RaiseClosed(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _raiseClosed(logger, DateTime.Now, null);
            }
        }

        public static void EstablishingConnection(this ILogger logger, Uri url)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _establishingConnection(logger, DateTime.Now, url, null);
            }
        }

        public static void ErrorWithNegotiation(this ILogger logger, Uri url, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorWithNegotiation(logger, DateTime.Now, url, exception);
            }
        }

        public static void ErrorStartingTransport(this ILogger logger, string transport, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorStartingTransport(logger, DateTime.Now, transport, exception);
            }
        }

        public static void HttpReceiveStarted(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                _httpReceiveStarted(logger, DateTime.Now, null);
            }
        }

        public static void SkipRaisingReceiveEvent(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _skipRaisingReceiveEvent(logger, DateTime.Now, null);
            }
        }

        public static void ScheduleReceiveEvent(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _scheduleReceiveEvent(logger, DateTime.Now, null);
            }
        }

        public static void RaiseReceiveEvent(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _raiseReceiveEvent(logger, DateTime.Now, null);
            }
        }

        public static void FailedReadingMessage(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _failedReadingMessage(logger, DateTime.Now, null);
            }
        }

        public static void ErrorReceiving(this ILogger logger, Exception exception)
        {
            if (logger.IsEnabled(LogLevel.Error))
            {
                _errorReceiving(logger, DateTime.Now, exception);
            }
        }

        public static void EndReceive(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Trace))
            {
                _endReceive(logger, DateTime.Now, null);
            }
        }

        public static void SendingMessage(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                _sendingMessage(logger, DateTime.Now, null);
            }
        }

        public static void StoppingClient(this ILogger logger)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                _stoppingClient(logger, DateTime.Now, null);
            }
        }
    }
}
