using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI.Widgets;
using TrafficLightManager.Code.Utils;
using Unity.Collections;
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

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultSplitPhasing { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultAlwaysGreenKerbsideTurn { get; set; }

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    public bool m_DefaultExclusivePedestrian { get; set; }

    [SettingsUIHidden]
    public List<CustomPhaseTemplate> m_CustomPhaseTemplates { get; set; } = [];

    [SettingsUISection(kTabGeneral, kGroupDefault)]
    [SettingsUIDropdown(typeof(Settings), "GetCustomPhaseTemplateValues")]
    public string m_DefaultCustomPhaseTemplateOption
    {
        get { return m_DefaultCustomPhaseTemplate.m_Name; }
        set
        {
            var templates = GetCustomPhaseTemplates();
            var templateIndex = templates.FindIndex(t => t.m_Name == value);
            if (templateIndex < 0)
            {
                Mod.m_Log.Warn($"Could not find custom phase template with name {value}");
                m_DefaultCustomPhaseTemplate = CustomPhaseTemplate.Default;
                return;
            }
            m_DefaultCustomPhaseTemplate = templates[templateIndex];
        }
    }

    public CustomPhaseTemplate m_DefaultCustomPhaseTemplate { get; private set; }

    public static DropdownItem<string>[] GetCustomPhaseTemplateValues()
    {
        List<DropdownItem<string>> list = new List<DropdownItem<string>>();
        var templates = Mod.m_Settings?.GetCustomPhaseTemplates();
        if (templates != null)
        {
            foreach (CustomPhaseTemplate template in templates)
            {
                list.Add(new DropdownItem<string> { value = template.m_Name, displayName = template.m_Name });
            }
        }
        return list.ToArray();
    }

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

    [SettingsUISection(kTabGeneral, kGroupDisplay)]
    public bool m_DisplayTrafficLightGroupNameWhenToolDisabled { get; set; }

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
        m_CustomPhaseTemplates = [];
        m_DefaultCustomPhaseTemplateOption = CustomPhaseTemplate.Default.m_Name;

        m_DisplayCurrentPhase = true;
        m_DisplayCurrentPhaseWhenToolDisabled = false;
        m_DisplayTrafficLightGroupNameWhenToolDisabled = false;
    }

    public override void Apply()
    {
        base.Apply();
        var uiSystem = Mod.m_World.GetOrCreateSystemManaged<Systems.UI.UISystem>();
        uiSystem.SettingUpdate();
    }

    public bool IsNotInGame()
    {
        return GameManager.instance.gameMode != Game.GameMode.Game;
    }

    public List<CustomPhaseTemplate> GetCustomPhaseTemplates()
    {
        var templates = new List<CustomPhaseTemplate>(m_CustomPhaseTemplates);
        templates.Add(CustomPhaseTemplate.Default);
        return templates;
    }

    public void UpdateCustomPhaseTemplate(CustomPhaseTemplate customPhaseTemplate)
    {
        if (customPhaseTemplate.m_Name == CustomPhaseTemplate.Default.m_Name)
        {
            Mod.m_Log.Warn($"Cannot update default custom phase template");
            return;
        }
        var index = m_CustomPhaseTemplates.FindIndex(t => t.m_Name == customPhaseTemplate.m_Name);
        if (index < 0)
        {
            m_CustomPhaseTemplates.Add(customPhaseTemplate);
        }
        else
        {
            m_CustomPhaseTemplates[index] = customPhaseTemplate;
        }
        Apply();
    }

    public void RemoveCustomPhaseTemplate(string name)
    {
        var index = m_CustomPhaseTemplates.FindIndex(t => t.m_Name == name);
        if (index < 0)
        {
            Mod.m_Log.Warn($"Could not find custom phase template with name {name} to remove");
            return;
        }
        m_CustomPhaseTemplates.RemoveAt(index);
        Apply();
    }
}
