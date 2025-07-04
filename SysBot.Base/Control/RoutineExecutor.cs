using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Base
{
    /// <summary>
    /// Commands a Bot to a perform a routine asynchronously.
    /// </summary>
    public abstract class RoutineExecutor<T> : IRoutineExecutor where T : class, IConsoleBotConfig
    {
        public readonly T Config;

        public readonly IConsoleConnectionAsync Connection;

        protected RoutineExecutor(IConsoleBotManaged<IConsoleConnection, IConsoleConnectionAsync> cfg)
        {
            Config = (T)cfg;
            Connection = cfg.CreateAsynchronous();
        }

        public string LastLogged { get; private set; } = "Offline";

        public DateTime LastTime { get; private set; } = DateTime.Now;

        public abstract string GetSummary();

        public abstract Task HardStop();

        public abstract Task InitialStartup(CancellationToken token);

        public void Log(string message)
        {
            Connection.Log(message);
            LastLogged = message;
            LastTime = DateTime.Now;
        }

        public abstract Task MainLoop(CancellationToken token);

        public abstract Task RebootAndStop(CancellationToken token);

        public async Task RebootAndStopAsync(CancellationToken token)
        {
            Connection.Connect();
            await InitialStartup(token).ConfigureAwait(false);
            await RebootAndStop(token).ConfigureAwait(false);
            Connection.Disconnect();
        }

        public void ReportStatus() => LastTime = DateTime.Now;

        /// <summary>
        /// Connects to the console, then runs the bot.
        /// </summary>
        /// <param name="token">Cancel this token to have the bot stop looping.</param>
        public async Task RunAsync(CancellationToken token)
        {
            Connection.Connect();
            Log("Initializing connection with console...");
            await InitialStartup(token).ConfigureAwait(false);
            await SetController(ControllerType.ProController, token);
            await MainLoop(token).ConfigureAwait(false);
            Connection.Disconnect();
        }

        public abstract Task SetController(ControllerType Controller, CancellationToken token);

        public abstract void SoftStop();
    }
}
