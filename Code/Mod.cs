using System.Reflection;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using TrafficLightManager.Code.Systems.Update;
using Unity.Collections;
using Unity.Entities;

namespace TrafficLightManager.Code;

public class Mod : IMod
{
    public static readonly string m_Id = typeof(Mod).Assembly.GetName().Name;

    public static readonly string m_InformationalVersion = (
        (AssemblyInformationalVersionAttribute)System.Attribute.GetCustomAttribute(Assembly.GetAssembly(typeof(Mod)), typeof(AssemblyInformationalVersionAttribute))
    ).InformationalVersion;

    public static readonly ILog m_Log = LogManager.GetLogger($"{m_Id}.{nameof(Mod)}").SetShowsErrorsInUI(false);

    public static TrafficLightManager.Code.Settings m_Settings;

    public static World m_World;

    private static Game.Net.TrafficLightInitializationSystem m_TrafficLightInitializationSystem;

    private static Game.Simulation.TrafficLightSystem m_TrafficLightSystem;

    private static Systems.Initialization.PatchedTrafficLightInitializationSystem m_PatchedTrafficLightInitializationSystem;

    private static Systems.Simulation.PatchedTrafficLightSystem m_PatchedTrafficLightSystem;

    private static Systems.Simulation.TrafficLightGroupSystem m_TrafficLightGroupSystem;

    private static Systems.Update.TrafficLightGroupUpdateSystem m_TrafficLightGroupUpdateSystem;

    public void OnLoad(UpdateSystem updateSystem)
    {
        m_Log.Info($"Loading {m_Id} v{m_InformationalVersion}");

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
        {
            m_Log.Info($"Current mod asset at {asset.path}");
        }

        m_World = updateSystem.World;

        m_TrafficLightInitializationSystem = m_World.GetOrCreateSystemManaged<Game.Net.TrafficLightInitializationSystem>();
        m_TrafficLightSystem = m_World.GetOrCreateSystemManaged<Game.Simulation.TrafficLightSystem>();
        m_PatchedTrafficLightInitializationSystem = m_World.GetOrCreateSystemManaged<Systems.Initialization.PatchedTrafficLightInitializationSystem>();
        m_PatchedTrafficLightSystem = m_World.GetOrCreateSystemManaged<Systems.Simulation.PatchedTrafficLightSystem>();
        m_TrafficLightGroupSystem = m_World.GetOrCreateSystemManaged<Systems.Simulation.TrafficLightGroupSystem>();
        m_TrafficLightGroupUpdateSystem = m_World.GetOrCreateSystemManaged<Systems.Update.TrafficLightGroupUpdateSystem>();
        m_Settings = new Settings(this);

        SystemSetup(updateSystem);

        string netToolSystemToolID = m_World.GetOrCreateSystemManaged<Game.Tools.NetToolSystem>().toolID;
        Assert(netToolSystemToolID == "Net Tool", $"netToolSystemToolID: {netToolSystemToolID}");
    }

    public void OnDispose()
    {
        m_Log.Info(nameof(OnDispose));
    }

    public void SystemSetup(UpdateSystem updateSystem)
    {
        m_World.GetOrCreateSystemManaged<Game.Tools.NetToolSystem>(); // Ensure NetToolSystem is created before our tool

        var noneList = new NativeList<ComponentType>(1, Allocator.Temp);
        noneList.Add(ComponentType.ReadOnly<Components.CustomTrafficLights>());

        Utils.EntityQueryUtils.UpdateEntityQuery(m_TrafficLightInitializationSystem, "m_TrafficLightsQuery", noneList);
        Utils.EntityQueryUtils.UpdateEntityQuery(m_TrafficLightSystem, "m_TrafficLightQuery", noneList);

        updateSystem.UpdateBefore<Systems.Initialization.PatchedTrafficLightInitializationSystem, Game.Net.TrafficLightInitializationSystem>(SystemUpdatePhase.Modification4B);
        updateSystem.UpdateBefore<Systems.Simulation.PatchedTrafficLightSystem, Game.Simulation.TrafficLightSystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<Systems.Simulation.TrafficLightGroupSystem>(SystemUpdatePhase.GameSimulation);
        updateSystem.UpdateAt<Systems.UI.TooltipSystem>(SystemUpdatePhase.UITooltip);
        updateSystem.UpdateAt<Systems.UI.UISystem>(SystemUpdatePhase.UIUpdate);
        updateSystem.UpdateAt<Systems.Tool.ToolSystem>(SystemUpdatePhase.ToolUpdate);
        updateSystem.UpdateAt<ModificationUpdateSystem>(SystemUpdatePhase.ModificationEnd);
        updateSystem.UpdateAt<Systems.Update.TrafficLightGroupUpdateSystem>(SystemUpdatePhase.ModificationEnd);
        updateSystem.UpdateAfter<SimulationUpdateSystem>(SystemUpdatePhase.GameSimulation);

        m_TrafficLightInitializationSystem.Enabled = false;
        m_TrafficLightSystem.Enabled = false;
    }

    public static void Assert(
        bool condition,
        string message = "",
        bool showInUI = false,
        [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(condition))] string expression = ""
    )
    {
        if (condition == true)
        {
            return;
        }
        bool showsErrorsInUI = m_Log.showsErrorsInUI;
        m_Log.SetShowsErrorsInUI(showInUI);
        m_Log.Error($"Assertion failed!\n{message}\nExpression: {expression}");
        m_Log.SetShowsErrorsInUI(showsErrorsInUI);
    }

    public static string ReleaseChannel()
    {
#if STABLE
        return "Stable";
#elif BETA
        return "Beta";
#else
        return "UNKNOWN";
#endif
    }
}
