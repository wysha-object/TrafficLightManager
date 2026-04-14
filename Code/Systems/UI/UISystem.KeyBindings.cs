using Game.Input;
using Game.UI;
using UnityEngine.InputSystem;

namespace TrafficLightManager.Code.Systems.UI;

public partial class UISystem : UISystemBase
{
    private ProxyAction m_MainPanelToggleKeyboardBinding;

    private void SetupKeyBindings()
    {
        if (Mod.m_Settings == null)
        {
            Mod.m_Log.Error($"Mod.m_Settings is null, key bindings will not work.");
            return;
        }
        m_MainPanelToggleKeyboardBinding = Mod.m_Settings.GetAction(Settings.kKeyboardBindingMainPanelToggle);
        m_MainPanelToggleKeyboardBinding.shouldBeEnabled = true;
        m_MainPanelToggleKeyboardBinding.onInteraction += ToggleToolState;
    }

    private void ToggleToolState(ProxyAction action, InputActionPhase phase)
    {
        if (Enabled && phase == InputActionPhase.Performed)
        {
            if ((ToolState)m_ToolStateBinding.value != ToolState.Disabled)
            {
                SetToolState(ToolState.Disabled);
            }
            else
            {
                SetToolState(ToolState.ChooseGroup);
            }
        }
    }
}
