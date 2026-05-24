using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace TrafficLightManager.Code.Utils;

public class LocalisationUtils
{
    public static readonly string m_DefaultLocale = "en-US";

    public static readonly string[] m_SupportedLocales =
    [
        "de-DE",
        "en-US",
        "es-ES",
        "fr-FR",
        "it-IT",
        "ja-JP",
        "ko-KR",
        "nl-NL",
        "pl-PL",
        "pt-BR",
        "ru-RU",
        "zh-HANS",
        "zh-HANT",
        "zh-HK",
        "zh-TW",
    ];

    public static readonly Dictionary<string, string[]> m_SupportedCultures = new() { { "en-US", ["nl-NL"] }, { "zh-HANT", ["zh-HK", "zh-TW"] } };

    public string m_Locale { get; private set; }

    private Dictionary<string, string> m_Dictionary = new Dictionary<string, string>();

    private static Dictionary<string, string> m_ActiveDictionary;

    public LocalisationUtils(string locale)
    {
        SetLocale(locale);
    }

    public void SetLocale(string locale)
    {
        m_Locale = locale;
        if (!m_SupportedLocales.Contains(locale))
        {
            m_Locale = m_DefaultLocale;
        }
        string resourceName = "TrafficLightManager.Code.Resources.Localization." + m_Locale + ".json";
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            Mod.m_Log.Error($"{resourceName} does not exist.");
            return;
        }
        using StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        string jsonString = reader.ReadToEnd();
        m_Dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonString);
    }

    public static string GetAutoLocale(string locale, string culture)
    {
        if (m_SupportedCultures.ContainsKey(locale) && m_SupportedCultures[locale].Contains(culture))
        {
            return culture;
        }
        if (m_SupportedLocales.Contains(locale))
        {
            return locale;
        }
        return m_DefaultLocale;
    }

    public void UpdateActiveDictionary()
    {
        m_ActiveDictionary = m_Dictionary;
    }

    public static Dictionary<string, string> GetActiveDictionary()
    {
        return m_ActiveDictionary;
    }

    public string GetString(string key)
    {
        if (m_Dictionary.ContainsKey(key))
        {
            return m_Dictionary[key];
        }
        return key;
    }

    public void AddToDictionary(Colossal.Localization.LocalizationDictionary dictionary)
    {
        if (Mod.m_Settings == null)
        {
            return;
        }
        dictionary.Add(Mod.m_Settings.GetSettingsLocaleID(), "Traffic Light Manager");

        dictionary.Add(Mod.m_Settings.GetOptionTabLocaleID(Settings.kTabGeneral), this.GetString("General"));
        dictionary.Add(Mod.m_Settings.GetOptionGroupLocaleID(Settings.kGroupGeneral), this.GetString("General"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_LocaleOption"), this.GetString("LocaleLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_LocaleOption"), this.GetString("LocaleDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionGroupLocaleID(Settings.kGroupDefault), this.GetString("Default"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_DefaultSplitPhasing"), this.GetString("DefaultSplitPhasingLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DefaultSplitPhasing"), this.GetString("DefaultSplitPhasingDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_DefaultAlwaysGreenKerbsideTurn"), this.GetString("DefaultAlwaysGreenKerbsideTurnLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DefaultAlwaysGreenKerbsideTurn"), this.GetString("DefaultAlwaysGreenKerbsideTurnDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_DefaultExclusivePedestrian"), this.GetString("DefaultExclusivePedestrianLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DefaultExclusivePedestrian"), this.GetString("DefaultExclusivePedestrianDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_DefaultCustomPhaseTemplateOption"), this.GetString("Settings.Default.DefaultCustomPhaseTemplate.Label"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DefaultCustomPhaseTemplateOption"), this.GetString("Settings.Default.DefaultCustomPhaseTemplate.Desc"));
        dictionary.Add(
            Mod.m_Settings.GetOptionLabelLocaleID("m_DisplayTrafficLightGroupNameWhenToolDisabled"),
            this.GetString("Settings.Default.DisplayTrafficLightGroupNameWhenToolDisabled.Label")
        );
        dictionary.Add(
            Mod.m_Settings.GetOptionDescLocaleID("m_DisplayTrafficLightGroupNameWhenToolDisabled"),
            this.GetString("Settings.Default.DisplayTrafficLightGroupNameWhenToolDisabled.Desc")
        );
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_ForceNodeUpdate"), this.GetString("ForceAllNodesUpdateLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_ForceNodeUpdate"), this.GetString("ForceAllNodesUpdateDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionWarningLocaleID("m_ForceNodeUpdate"), this.GetString("ForceAllNodesUpdateWarning"));
        dictionary.Add(Mod.m_Settings.GetOptionGroupLocaleID(Settings.kGroupDisplay), this.GetString("Settings.Display"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_DisplayCurrentPhase"), this.GetString("Settings.Display.DisplayCurrentPhase.Label"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DisplayCurrentPhase"), this.GetString("Settings.Display.DisplayCurrentPhase.Desc"));
        dictionary.Add(
            Mod.m_Settings.GetOptionLabelLocaleID("m_DisplayCurrentPhaseWhenToolDisabled"),
            this.GetString("Settings.Display.DisplayCurrentPhaseWhenToolDisabled.Label")
        );
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_DisplayCurrentPhaseWhenToolDisabled"), this.GetString("Settings.Display.DisplayCurrentPhaseWhenToolDisabled.Desc"));

        dictionary.Add(Mod.m_Settings.GetOptionTabLocaleID(Settings.kTabKeyBindings), this.GetString("KeyBindings"));
        dictionary.Add(Mod.m_Settings.GetOptionGroupLocaleID(Settings.kGroupMainPanel), this.GetString("MainPanel"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_MainPanelToggleKeyboardBinding"), this.GetString("MainPanelToggleKeyboardBindingLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_MainPanelToggleKeyboardBinding"), this.GetString("MainPanelToggleKeyboardBindingDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionGroupLocaleID(Settings.kGroupKeyBindingReset), this.GetString("Reset"));
        dictionary.Add(Mod.m_Settings.GetOptionLabelLocaleID("m_ResetBindings"), this.GetString("ResetBindingsLabel"));
        dictionary.Add(Mod.m_Settings.GetOptionDescLocaleID("m_ResetBindings"), this.GetString("ResetBindingsDesc"));
        dictionary.Add(Mod.m_Settings.GetOptionWarningLocaleID("m_ResetBindings"), this.GetString("ResetBindingsDesc"));
    }
}
