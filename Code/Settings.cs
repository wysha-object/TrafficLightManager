using System.Collections.Generic;
using System.Reflection;
using Colossal.IO.AssetDatabase;
using Game.Input;
using Game.Modding;
using Game.SceneFlow;
using Game.Settings;
using Game.UI.Widgets;
using TrafficLightManager.Code.Utils;
using Unity.Entities;

namespace TrafficLightManager.Code;

[FileLocation($"ModsSettings/{nameof(TrafficLightManager)}/{nameof(TrafficLightManager)}")]
[SettingsUITabOrder(kTabGeneral, kTabKeyBindings)]
[SettingsUIGroupOrder(kGroupGeneral, kGroupDefault, kGroupDisplay, kGroupVersion, kGroupMainPanel, kGroupKeyBindingReset)]
[SettingsUIShowGroupName]
public class Settings : ModSetting
{
    public const string kTabGeneral = "TabGeneral";
    public const string kGroupGeneral = "GroupGeneral";
    public const string kGroupDefault = "GroupDefault";
    public const string kGroupDisplay = "GroupDisplay";
    public const string kGroupVersion = "GroupVersion";

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

    [SettingsUIHidden]
    public Dictionary<string, string> m_Storage;

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

    public DropdownItem<string>[] GetCustomPhaseTemplateValues()
    {
        List<DropdownItem<string>> list = new List<DropdownItem<string>>();
        var templates = GetCustomPhaseTemplates();
        foreach (CustomPhaseTemplate template in templates)
        {
            list.Add(new DropdownItem<string> { value = template.m_Name, displayName = template.m_Name });
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

    [SettingsUISection(kTabGeneral, kGroupVersion)]
    public string m_ReleaseChannel => Mod.ReleaseChannel();

    [SettingsUISection(kTabGeneral, kGroupVersion)]
    public string m_Version => Mod.m_InformationalVersion;

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
        onSettingsApplied += (Setting setting) =>
        {
            Verify();
            RegisterInOptionsUI();
            RegisterKeyBindings();
        };
        SetDefaults();
        AssetDatabase.global.LoadSettings(nameof(TrafficLightManager), this);
        Apply();
    }

    public override void SetDefaults()
    {
        m_Storage = new Dictionary<string, string>();

        m_DefaultSplitPhasing = false;
        m_DefaultAlwaysGreenKerbsideTurn = false;
        m_DefaultExclusivePedestrian = false;
        m_CustomPhaseTemplates = [];
        m_DefaultCustomPhaseTemplateOption = CustomPhaseTemplate.Default.m_Name;

        m_DisplayCurrentPhase = true;
        m_DisplayCurrentPhaseWhenToolDisabled = false;
        m_DisplayTrafficLightGroupNameWhenToolDisabled = false;
    }

    public void Verify()
    {
        if (m_CustomPhaseTemplates == null)
        {
            m_CustomPhaseTemplates = [];
        }
        if (m_Storage == null)
        {
            m_Storage = new Dictionary<string, string>();
        }
    }

    public bool IsNotInGame()
    {
        return GameManager.instance.gameMode != Game.GameMode.Game;
    }

    public List<CustomPhaseTemplate> GetCustomPhaseTemplates()
    {
        var templates = new List<CustomPhaseTemplate>(m_CustomPhaseTemplates) { CustomPhaseTemplate.Default };
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
        if (name == m_DefaultCustomPhaseTemplate.m_Name)
        {
            m_DefaultCustomPhaseTemplate = CustomPhaseTemplate.Default;
        }
        Apply();
    }

    public string GetStorage(string key)
    {
        if (m_Storage.TryGetValue(key, out string value))
        {
            return value;
        }
        return null;
    }

    public void UpdateStorage(string key, string value)
    {
        m_Storage[key] = value;
    }
}
