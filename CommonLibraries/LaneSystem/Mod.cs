using System.Reflection;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;

namespace C2VM.CommonLibraries.LaneSystem;

public class Mod : IMod
{
    public static readonly string m_Id = typeof(Mod).Assembly.GetName().Name;

    public static readonly string m_InformationalVersion = ((AssemblyInformationalVersionAttribute) System.Attribute.GetCustomAttribute(Assembly.GetAssembly(typeof(Mod)), typeof(AssemblyInformationalVersionAttribute))).InformationalVersion;

    public static readonly ILog m_Log = LogManager.GetLogger($"{m_Id}.{nameof(Mod)}").SetShowsErrorsInUI(false);

    public void OnLoad(UpdateSystem updateSystem)
    {
        m_Log.Info($"Loading {m_Id} v{m_InformationalVersion}");

        if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
        {
            m_Log.Info($"Current mod asset at {asset.path}");
        }

        // updateSystem.World.GetOrCreateSystemManaged<Game.Net.LaneSystem>().Enabled = false;
        updateSystem.World.GetOrCreateSystemManaged<Game.Net.C2VMPatchedLaneSystem>().Enabled = false;

        // updateSystem.UpdateBefore<Game.Net.C2VMPatchedLaneSystem, Game.Net.LaneSystem>(Game.SystemUpdatePhase.Modification4);
    }

    public void OnDispose()
    {
        m_Log.Info(nameof(OnDispose));
    }
}