using Colossal.Entities;
using Game;
using Game.Simulation;
using Unity.Entities;

namespace TrafficLightManager.Code.Systems.Update;

public partial class SimulationUpdateSystem : GameSystemBase
{
    private Game.Simulation.SimulationSystem m_SimulationSystem;

    private TrafficLightManager.Code.Systems.UI.UISystem m_UISystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_SimulationSystem = World.GetOrCreateSystemManaged<Game.Simulation.SimulationSystem>();
        m_UISystem = World.GetOrCreateSystemManaged<TrafficLightManager.Code.Systems.UI.UISystem>();
    }

    protected override void OnUpdate()
    {
        bool hasUpdate = false;
        m_UISystem.ForEachTrafficLight(
            (e) =>
            {
                if (hasUpdate)
                {
                    return;
                }
                if (EntityManager.TryGetSharedComponent<UpdateFrame>(e, out var updateFrame))
                {
                    if (updateFrame.m_Index == SimulationUtils.GetUpdateFrameWithInterval(m_SimulationSystem.frameIndex, 4, 16))
                    {
                        hasUpdate = true;
                        return;
                    }
                }
            }
        );
        if (hasUpdate)
        {
            m_UISystem.SimulationUpdate();
        }
    }
}
