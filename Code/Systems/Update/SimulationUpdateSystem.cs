using Colossal.Entities;
using Game;
using Game.Simulation;
using Unity.Entities;

namespace TrafficLightManager.Code.Systems.Update;

public partial class SimulationUpdateSystem : GameSystemBase
{
    private SimulationSystem m_SimulationSystem;

    private UI.UISystem m_UISystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_SimulationSystem = World.GetOrCreateSystemManaged<SimulationSystem>();
        m_UISystem = World.GetOrCreateSystemManaged<UI.UISystem>();
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
