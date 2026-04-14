using Game;
using Game.Common;
using Unity.Entities;

namespace TrafficLightManager.Code.Systems.Update;

public partial class ModificationUpdateSystem : GameSystemBase
{
    private TrafficLightManager.Code.Systems.UI.UISystem m_UISystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        m_UISystem = World.GetOrCreateSystemManaged<TrafficLightManager.Code.Systems.UI.UISystem>();
    }

    protected override void OnUpdate()
    {
        bool hasModification = false;
        m_UISystem.ForEachTrafficLight(
            (e) =>
            {
                if (m_UISystem.m_SelectedTrafficLightGroupEntity != Entity.Null && EntityManager.HasComponent<Updated>(e))
                {
                    hasModification = true;
                    m_UISystem.UpdateEdgeInfo(e);
                }
            }
        );
        if (hasModification)
        {
            m_UISystem.RedrawGizmo();
        }
    }
}
