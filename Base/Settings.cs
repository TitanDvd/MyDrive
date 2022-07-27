using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using IWshRuntimeLibrary;
using System.Windows;
using System.Windows.Media;
using MyDrive.Base.Types;

namespace MyDrive.Base
{
    public class GeneralSettings: ControlSettings
    {
        public bool StartAtOsBoot
        {
            get => StartAtOsBootFn("MyDrive.exe", Directory.GetCurrentDirectory(), _startAtBoot);
            set
            {
                _startAtBoot = value;
                StartAtOsBootFn("MyDrive.exe", Directory.GetCurrentDirectory(), _startAtBoot);
            }
        }
        public bool ShowOnTopClick { get; set; }
        public bool ShowOnTopMouseSlide { get; set; }
        public bool UpdateAtShown { get; set; }


        private bool _startAtBoot;

        private bool StartAtOsBootFn(string targetExeName, string workingDir, bool autostart = true)
        {
            var startUpFolderW10 = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
            var shortCutDest = $"{startUpFolderW10}\\{targetExeName}.lnk";
            if (autostart)
                CreateShortCut(shortCutDest, workingDir, targetExeName);
            else
            {
                try
                {
                    System.IO.File.Delete(shortCutDest);
                }
                catch { }
            }

            return autostart;
        }



        private void CreateShortCut(string placeToLnk, string workingdir, string exename)
        {
            WshShell shell = new WshShell();
            // Where to place the shortcut
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(placeToLnk);
            // Execution path example: D:\myprogram\run.exe
            shortcut.TargetPath = $"{workingdir}\\{exename}";
            shortcut.WorkingDirectory = workingdir;
            shortcut.Save();
        }
    }



    public class VisualSettings: ControlSettings
    {
        public Color FontColor { get; set; }
        public Color TraslucentColor { get; set; }
        public Color GoodSpaceBarColor { get; set; }
        public Color WarningSpaceBarColor { get; set; }
        public Color FullSpaceBarColor { get; set; }
    }



    public class EAI: ControlSettings
    {
        public List<EasyAccessItem> AccessItems { get; set; }
    }


    public abstract class ControlSettings { }


    public class ApplicationSettings
    {
        public GeneralSettings Generals { get; set; }
        public VisualSettings Visual { get; set; }
        public EAI EasyAccessItems { get; set; }
        private string settingsFolder => "AppSettings";


        public ApplicationSettings()
        {
            if (!Directory.Exists(settingsFolder))
                Directory.CreateDirectory(settingsFolder);

            Generals = LoadSettings<GeneralSettings>();
            Visual = LoadSettings<VisualSettings>();
            EasyAccessItems = LoadSettings<EAI>();



            if (Visual == null)
            {
                Visual = new VisualSettings
                {
                    FontColor = Color.FromRgb(255, 255, 255),
                    FullSpaceBarColor = (Color)ColorConverter.ConvertFromString("#FFFF3C3C"),
                    GoodSpaceBarColor = (Color)ColorConverter.ConvertFromString("#FF297DBC"),
                    TraslucentColor = (Color)ColorConverter.ConvertFromString("#96000000"),
                    WarningSpaceBarColor = (Color)ColorConverter.ConvertFromString("#FFFF8040")
                };
            }
            if (Generals == null)
            {
                Generals = new GeneralSettings
                {
                    ShowOnTopClick = false,
                    ShowOnTopMouseSlide = true,
                    StartAtOsBoot = false,
                    UpdateAtShown = true
                };
            }
            if (EasyAccessItems == null)
            {
                EasyAccessItems = new EAI
                {
                    AccessItems = new List<EasyAccessItem>
                    {
                        new EasyAccessItem
                        {
                            RootPath = @"C:\",
                            Tag = "Local Drive C"
                        }
                    }
                };
            }
        }



        public T LoadSettings<T>()
        {
            string settingsFileName = $"{typeof(T).Name}.json";
            string settingsPath = $"{settingsFolder}\\{settingsFileName}";

            if (System.IO.File.Exists(settingsPath))
            {
                string jsonStr = System.IO.File.ReadAllText(settingsPath);
                if(!string.IsNullOrEmpty(jsonStr))
                    return JsonConvert.DeserializeObject<T>(jsonStr);
            }
            else
                System.IO.File.WriteAllText(settingsPath, "");
            return default(T);
        }


        public void SaveSettings(ControlSettings settingsObj)
        {
            string settingsFileName = $"{settingsObj.GetType().Name}.json";
            string settingsPath = $"{settingsFolder}\\{settingsFileName}";

            if (System.IO.File.Exists(settingsPath))
            {
                var jsonStr = JsonConvert.SerializeObject(settingsObj);
                System.IO.File.WriteAllText(settingsPath, jsonStr);
            }
        }



        

    }


    public static class AppSettings
    {
        public static ApplicationSettings Options = new ApplicationSettings();
    }
}
