using IniParser;
using IniParser.Model;
using IniParser.Model.Configuration;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

namespace RichNote
{
    public sealed partial class SettingsPage : UserControl
    {
        // Initialization
        public static SettingsPage Instance { get; private set; }
        public event EventHandler? OkClicked;
        private IniParserConfiguration parserSettings = new IniParserConfiguration { CaseInsensitive = true };
        public IniData parsedSettings = null;
        private IniData defaultSettings = null;    

        public SettingsPage()
        {
            InitializeComponent();
            Instance = this;  
        }

        // Event handlers
        private void SettingHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleSwitch toggledSwitch && toggledSwitch.Tag != null)
            {
                string switchTag = toggledSwitch.Tag.ToString();

                switch (switchTag)
                {
                    case "1 1":
                        WriteSetting("Saving", "AutosaveOnClose", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 2":
                        WriteSetting("Saving", "AutoloadOnOpen", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 3":
                        WriteSetting("Saving", "OpenBlankDocOnAutoload", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 4":
                        WriteSetting("Saving", "DefaultEditor", toggledSwitch.IsOn == true ? "txt" : "rtf");
                        break;

                    case "2 1":
                        WriteSetting("Interface", "ShowStatusBar", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "2 2":
                        WriteSetting("Interface", "ShowSplashScreen", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    default:
                        break;
                }
            }
        }

        private void ButtonHandler(object sender, RoutedEventArgs e)
        {
            if (sender is Button clickedButton)
            {
                string clickedText = clickedButton.Content.ToString();

                switch (clickedText)
                {
                    case "Reset defaults":
                        CreateDefaultSettings(false);
                        break;
                    
                    case "Open user folder":
                        var LocalAppData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RichNote");
                        System.Diagnostics.Process.Start("explorer.exe", LocalAppData);
                        break;
                    
                    case "OK":
                        OkClicked?.Invoke(this, EventArgs.Empty);
                        break;

                    default:
                        break;
                }
            }
        }

        // Helper methods
        public static void CreateDefaultSettings(bool checkExistence)
        {
            var LocalSettingsData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RichNote", "config.ini");
            var p = new FileIniDataParser();
            var defaults = new IniData();
            defaults.Sections.AddSection("Saving");
            defaults["Saving"].AddKey("AutosaveOnClose", "true");
            defaults["Saving"].AddKey("AutoloadOnOpen", "true");
            defaults["Saving"].AddKey("OpenBlankDocOnAutoload", "false");
            defaults["Saving"].AddKey("DefaultEditor", "txt");
            defaults.Sections.AddSection("Interface");
            defaults["Interface"].AddKey("ShowStatusBar", "true");
            defaults["Interface"].AddKey("ShowSplashScreen", "false");

            //defaults.Sections.GetSectionData("Saving").Keys.GetKeyData("DefaultEditor").Comments.Add("\"txt\" or \"rtf\"");

            if ((checkExistence == true && !File.Exists(LocalSettingsData)) || (checkExistence == false))
            {
                p.WriteFile(LocalSettingsData, defaults);
            }

            Instance.defaultSettings = defaults;
            ParseSettings();
        }
        
        public static void ParseSettings()
        {
            var LocalSettingsData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RichNote", "config.ini");
            var p = new FileIniDataParser();
            var parsed = new IniData();

            parsed = p.ReadFile(LocalSettingsData);

            Instance.parsedSettings = parsed;
            ToggleSettingVars();
        }

        public static void ToggleSettingVars()
        {
            if (Instance.parsedSettings != null)
            {
                Instance.AutosaveOnClose.IsOn = Instance.parsedSettings["Saving"]["AutosaveOnClose"] == "true" ? true : false;
                Instance.AutoloadOnOpen.IsOn = Instance.parsedSettings["Saving"]["AutoloadOnOpen"] == "true" ? true : false;
                Instance.OpenBlankDocOnAutoload.IsOn = Instance.parsedSettings["Saving"]["OpenBlankDocOnAutoload"] == "true" ? true : false;
                Instance.IsDefaultTXT.IsOn = Instance.parsedSettings["Saving"]["DefaultEditor"] == "txt" ? true : false;
                Instance.ShowStatusBar.IsOn = Instance.parsedSettings["Interface"]["ShowStatusBar"] == "true" ? true : false;
                Instance.ShowSplashScreen.IsOn = Instance.parsedSettings["Interface"]["ShowSplashScreen"] == "true" ? true : false;
            }
        }

        private static void WriteSetting(string sectionName, string keyName, string value)
        {
            var LocalSettingsData = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RichNote", "config.ini");
            var p = new FileIniDataParser();
            var settingsToMod = Instance.parsedSettings;

            settingsToMod[sectionName][keyName] = value;

            p.WriteFile(LocalSettingsData, settingsToMod);
            Instance.parsedSettings = settingsToMod;
        }
    }
}
