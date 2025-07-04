using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public interface IPokeBotRunner
{
    List<BotSource<PokeBotState>> Bots { get; }
    PokeTradeHubConfig Config { get; }

    bool RunOnce { get; }

    bool IsRunning { get; }

    void StartAll();

    void StopAll();

    void InitializeStart();

    void Add(PokeRoutineExecutorBase newbot);

    void Remove(IConsoleBotConfig state, bool callStop);

    BotSource<PokeBotState>? GetBot(PokeBotState state);

    PokeRoutineExecutorBase CreateBotFromConfig(PokeBotState cfg);

    bool SupportsRoutine(PokeRoutineType pokeRoutineType);

    event EventHandler BotStopped;
}

public abstract class PokeBotRunner<T> : BotRunner<PokeBotState>, IPokeBotRunner where T : PKM, new()
{
    // Explicit interface implementation forwarding to base class Bots property
    List<BotSource<PokeBotState>> IPokeBotRunner.Bots => Bots;

    public readonly PokeTradeHub<T> Hub;
    private readonly BotFactory<T> Factory;
    public event EventHandler BotStopped;


    public PokeTradeHubConfig Config => Hub.Config;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected PokeBotRunner(PokeTradeHub<T> hub, BotFactory<T> factory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Hub = hub;
        Factory = factory;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected PokeBotRunner(PokeTradeHubConfig config, BotFactory<T> factory)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        Factory = factory;
        Hub = new PokeTradeHub<T>(config);
    }

    protected virtual void AddIntegrations()
    { }

    public override void Add(RoutineExecutor<PokeBotState> bot)
    {
        base.Add(bot);
        if (bot is PokeRoutineExecutorBase b && b.Config.InitialRoutine.IsTradeBot())
            Hub.Bots.Add(b);
    }

    public override bool Remove(IConsoleBotConfig cfg, bool callStop)
    {
        var bot = GetBot(cfg)?.Bot;
        if (bot is PokeRoutineExecutorBase b && b.Config.InitialRoutine.IsTradeBot())
            Hub.Bots.Remove(b);
        return base.Remove(cfg, callStop);
    }

    public override void StartAll()
    {
        InitializeStart();

        if (!Hub.Config.SkipConsoleBotCreation)
            base.StartAll();
    }

    public override void InitializeStart()
    {
        if (RunOnce)
            return;

        AutoLegalityWrapper.EnsureInitialized(Hub.Config.Legality);

        AddIntegrations();
        AddTradeBotMonitors();

        base.InitializeStart();
    }

    public override void StopAll()
    {
        // Raise the BotStopped event
        BotStopped?.Invoke(this, EventArgs.Empty);

        base.StopAll();

        // bots currently don't de-register
        Thread.Sleep(100);

        int count = Hub.BotSync.Barrier.ParticipantCount;
        if (count != 0)
            Hub.BotSync.Barrier.RemoveParticipants(count);
    }

    public override void PauseAll()
    {
        if (!Hub.Config.SkipConsoleBotCreation)
            base.PauseAll();
    }

    public override void ResumeAll()
    {
        if (!Hub.Config.SkipConsoleBotCreation)
            base.ResumeAll();
    }

    private void AddTradeBotMonitors()
    {
        Task.Run(async () => await new QueueMonitor<T>(Hub).MonitorOpenQueue(CancellationToken.None).ConfigureAwait(false));

        var path = Hub.Config.Folder.DistributeFolder;
        if (!Directory.Exists(path))
            LogUtil.LogError("The distribution folder was not found. Please verify that it exists!", "Hub");

        var pool = Hub.Ledy.Pool;
        if (!pool.Reload(Hub.Config.Folder.DistributeFolder))
            LogUtil.LogError("Nothing to distribute for Empty Trade Queues!", "Hub");
    }

    public PokeRoutineExecutorBase CreateBotFromConfig(PokeBotState cfg) => Factory.CreateBot(Hub, cfg);

    public BotSource<PokeBotState>? GetBot(PokeBotState state) => base.GetBot(state);

    void IPokeBotRunner.Remove(IConsoleBotConfig state, bool callStop) => Remove(state, callStop);

    public void Add(PokeRoutineExecutorBase newbot) => Add((RoutineExecutor<PokeBotState>)newbot);

    public bool SupportsRoutine(PokeRoutineType t) => Factory.SupportsRoutine(t);
}
