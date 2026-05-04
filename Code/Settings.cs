using System.Reflection;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI.Widgets;
using Unity.Entities;

namespace TrafficLightManager.Code;

[FileLocation("ModsSettings/TrafficLightManager.Code/Settings")]
[SettingsUITabOrder(kTabGeneral, kTabKeyBindings)]
[SettingsUIGroupOrder(kGroupGeneral, kGroupDefault, kGroupDisplay, kGroupMainPanel, kGroupKeyBindingReset)]
[SettingsUIShowGroupName]
public class Settings : ModSetting
{
    public const string kTabGeneral = "TabGeneral";
    public const string kGroupGeneral = "GroupGeneral";
    public const string kGroupDefault = "GroupDefault";
    public const string kGroupDisplay = "GroupDisplay";

    public const string kTabKeyBindings = "TabKeyBindings";
    public const string kGroupMainPanel = "GroupMainPanel";
    public const string kKeyboardBindingMainPanelToggle = "KeyboardBindingMainPanelToggle";
    public const string kGroupKeyBindingReset = "GroupKeyBindingReset";

    public struct Values
    {
        public bool m_DefaultSplitPhasing;

        public bool m_DefaultAlwaysGreenKerbsideTurn;

        public bool m_DefaultExclusivePedestrian;

        public Values(Settings settings)
        {
            m_DefaultSplitPhasing = settings.m_DefaultSplitPhasing;
            m_DefaultAlwaysGreenKerbsideTurn = settings.m_DefaultAlwaysGreenKerbsideTurn;
            m_DefaultExclusivePedestrian = settings.m_DefaultExclusivePedestrian;
        }
    }

    [SettingsUISection(kTabGeneral, kGroupGeneral)]
    [SettingsUIDropdown(typeof(Settings), "GetLanguageValues")]
    public string m_LocaleOption
    {
        get { return m_Locale; }
        set
        {
            m_Locale = value;
            Colossal.Localization.LocalizationManager localizationManager = Game.SceneFlow.GameManager.instance.localizationManager;
            localizationManager.GetType().GetTypeInfo().GetDeclaredMethod("NotifyActiveDictionaryChanged").Invoke(localizationManager, null);
        }
    }
    public string m_Locale { get; private set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultSplitPhasing { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultAlwaysGreenKerbsideTurn { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultExclusivePedestrian { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    [SettingsUIButton]
    [SettingsUIConfirmation(null, null)]
    [SettingsUIDisableByCondition(typeof(Settings), "IsNotInGame")]
    public bool m_ForceNodeUpdate
    {
        get { return false; }
        set
        {
            EntityQuery entityQuery = Mod.m_World.EntityManager.CreateEntityQuery(ComponentType.ReadOnly<Game.Net.TrafficLights>());
            Mod.m_World.EntityManager.AddComponent<Game.Common.Updated>(entityQuery);
        }
    }

    [SettingsUISection(kTabGeneral, kGroupDisplay)]
    public bool m_DisplayCurrentPhase { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDisplay)]
    [SettingsUIDisableByCondition(typeof(Settings), "m_DisplayCurrentPhase", true)]
    public bool m_DisplayCurrentPhaseWhenToolDisabled { get; set; }

    [SettingsUIKeyboardBinding(BindingKeyboard.None, kKeyboardBindingMainPanelToggle)]
    [SettingsUISection(kTabKeyBindings, kGroupMainPanel)]
    public ProxyBinding m_MainPanelToggleKeyboardBinding { get; set; }

    [SettingsUISection(kTabKeyBindings, kGroupKeyBindingReset)]
    [SettingsUIButton]
    [SettingsUIConfirmation(null, null)]
    public bool m_ResetBindings
    {
        set { ResetKeyBindings(); }
    }

    public Settings(IMod mod)
        : base(mod)
    {
        SetDefaults();
        RegisterInOptionsUI();
        RegisterKeyBindings();
        AssetDatabase.global.LoadSettings(nameof(Settings), this);
    }

    public override void SetDefaults()
    {
        m_LocaleOption = "auto";
        m_DefaultSplitPhasing = false;
        m_DefaultAlwaysGreenKerbsideTurn = false;
        m_DefaultExclusivePedestrian = false;
        m_DisplayCurrentPhase = true;
        m_DisplayCurrentPhaseWhenToolDisabled = false;
    }

    public override void Apply()
    {
        base.Apply();
    }

    public static DropdownItem<string>[] GetLanguageValues()
    {
        DropdownItem<string>[] list =
        [
            new DropdownItem<string> { value = "auto", displayName = "Auto" },
            new DropdownItem<string> { value = "de-DE", displayName = "German" },
            new DropdownItem<string> { value = "en-US", displayName = "English" },
            new DropdownItem<string> { value = "es-ES", displayName = "Spanish" },
            new DropdownItem<string> { value = "fr-FR", displayName = "French" },
            new DropdownItem<string> { value = "it-IT", displayName = "Italian" },
            new DropdownItem<string> { value = "ja-JP", displayName = "Japanese" },
            new DropdownItem<string> { value = "ko-KR", displayName = "Korean" },
            new DropdownItem<string> { value = "nl-NL", displayName = "Dutch" },
            new DropdownItem<string> { value = "pl-PL", displayName = "Polish" },
            new DropdownItem<string> { value = "pt-BR", displayName = "Portuguese (Brazil)" },
            new DropdownItem<string> { value = "ru-RU", displayName = "Russian" },
            new DropdownItem<string> { value = "zh-HANS", displayName = "Chinese (Simplified)" },
            new DropdownItem<string> { value = "zh-HANT", displayName = "Chinese (Traditional)" },
        ];
        return list;
    }

    public bool IsNotInGame()
    {
        return GameManager.instance.gameMode != Game.GameMode.Game;
    }
}
