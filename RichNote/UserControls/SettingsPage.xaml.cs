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

namespace RichNote.UserControls
{
    public sealed partial class SettingsPage : UserControl
    {
        // Initialization
        public static SettingsPage Instance { get; private set; }
        public event EventHandler? OkClicked;
        public event EventHandler? StatusBarToggled;
        public bool EnableStatusBar => ShowStatusBar.IsOn;
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
                        WriteSetting("Document", "AutosaveOnClose", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 2":
                        WriteSetting("Document", "AutoloadOnOpen", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 3":
                        WriteSetting("Document", "OpenBlankDocOnAutoload", toggledSwitch.IsOn.ToString().ToLower());
                        break;

                    case "1 4":
                        WriteSetting("Document", "DefaultEditor", toggledSwitch.IsOn == true ? "txt" : "rtf");
                        break;

                    case "2 1":
                        WriteSetting("Interface", "ShowStatusBar", toggledSwitch.IsOn.ToString().ToLower());
                        StatusBarToggled?.Invoke(this, EventArgs.Empty);
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
            defaults.Sections.AddSection("Document");
            defaults["Document"].AddKey("AutosaveOnClose", "true");
            defaults["Document"].AddKey("AutoloadOnOpen", "true");
            defaults["Document"].AddKey("OpenBlankDocOnAutoload", "false");
            defaults["Document"].AddKey("DefaultEditor", "txt");
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

            if (parsed["Document"]["DefaultEditor"] != "txt" && parsed["Document"]["DefaultEditor"] != "rtf") {
                WriteSetting("Document", "DefaultEditor", "txt");
            }
            if (parsed["Interface"]["ShowStatusBar"] != "true") {
                Instance.StatusBarToggled?.Invoke(Instance, EventArgs.Empty);
            }

            ToggleSettingVars();
        }

        public static void ToggleSettingVars()
        {
            if (Instance.parsedSettings != null)
            {
                Instance.AutosaveOnClose.IsOn = Instance.parsedSettings["Document"]["AutosaveOnClose"] == "true" ? true : false;
                Instance.AutoloadOnOpen.IsOn = Instance.parsedSettings["Document"]["AutoloadOnOpen"] == "true" ? true : false;
                Instance.OpenBlankDocOnAutoload.IsOn = Instance.parsedSettings["Document"]["OpenBlankDocOnAutoload"] == "true" ? true : false;
                Instance.IsDefaultTXT.IsOn = Instance.parsedSettings["Document"]["DefaultEditor"] == "txt" ? true : false;
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
