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
        m_UISystem.RedrawGizmo();
    }
}
