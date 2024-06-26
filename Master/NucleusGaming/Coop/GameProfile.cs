﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Forms.NucleusMessageBox;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    public class GameProfile
    {
        private List<UserScreen> screens;
        public List<UserScreen> Screens => screens;
        private List<PlayerInfo> deviceList;

        public Dictionary<string, object> Options => options;
        private Dictionary<string, object> options;
        /// <summary>
        /// Return a list of all players(connected devices)
        /// </summary>       
        public List<PlayerInfo> DevicesList => GetDevicesList();
        private static readonly IniFile ini = Globals.ini;

        public static GameProfile _GameProfile;

        public static GenericGameInfo Game;
        private static SetupScreenControl setupScreen = null;

        public static int TotalAssignedPlayers;//K&m player will count as one only if 2 devices(keyboard & mouse) has the same bounds.

        private static int totalProfilePlayers;//How many players the loaded profile counts.
        public static int TotalProfilePlayers => totalProfilePlayers;
     
        private static int profilesCount;//Used to check if we need to create a new profile 
        public static int ProfilesCount => profilesCount;

        private static int profileToSave;
        public static int CurrentProfileId => profileToSave;

        private static string modeText;// = "New Profile";
        public static string ModeText => modeText;

        public static string GameGUID;

        public static List<string> profilesPathList = new List<string>();

        public static IDictionary<string, string> AudioInstances = new Dictionary<string, string>();

        private static bool showError = false;

        public static bool Loaded => totalProfilePlayers > 0;

        private static int hWndInterval;
        public static int HWndInterval
        {
            get => hWndInterval;
            set => hWndInterval = value;
        }

        private static bool useSplitDiv;
        public static bool UseSplitDiv
        {
            get => useSplitDiv;
            set => useSplitDiv = value;
        }
        
        private static bool hideDesktopOnly;
        public static bool HideDesktopOnly
        {
            get => hideDesktopOnly;
            set => hideDesktopOnly = value;
        }
        private static int customLayout_Ver;
        public static int CustomLayout_Ver
        {
            get => customLayout_Ver;
            set => customLayout_Ver = value;
        }

        private static int customLayout_Hor;
        public static int CustomLayout_Hor
        {
            get => customLayout_Hor;
            set => customLayout_Hor = value;
        }

        private static int customLayout_Max;
        public static int CustomLayout_Max
        {
            get => customLayout_Max;
            set => customLayout_Max = value;
        }

        private static bool autoDesktopScaling;
        public static bool AutoDesktopScaling
        {
            get => autoDesktopScaling;
            set => autoDesktopScaling = value;
        }

        private static bool autoPlay;
        public static bool AutoPlay
        {
            get => autoPlay;
            set => autoPlay = value;
        }

        private static string splitDivColor = "Black";
        public static string SplitDivColor
        {
            get => splitDivColor;
            set => splitDivColor = value;
        }

        private static string network = "Automatic";
        public static string Network
        {
            get => network;
            set => network = value;
        }

        private static string notes;
        public static string Notes
        {
            get => notes;
            set => notes = value;
        }

        private static string title;
        public static string Title
        {
            get => title;
            set => title = value;
        }

        private static int pauseBetweenInstanceLaunch;
        public static int PauseBetweenInstanceLaunch
        {
            get => pauseBetweenInstanceLaunch;
            set => pauseBetweenInstanceLaunch = value;
        }

        private static bool useNicknames;
        public static bool UseNicknames
        {
            get => useNicknames;
            set => useNicknames = value;
        }

        private static bool audioDefaultSettings;
        public static bool AudioDefaultSettings
        {
            get => audioDefaultSettings;
            set => audioDefaultSettings = value;
        }

        private static bool audioCustomSettings;
        public static bool AudioCustomSettings
        {
            get => audioCustomSettings;
            set => audioCustomSettings = value;
        }

        private static bool cts_MuteAudioOnly;
        public static bool Cts_MuteAudioOnly
        {
            get => cts_MuteAudioOnly;
            set => cts_MuteAudioOnly = value;
        }

        private static bool cts_KeepAspectRatio;
        public static bool Cts_KeepAspectRatio
        {
            get => cts_KeepAspectRatio;
            set => cts_KeepAspectRatio = value;
        }

        private static bool cts_Unfocus;
        public static bool Cts_Unfocus
        {
            get => cts_Unfocus;
            set => cts_Unfocus = value;
        }

        private static bool cts_BringToFront;
        public static bool Cts_BringToFront
        {
            get => cts_BringToFront;
            set => cts_BringToFront = value;
        }

        private static int gamepadCount;
        public static int GamepadCount => gamepadCount;

        private static int keyboardCount;
        public static int KeyboardCount => keyboardCount;

        private static bool saved;
        public static bool Saved => saved;

        public static bool Ready;

        private static bool useXinputIndex;
        public static bool UseXinputIndex => useXinputIndex;

        //Avoid "Autoplay" to be applied right after setting the option in profile settings
        private static bool updating;
        public static bool Updating
        {
            get => updating;
            set => updating = value;
        }    

        public static List<RectangleF> AllScreens = new List<RectangleF>();
        public static List<ProfilePlayer> ProfilePlayersList = new List<ProfilePlayer>();
        public static List<PlayerInfo> loadedProfilePlayers = new List<PlayerInfo>();
        public static List<PlayerInfo> devicesToMerge = new List<PlayerInfo>();
        public static List<(Rectangle,RectangleF)> GhostBounds = new List<(Rectangle,RectangleF)>();

        private List<PlayerInfo> GetDevicesList()
        {
            lock (deviceList)
            {
                return deviceList;
            }
        }

        private void ListGameProfiles()
        {
            string path = GetGameProfilesPath();
            profilesPathList.Clear();
            profilesCount = 0;

            if (path != null)
            {
                profilesPathList = Directory.EnumerateFiles(path).OrderBy(s => s.Length).ToList();
                profilesCount = profilesPathList.Count();
            }
        }

        public void Reset()
        {
            bool profileDisabled = bool.Parse(ini.IniReadValue("Misc", "DisableGameProfiles"));

            ProfilePlayersList.Clear();
            AllScreens.Clear();
            GhostBounds.Clear();
            totalProfilePlayers = 0;

            showError = false;
            autoPlay = false;

            UpdateSharedSettings();

            notes = string.Empty;
            title = string.Empty;

            hWndInterval = 0;
            pauseBetweenInstanceLaunch = 0;

            profileToSave = 0;

            gamepadCount = 0;
            keyboardCount = 0;

            modeText = "New Profile";

            Ready = false;
            saved = false;

            if (!profileDisabled)
            {
                TotalAssignedPlayers = 0;

                RefreshSetupScreen();

                if (deviceList != null)//Switching profile
                {
                    foreach (PlayerInfo player in deviceList)
                    {
                        player.MonitorBounds = Rectangle.Empty;
                        player.EditBounds = player.SourceEditBounds;
                        player.ScreenIndex = -1;
                        player.PlayerID = -1;
                        player.SteamID = -1;
                        player.Nickname = null;

                    }

                    RefreshKeyboardAndMouse();
                }

                options = new Dictionary<string, object>();

                foreach (GameOption opt in Game.Options)
                {
                    options.Add(opt.Key, opt.Value);
                }
            
                setupScreen.gameProfilesList.ProfileBtn_CheckedChanged(new Label(), null);

                loadedProfilePlayers.Clear();
                devicesToMerge.Clear();

                setupScreen.profileSettings_Tooltip = CustomToolTips.SetToolTip(setupScreen.profileSettings_btn,
                    $"{Game.GameName} {ModeText.ToLower()} settings.", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

                ListGameProfiles();
            }

            if (BoundsFunctions.screens != null)
            {
                screens = BoundsFunctions.screens.ToList();
            }
        }

        public void InitializeDefault(GenericGameInfo game, SetupScreenControl pc)
        {
            _GameProfile = this;
            Game = game;
            setupScreen = pc;

            Reset();

            if (deviceList == null)
            {
                deviceList = new List<PlayerInfo>();
            }

            loadedProfilePlayers.Clear();
            devicesToMerge.Clear();

            TotalAssignedPlayers = 0;

            if (screens == null)
            {
                screens = new List<UserScreen>();
            }

            if (options == null)
            {
                options = new Dictionary<string, object>();

                foreach (GameOption opt in game.Options)
                {
                    options.Add(opt.Key, opt.Value);
                }
            }
        }

        public bool LoadGameProfile(int _profileToLoad)
        {
            string path = $"{Globals.GameProfilesFolder}\\{GameGUID}\\Profile[{_profileToLoad}].json";

            string jsonString = File.ReadAllText(path);

            JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);

            Reset();

            profileToSave = _profileToLoad;///to keep after reset()

            if (Jprofile == null)
            {
                Ready = false;
                return false;
            }

            JToken Joptions = Jprofile["Options"] as JToken;

            options = new Dictionary<string, object>();

            foreach (JProperty Jopt in Joptions)
            {
                options.Add((string)Jopt.Name, Jopt.Value.ToString());
            }

            if (hWndInterval == 0)
            {
                HWndInterval = (int)Jprofile["WindowsSetupTiming"]["Time"];
            }

            if (pauseBetweenInstanceLaunch == 0)
            {
                PauseBetweenInstanceLaunch = (int)Jprofile["PauseBetweenInstanceLaunch"]["Time"];
            }

            UseSplitDiv = (bool)Jprofile["UseSplitDiv"]["Enabled"];

            if (Jprofile["UseSplitDiv"]["HideOnly"] != null)
            {
                HideDesktopOnly = (bool)Jprofile["UseSplitDiv"]["HideOnly"];
            }

            SplitDivColor = (string)Jprofile["UseSplitDiv"]["Color"];
            AutoDesktopScaling = (bool)Jprofile["AutoDesktopScaling"]["Enabled"];
            UseNicknames = (bool)Jprofile["UseNicknames"]["Use"];
            Cts_KeepAspectRatio = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_KeepAspectRatio"];
            Cts_MuteAudioOnly = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_MuteAudioOnly"];
            Cts_Unfocus = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_Unfocus"];

            if (Jprofile["CutscenesModeSettings"]["Cutscenes_BringToFront"] != null)//such checks are there so testers/users can still use there existing profiles
                                                                                    //after new options implementation. Any new option must have that null check.
            {
                Cts_BringToFront = (bool)Jprofile["CutscenesModeSettings"]["Cutscenes_BringToFront"];
            }

            Network = (string)Jprofile["Network"]["Type"];
            CustomLayout_Ver = (int)Jprofile["CustomLayout"]["Ver"];
            CustomLayout_Hor = (int)Jprofile["CustomLayout"]["Hor"];
            CustomLayout_Max = (int)Jprofile["CustomLayout"]["Max"];
            AutoPlay = (bool)Jprofile["AutoPlay"]["Enabled"];
            Notes = (string)Jprofile["Notes"];
            Title = (string)Jprofile["Title"];

            JToken JplayersInfos = Jprofile["Data"] as JToken;

            for (int i = 0; i < JplayersInfos.Count(); i++)
            {
                ProfilePlayer player = new ProfilePlayer();

                player.PlayerID = (int)JplayersInfos[i]["PlayerID"];
                player.Nickname = (string)JplayersInfos[i]["Nickname"];
                player.SteamID = (long)JplayersInfos[i]["SteamID"];
                player.GamepadGuid = (Guid)JplayersInfos[i]["GamepadGuid"];
                player.OwnerType = (int)JplayersInfos[i]["Owner"]["Type"];
                player.DisplayIndex = (int)JplayersInfos[i]["Owner"]["DisplayIndex"];

                player.OwnerDisplay = new Rectangle(
                                               (int)JplayersInfos[i]["Owner"]["Display"]["X"],
                                               (int)JplayersInfos[i]["Owner"]["Display"]["Y"],
                                               (int)JplayersInfos[i]["Owner"]["Display"]["Width"],
                                               (int)JplayersInfos[i]["Owner"]["Display"]["Height"]);

                player.OwnerUIBounds = new RectangleF(
                                               (float)JplayersInfos[i]["Owner"]["UiBounds"]["X"],
                                               (float)JplayersInfos[i]["Owner"]["UiBounds"]["Y"],
                                               (float)JplayersInfos[i]["Owner"]["UiBounds"]["Width"],
                                               (float)JplayersInfos[i]["Owner"]["UiBounds"]["Height"]);


                player.MonitorBounds = new Rectangle(
                                               (int)JplayersInfos[i]["MonitorBounds"]["X"],
                                               (int)JplayersInfos[i]["MonitorBounds"]["Y"],
                                               (int)JplayersInfos[i]["MonitorBounds"]["Width"],
                                               (int)JplayersInfos[i]["MonitorBounds"]["Height"]);

                player.EditBounds = new RectangleF(
                                              (float)JplayersInfos[i]["EditBounds"]["X"],
                                              (float)JplayersInfos[i]["EditBounds"]["Y"],
                                              (float)JplayersInfos[i]["EditBounds"]["Width"],
                                              (float)JplayersInfos[i]["EditBounds"]["Height"]);


                player.IsDInput = (bool)JplayersInfos[i]["IsDInput"];
                player.IsXInput = (bool)JplayersInfos[i]["IsXInput"];

                player.IsKeyboardPlayer = (bool)JplayersInfos[i]["IsKeyboardPlayer"];
                player.IsRawMouse = (bool)JplayersInfos[i]["IsRawMouse"];

                string[] hidIds = new string[] { "", "" };
                for (int h = 0; h < JplayersInfos[i]["HIDDeviceID"].Count(); h++)
                {
                    hidIds.SetValue(JplayersInfos[i]["HIDDeviceID"][h].ToString(), h);
                }

                player.HIDDeviceIDs = hidIds;

                player.ScreenPriority = (int)JplayersInfos[i]["ScreenPriority"];
                player.ScreenIndex = (int)JplayersInfos[i]["ScreenIndex"];

                player.IdealProcessor = (string)JplayersInfos[i]["Processor"]["IdealProcessor"];
                player.Affinity = (string)JplayersInfos[i]["Processor"]["ProcessorAffinity"];
                player.PriorityClass = (string)JplayersInfos[i]["Processor"]["ProcessorPriorityClass"];

                if (player.IsXInput || player.IsDInput)
                {
                    gamepadCount++;
                }
                else
                {
                    keyboardCount++;
                }

                ProfilePlayersList.Add(player);
            }

            JToken JaudioSettings = Jprofile["AudioSettings"] as JToken;

            foreach (JProperty JaudioSetting in JaudioSettings)
            {
                if (JaudioSetting.Name.Contains("Custom"))
                {
                    AudioCustomSettings = (bool)JaudioSetting.Value;
                }
                else
                {
                    AudioDefaultSettings = (bool)JaudioSetting.Value;
                }
            }

            AudioInstances.Clear();

            JToken JAudioInstances = Jprofile["AudioInstances"] as JToken;
            foreach (JProperty JaudioDevice in JAudioInstances)
            {
                AudioInstances.Add((string)JaudioDevice.Name, (string)JaudioDevice.Value);
            }

            JToken JAllscreens = Jprofile["AllScreens"] as JToken;
            for (int s = 0; s < JAllscreens.Count(); s++)
            {
                AllScreens.Add(new RectangleF((float)JAllscreens[s]["X"], (float)JAllscreens[s]["Y"], (float)JAllscreens[s]["Width"], (float)JAllscreens[s]["Height"]));
            }

            totalProfilePlayers = JplayersInfos.Count();

            modeText = $"Profile n°{profileToSave}";

            setupScreen.profileSettings_Tooltip.SetToolTip(setupScreen.profileSettings_btn, $"{GameProfile.Game.GameName} {GameProfile.ModeText.ToLower()} settings.");

            Ready = true;

            GetGhostBounds();

            Globals.MainOSD.Show(1000, $"Game Profile N°{_profileToLoad} loaded");

            return true;
        }

        private static string GetGameProfilesPath()
        {
            string path = $"{Globals.GameProfilesFolder}\\{GameGUID}";
            if (!Directory.Exists(path))
            {
                return null;
            }

            return path;
        }

        private static void RefreshSetupScreen()
        {
            BoundsFunctions.screens = null;
            BoundsFunctions.UpdateScreens();

            for (int i = 0; i < BoundsFunctions.screens.Length; i++)
            {
                UserScreen s = BoundsFunctions.screens[i];
                s.PlayerOnScreen = 0;            
            }
        }

        public static void UpdateSharedSettings()
        {
            autoDesktopScaling = bool.Parse(ini.IniReadValue("Misc", "AutoDesktopScaling"));
            useNicknames = bool.Parse(ini.IniReadValue("Misc", "UseNicksInGame"));
            useSplitDiv = bool.Parse(ini.IniReadValue("CustomLayout", "SplitDiv"));
            hideDesktopOnly = bool.Parse(ini.IniReadValue("CustomLayout", "HideOnly"));
            customLayout_Ver = int.Parse(ini.IniReadValue("CustomLayout", "VerticalLines"));
            customLayout_Hor = int.Parse(ini.IniReadValue("CustomLayout", "HorizontalLines"));
            customLayout_Max = int.Parse(ini.IniReadValue("CustomLayout", "MaxPlayers"));
            splitDivColor = ini.IniReadValue("CustomLayout", "SplitDivColor");
            network = ini.IniReadValue("Misc", "Network");

            audioCustomSettings = int.Parse(ini.IniReadValue("Audio", "Custom")) == 1;
            audioDefaultSettings = audioCustomSettings == false;

            cts_MuteAudioOnly = bool.Parse(ini.IniReadValue("CustomLayout", "Cts_MuteAudioOnly"));
            cts_KeepAspectRatio = bool.Parse(ini.IniReadValue("CustomLayout", "Cts_KeepAspectRatio"));
            cts_Unfocus = bool.Parse(ini.IniReadValue("CustomLayout", "Cts_Unfocus"));

            if (ini.IniReadValue("CustomLayout", "Cts_BringToFront") != "")//such checks are there so testers/users can still use there existing profiles
                                                                           //after new options implementation. Any new option must have that null check.
            {
                cts_BringToFront = bool.Parse(ini.IniReadValue("CustomLayout", "Cts_BringToFront"));
            }
          
           useXinputIndex = bool.Parse(ini.IniReadValue("Dev", "UseXinputIndex"));
        }

        public static void UpdateGameProfile(GameProfile profile)
        {
            string path;

            bool profileDisabled = bool.Parse(Globals.ini.IniReadValue("Misc", "DisableGameProfiles"));

            if (profilesCount + 1 >= 21 || profileDisabled)
            {
                if (!profileDisabled)
                {
                    Globals.MainOSD.Show(2000, $"Limit Of 20 Profiles Has Been Reach Already");
                }

                return;
            }

            if (!GameProfile.Loaded || profilesCount == 0)
            {
                profilesCount++;//increase to set new profile name
                path = $"{Globals.GameProfilesFolder}\\{GameGUID}\\Profile[{profilesCount}].json";
            }
            else
            {
                path = $"{Globals.GameProfilesFolder}\\{GameGUID}\\Profile[{profileToSave}].json";
            }

            if (!Directory.Exists($"{Globals.GameProfilesFolder}\\{GameGUID}"))
            {
                Directory.CreateDirectory($"{Globals.GameProfilesFolder}\\{GameGUID}");
            }

            JObject options = new JObject();
            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                if (opt.Value.GetType() == typeof(System.Dynamic.ExpandoObject))//Only used for options with pictures so far
                {
                    JObject values = new JObject();
                    System.Dynamic.ExpandoObject _vals = (System.Dynamic.ExpandoObject)opt.Value;

                    foreach (var t in _vals)
                    {
                        values.Add(new JProperty(t.Key, t.Value));
                    }

                    options.Add(new JProperty(opt.Key.ToString(), values));
                }
                else
                {
                    options.Add(new JProperty(opt.Key.ToString(), opt.Value.ToString()));
                }
            }

            JObject JHWndInterval = new JObject();

            if (hWndInterval > 0)
            {
                JHWndInterval.Add(new JProperty("Time", hWndInterval.ToString()));
            }
            else
            {
                JHWndInterval.Add(new JProperty("Time", "0"));
            }

            JObject JPauseBetweenInstanceLaunch = new JObject();

            if (pauseBetweenInstanceLaunch > 0)
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", pauseBetweenInstanceLaunch.ToString()));
            }
            else
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", "0"));
            }

            JObject JCustomLayout = new JObject(new JProperty("Ver", customLayout_Ver),
                                                new JProperty("Hor", customLayout_Hor),
                                                new JProperty("Max", CustomLayout_Max));

            JObject JUseSplitDiv = new JObject(new JProperty("Enabled", useSplitDiv),
                                               new JProperty("HideOnly", hideDesktopOnly),
                                               new JProperty("Color", splitDivColor));

            JObject JAutoDesktopScaling = new JObject(new JProperty("Enabled", autoDesktopScaling));
            JObject JUseNicknames = new JObject(new JProperty("Use", useNicknames));
            JObject JNetwork = new JObject(new JProperty("Type", network));
            JObject JAutoPlay = new JObject(new JProperty("Enabled", autoPlay));
            JObject JCts_Settings = new JObject(new JProperty("Cutscenes_KeepAspectRatio", cts_KeepAspectRatio),
                                                new JProperty("Cutscenes_MuteAudioOnly", cts_MuteAudioOnly),
                                                new JProperty("Cutscenes_Unfocus", cts_Unfocus),
                                                new JProperty("Cutscenes_BringToFront", cts_BringToFront)); 

            JObject JAudioInstances = new JObject();

            foreach (KeyValuePair<string, string> JaudioDevice in AudioInstances)
            {
                JAudioInstances.Add(new JProperty(JaudioDevice.Key, JaudioDevice.Value));
            }

            JObject JAudioSettings = new JObject(new JProperty("CustomSettings", audioCustomSettings), new JProperty("DefaultSettings", audioDefaultSettings));

            List<JObject> playersInfos = new List<JObject>();//Players object

            int gamepadCount = 0;
            int keyboardCount = 0;

            for (int i = 0; i < ProfilePlayersList.Count(); i++)//build per player object
            {
                ProfilePlayer player = ProfilePlayersList[i];

                if (player.IsXInput || player.IsDInput)
                {
                    gamepadCount++;
                }
                else
                {
                    keyboardCount++;
                }

                JObject JOwner = new JObject(
                                      new JProperty("Type", player.OwnerType),

                                       new JProperty("UiBounds", new JObject(
                                                               new JProperty("X", player.OwnerUIBounds.X),
                                                               new JProperty("Y", player.OwnerUIBounds.Y),
                                                               new JProperty("Width", player.OwnerUIBounds.Width),
                                                               new JProperty("Height", player.OwnerUIBounds.Height))),

                                      new JProperty("DisplayIndex", player.DisplayIndex),
                                      new JProperty("Display", new JObject(
                                                               new JProperty("X", player.OwnerDisplay.X),
                                                               new JProperty("Y", player.OwnerDisplay.Y),
                                                               new JProperty("Width", player.OwnerDisplay.Width),
                                                               new JProperty("Height", player.OwnerDisplay.Height))));

                JObject JMonitorBounds = new JObject(
                                             new JProperty("X", player.MonitorBounds.X),
                                             new JProperty("Y", player.MonitorBounds.Y),
                                             new JProperty("Width", player.MonitorBounds.Width),
                                             new JProperty("Height", player.MonitorBounds.Height));

                JObject JEditBounds = new JObject(
                                          new JProperty("X", player.EditBounds.X),
                                          new JProperty("Y", player.EditBounds.Y),
                                          new JProperty("Width", player.EditBounds.Width),
                                          new JProperty("Height", player.EditBounds.Height));

                JObject JProcessor = new JObject
                {
                    new JProperty("IdealProcessor", player.IdealProcessor),
                    new JProperty("ProcessorAffinity", player.Affinity),
                    new JProperty("ProcessorPriorityClass", player.PriorityClass)
                };

                JObject JPData = new JObject(//build all individual player datas object
                                 new JProperty("PlayerID", player.PlayerID),
                                 new JProperty("Nickname", player.Nickname),
                                 new JProperty("SteamID", player.SteamID),
                                 new JProperty("GamepadGuid", player.GamepadGuid),
                                 new JProperty("IsDInput", player.IsDInput),
                                 new JProperty("IsXInput", player.IsXInput),
                                 new JProperty("Processor", JProcessor),
                                 new JProperty("IsKeyboardPlayer", player.IsKeyboardPlayer),
                                 new JProperty("IsRawMouse", player.IsRawMouse),
                                 new JProperty("HIDDeviceID", player.HIDDeviceIDs),
                                 new JProperty("ScreenPriority", player.ScreenPriority),
                                 new JProperty("ScreenIndex", player.ScreenIndex),
                                 new JProperty("EditBounds", JEditBounds),
                                 new JProperty("MonitorBounds", JMonitorBounds),
                                 new JProperty("Owner", JOwner)
                    );

                playersInfos.Add(JPData);
            }

            List<JObject> JScreens = new List<JObject>();

            for (int s = 0; s < AllScreens.Count(); s++)
            {
                JObject JScreen = new JObject(new JProperty("X", AllScreens[s].X),
                                              new JProperty("Y", AllScreens[s].Y),
                                              new JProperty("Width", AllScreens[s].Width),
                                              new JProperty("Height", AllScreens[s].Height)
                                              );

                JScreens.Add(JScreen);
            }

            JObject profileJson = new JObject//shared settings object
            (
               new JProperty("Title", Title),
               new JProperty("Notes", Notes),
               new JProperty("Player(s)", ProfilePlayersList.Count),
               new JProperty("Controller(s)", gamepadCount),
               new JProperty("Use XInput Index", useXinputIndex),
               new JProperty("K&M", keyboardCount),
               new JProperty("AutoPlay", JAutoPlay),
               new JProperty("Data", playersInfos),
               new JProperty("Options", options),
               new JProperty("UseNicknames", JUseNicknames),
               new JProperty("AutoDesktopScaling", JAutoDesktopScaling),
               new JProperty("UseSplitDiv", JUseSplitDiv),
               new JProperty("CustomLayout", JCustomLayout),
               new JProperty("WindowsSetupTiming", JHWndInterval),
               new JProperty("PauseBetweenInstanceLaunch", JPauseBetweenInstanceLaunch),
               new JProperty("Network", JNetwork),
               new JProperty("AudioSettings", JAudioSettings),
               new JProperty("AudioInstances", JAudioInstances),
               new JProperty("CutscenesModeSettings", JCts_Settings),
               new JProperty("AllScreens", JScreens)

            );

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(profileJson, Formatting.Indented);
                    writer.Write(json);
                    stream.Flush();
                }
            }

            modeText = $"Profile n°{profileToSave}";

            updating = true;

            Globals.MainOSD.Show(1600, $"Game Profile Updated");
        }

        public static void SaveGameProfile(GameProfile profile)
        {
            string path;
            bool profileDisabled = bool.Parse(Globals.ini.IniReadValue("Misc", "DisableGameProfiles"));

            if (profilesCount + 1 >= 21 || profileDisabled)
            {
                if (!profileDisabled)
                {
                    Globals.MainOSD.Show(2000, $"Limit Of 20 Profiles Has Been Reach Already");
                }

                saved = true;
                return;
            }

            if (profile.deviceList.Count != TotalProfilePlayers || !Loaded || profilesCount == 0)
            {
                profilesCount++;//increase to set new profile name
                path = $"{Globals.GameProfilesFolder}\\{GameGUID}\\Profile[{profilesCount}].json";

            }
            else
            {
                path = $"{Globals.GameProfilesFolder}\\{GameGUID}\\Profile[{profileToSave}].json";
            }

            if (!Directory.Exists(Path.Combine($"{Globals.GameProfilesFolder}\\{GameGUID}")))
            {
                Directory.CreateDirectory($"{Globals.GameProfilesFolder}\\{GameGUID}");
            }

            JObject options = new JObject();
            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                if (opt.Value.GetType() == typeof(System.Dynamic.ExpandoObject))//Only used for options with pictures so far
                {
                    JObject values = new JObject();
                    System.Dynamic.ExpandoObject _vals = (System.Dynamic.ExpandoObject)opt.Value;

                    foreach (var t in _vals)
                    {
                        values.Add(new JProperty(t.Key, t.Value));
                    }

                    options.Add(new JProperty(opt.Key.ToString(), values));
                }
                else
                {
                    options.Add(new JProperty(opt.Key.ToString(), opt.Value.ToString()));
                }
            }

            JObject JHWndInterval = new JObject();

            if (hWndInterval > 0)
            {
                JHWndInterval.Add(new JProperty("Time", hWndInterval.ToString()));
            }
            else
            {
                JHWndInterval.Add(new JProperty("Time", "0"));
            }

            JObject JPauseBetweenInstanceLaunch = new JObject();

            if (pauseBetweenInstanceLaunch > 0)
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", pauseBetweenInstanceLaunch.ToString()));
            }
            else
            {
                JPauseBetweenInstanceLaunch.Add(new JProperty("Time", "0"));
            }

            JObject JCustomLayout = new JObject(new JProperty("Ver", customLayout_Ver),
                                                new JProperty("Hor", customLayout_Hor),
                                                new JProperty("Max", CustomLayout_Max));

            JObject JUseSplitDiv = new JObject(new JProperty("Enabled", useSplitDiv),
                                               new JProperty("HideOnly", hideDesktopOnly),
                                               new JProperty("Color", splitDivColor));

            JObject JAutoDesktopScaling = new JObject(new JProperty("Enabled", autoDesktopScaling));
            JObject JUseNicknames = new JObject(new JProperty("Use", useNicknames));
            JObject JNetwork = new JObject(new JProperty("Type", network));
            JObject JAutoPlay = new JObject(new JProperty("Enabled", autoPlay));

            JObject JAudioInstances = new JObject();

            JObject JCts_Settings = new JObject(new JProperty("Cutscenes_KeepAspectRatio", cts_KeepAspectRatio),
                                                new JProperty("Cutscenes_MuteAudioOnly", cts_MuteAudioOnly),
                                                new JProperty("Cutscenes_Unfocus", cts_Unfocus),
                                                new JProperty("Cutscenes_BringToFront", cts_BringToFront)); 

            foreach (KeyValuePair<string, string> JaudioDevice in AudioInstances)
            {
                JAudioInstances.Add(new JProperty(JaudioDevice.Key, JaudioDevice.Value));
            }

            JObject JAudioSettings = new JObject(new JProperty("CustomSettings", audioCustomSettings), new JProperty("DefaultSettings", audioDefaultSettings));

            List<PlayerInfo> players = (List<PlayerInfo>)profile.DevicesList.OrderBy(c => c.PlayerID).ToList();//need to do this because sometimes it's reversed
            List<JObject> playersInfos = new List<JObject>();//Players object

            int gamepadCount = 0;
            int keyboardCount = 0;

            for (int i = 0; i < players.Count(); i++)//build per players object
            {
                if (players[i].IsXInput || players[i].IsDInput)
                {
                    gamepadCount++;
                }
                else
                {
                    keyboardCount++;
                }

                JObject JOwner = new JObject(
                                   new JProperty("Type", players[i].Owner.Type),

                                   new JProperty("UiBounds", new JObject(
                                                           new JProperty("X", players[i].Owner.UIBounds.X),
                                                           new JProperty("Y", players[i].Owner.UIBounds.Y),
                                                           new JProperty("Width", players[i].Owner.UIBounds.Width),
                                                           new JProperty("Height", players[i].Owner.UIBounds.Height))),
                                   new JProperty("DisplayIndex", players[i].Owner.DisplayIndex),
                                   new JProperty("Display", new JObject(
                                                           new JProperty("X", players[i].Owner.display.X),
                                                           new JProperty("Y", players[i].Owner.display.Y),
                                                           new JProperty("Width", players[i].Owner.display.Width),
                                                           new JProperty("Height", players[i].Owner.display.Height))));


                JObject JMonitorBounds = new JObject(
                                         new JProperty("X", useSplitDiv && !hideDesktopOnly ? players[i].MonitorBounds.Location.X - 1 : players[i].MonitorBounds.Location.X),
                                         new JProperty("Y", useSplitDiv && !hideDesktopOnly ? players[i].MonitorBounds.Location.Y - 1 : players[i].MonitorBounds.Location.Y),
                                         new JProperty("Width", useSplitDiv && !hideDesktopOnly ? players[i].MonitorBounds.Width + 2 : players[i].MonitorBounds.Width),
                                         new JProperty("Height", useSplitDiv && !hideDesktopOnly ? players[i].MonitorBounds.Height + 2 : players[i].MonitorBounds.Height));

                JObject JEditBounds = new JObject(
                                      new JProperty("X", players[i].EditBounds.X),
                                      new JProperty("Y", players[i].EditBounds.Y),
                                      new JProperty("Width", players[i].EditBounds.Width),
                                      new JProperty("Height", players[i].EditBounds.Height));

                JObject JProcessor = new JObject
                {
                    new JProperty("IdealProcessor", players[i].IdealProcessor),
                    new JProperty("ProcessorAffinity", players[i].Affinity),
                    new JProperty("ProcessorPriorityClass", players[i].PriorityClass)
                };

                JObject JPData = new JObject(//build all individual player datas object
                                 new JProperty("PlayerID", players[i].PlayerID),
                                 new JProperty("Nickname", players[i].Nickname),
                                 new JProperty("SteamID", players[i].SteamID),
                                 new JProperty("GamepadGuid", players[i].GamepadGuid),
                                 new JProperty("IsDInput", players[i].IsDInput),
                                 new JProperty("IsXInput", players[i].IsXInput),
                                 new JProperty("Processor", JProcessor),
                                 new JProperty("IsKeyboardPlayer", players[i].IsKeyboardPlayer),
                                 new JProperty("IsRawMouse", players[i].IsRawMouse),
                                 new JProperty("HIDDeviceID", players[i].HIDDeviceID),
                                 new JProperty("ScreenPriority", players[i].ScreenPriority),
                                 new JProperty("ScreenIndex", players[i].ScreenIndex),
                                 new JProperty("EditBounds", JEditBounds),
                                 new JProperty("MonitorBounds", JMonitorBounds),
                                 new JProperty("Owner", JOwner)
                                 );

                playersInfos.Add(JPData);
            }

            List<JObject> JScreens = new List<JObject>();

            var screens = BoundsFunctions.screens;

            for (int s = 0; s < BoundsFunctions.screens.Count(); s++)
            {
                UserScreen screen = screens[s];


                JObject JScreen = new JObject(new JProperty("X", screen.UIBounds.X),
                                              new JProperty("Y", screen.UIBounds.Y),
                                              new JProperty("Width", screen.UIBounds.Width),
                                              new JProperty("Height", screen.UIBounds.Height)
                                             );

                JScreens.Add(JScreen);
            }

            JObject profileJson = new JObject//shared settings object
            (
               new JProperty("Title", Title),
               new JProperty("Notes", Notes),
               new JProperty("Player(s)", profile.deviceList.Count),
               new JProperty("Controller(s)", gamepadCount),
               new JProperty("K&M", keyboardCount),
               new JProperty("Use XInput Index", useXinputIndex),
               new JProperty("AutoPlay", JAutoPlay),
               new JProperty("Data", playersInfos),
               new JProperty("Options", options),
               new JProperty("UseNicknames", JUseNicknames),
               new JProperty("AutoDesktopScaling", JAutoDesktopScaling),
               new JProperty("UseSplitDiv", JUseSplitDiv),
               new JProperty("CustomLayout", JCustomLayout),
               new JProperty("WindowsSetupTiming", JHWndInterval),
               new JProperty("PauseBetweenInstanceLaunch", JPauseBetweenInstanceLaunch),
               new JProperty("Network", JNetwork),
               new JProperty("AudioSettings", JAudioSettings),
               new JProperty("AudioInstances", JAudioInstances),
               new JProperty("CutscenesModeSettings", JCts_Settings),
               new JProperty("AllScreens", JScreens)

            );

            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(profileJson, Formatting.Indented);
                    writer.Write(json);
                    stream.Flush();
                }
            }

            saved = true;

            LogManager.Log("Game Profile Saved");

            Globals.MainOSD.Show(1600, $"Game Profile Saved");
        }

        public static void FindProfilePlayers(PlayerInfo player)
        {
            if (loadedProfilePlayers.Contains(player) || BoundsFunctions.dragging)
            {
                return;
            }

            if (showError)
            {
                _GameProfile.Reset();
                return;
            }

            if (Loaded)
            {
                if (TotalAssignedPlayers < 0 && !showError)
                {
                    showError = true;
                    NucleusMessageBox.Show("error", "Oops!\nSomething went wrong, profile has been unloaded.", false);
                    return;
                }
            }

            if (TotalAssignedPlayers < TotalProfilePlayers)
            {
                GroupKeyboardAndMouse();

                ProfilePlayer profilePlayer = FindProfilePlayerInput(player);

                if (profilePlayer == null)
                {
                    return;
                }
             
                //if the screen looked for is not present look for an other one
                var scr = FindScreenOrAlternative(profilePlayer);
               

                //if the profile requires more screens than availables
                if (ProfilePlayersList.Any(pp => pp.ScreenIndex != profilePlayer.ScreenIndex) && scr.Item2 != profilePlayer.ScreenIndex)
                {
                    _GameProfile.Reset();
                    Globals.MainOSD.Show(2000, $"Not Enough Active Screens");
                    scr.Item1.Type = UserScreenType.FullScreen;
                    return;
                }

                if (_GameProfile.DevicesList.All(lpp => lpp.MonitorBounds != TranslateBounds(profilePlayer, scr.Item1).Item1) && 
                    ProfilePlayersList.FindIndex(pp => pp == profilePlayer) == loadedProfilePlayers.Count)//avoid to add player in the same bounds                                                                                                                           // ProfilePlayersList.FindIndex(pp => pp == profilePlayer) == loadedProfilePlayers.Count)//make sure to insert player like saved in the game profile
                {
                    SetProfilePlayerDatas(player, profilePlayer, scr.Item1, scr.Item2);                  
                    loadedProfilePlayers.Add(player);

                    scr.Item1.PlayerOnScreen++;
                    TotalAssignedPlayers++;

                    setupScreen.Invalidate();

                    if (TotalAssignedPlayers == TotalProfilePlayers && Ready && AutoPlay && !Updating)
                    {
                        setupScreen.CanPlayUpdated(true, true);
                        Globals.PlayButton.PerformClick();
                        Ready = false;
                        return;
                    }
                }        
            }

            if (TotalAssignedPlayers == TotalProfilePlayers)
            {
                if (Ready && (!AutoPlay || Updating))
                {
                    setupScreen.CanPlayUpdated(true, true);
                    Updating = false;
                    return;
                }
            }
        }

        private void GetGhostBounds()
        {
            foreach (ProfilePlayer pp in ProfilePlayersList)
            {
                var scr = FindScreenOrAlternative(pp);
                var ghostBounds = TranslateBounds(pp, scr.Item1);

                GhostBounds.Add(ghostBounds);             
            }
        }

        private static void SetProfilePlayerDatas(PlayerInfo player, ProfilePlayer profilePlayer, UserScreen screen, int screenIndex)
        {
            player.Owner = screen;
            player.Owner.Type = screen.Type;

            player.ScreenPriority = screen.priority;
            player.DisplayIndex = screen.DisplayIndex;

            var translatedBounds = TranslateBounds(profilePlayer, screen);

            player.EditBounds = translatedBounds.Item2;

            player.MonitorBounds = translatedBounds.Item1;

            player.ScreenIndex = screenIndex;
            player.Nickname = profilePlayer.Nickname;
            player.PlayerID = profilePlayer.PlayerID;
            player.SteamID = profilePlayer.SteamID;
            player.IsInputUsed = true;
        }

        private static ProfilePlayer FindProfilePlayerInput(PlayerInfo player)
        {
            ProfilePlayer profilePlayer = null;

            var screens = _GameProfile.screens;

            bool skipGuid = false;

            //DInput && XInput (follow gamepad api indexes)
            if (player.IsController && useXinputIndex)
            {
                foreach (ProfilePlayer pp in ProfilePlayersList)
                {
                    if (loadedProfilePlayers.All(lp => lp.PlayerID != pp.PlayerID) && (pp.IsDInput || pp.IsXInput))
                    {
                        profilePlayer = pp;
                        skipGuid = true;
                        break;
                    }
                }
            }

            //DInput && XInput using GamepadGuid(do not follow gamepad api indexes)
            if (player.IsController && !skipGuid && !useXinputIndex)
            {
                profilePlayer = ProfilePlayersList.Where(pl => (pl.IsDInput || pl.IsXInput) && (pl.GamepadGuid == player.GamepadGuid)).FirstOrDefault();
            }

            //single k&m 
            if (player.GamepadGuid.ToString() == "10000000-1000-1000-1000-100000000000")
            {
                profilePlayer = ProfilePlayersList.Where(pl => pl.GamepadGuid.ToString() == "10000000-1000-1000-1000-100000000000").FirstOrDefault();
            }

            bool isKm = player.IsRawKeyboard && player.IsRawMouse;

            //Merged raw keyboard and raw mouse player
            if (isKm)
            {
                profilePlayer = ProfilePlayersList.Where(pl => isKm &&
                (pl.HIDDeviceIDs.Contains(player.HIDDeviceID[0]) && pl.HIDDeviceIDs.Contains(player.HIDDeviceID[1]))).FirstOrDefault();
            }

            for (int i = 0; i < screens.Count(); i++)
            {
                UserScreen anyScr = screens[i];

                if (ProfilePlayersList.Any(pp => pp.ScreenIndex == i))
                {
                    anyScr.Type = (UserScreenType)ProfilePlayersList.Where(pp => pp.ScreenIndex == i).FirstOrDefault().OwnerType;
                }
            }

            return profilePlayer;
        }

        private static void GroupKeyboardAndMouse()
        {
            for (int i = 0; i < ProfilePlayersList.Count(); i++)
            {
                List<PlayerInfo> groupedPlayers = new List<PlayerInfo>();

                ProfilePlayer kbPlayer = ProfilePlayersList[i];

                for (int pl = 0; pl < _GameProfile.DevicesList.Count; pl++)
                {
                    PlayerInfo p = _GameProfile.DevicesList[pl];

                    if (p.IsRawKeyboard || p.IsRawMouse)
                    {
                        if (kbPlayer.HIDDeviceIDs.Any(hid => hid == p.HIDDeviceID[0]))
                        {
                            groupedPlayers.Add(p);
                        }
                    }

                    if (groupedPlayers.Count == 2)
                    {
                        var firstInGroup = groupedPlayers.First();
                        var secondInGroup = groupedPlayers.Last();

                        firstInGroup.IsRawKeyboard = groupedPlayers.Count(x => x.IsRawKeyboard) > 0;
                        firstInGroup.IsRawMouse = groupedPlayers.Count(x => x.IsRawMouse) > 0;

                        if (firstInGroup.IsRawKeyboard) firstInGroup.RawKeyboardDeviceHandle = groupedPlayers.First(x => x.RawKeyboardDeviceHandle != (IntPtr)(-1)).RawKeyboardDeviceHandle;
                        if (firstInGroup.IsRawMouse) firstInGroup.RawMouseDeviceHandle = groupedPlayers.First(x => x.RawMouseDeviceHandle != (IntPtr)(-1)).RawMouseDeviceHandle;

                        firstInGroup.HIDDeviceID = new string[2] { firstInGroup.HIDDeviceID[0], secondInGroup.HIDDeviceID[0] };

                        _GameProfile.DevicesList.Remove(secondInGroup);
                        p = firstInGroup;
                        p.IsInputUsed = true;///needed else device is invisible and can't be moved

                        break;
                    }
                }
            }
        }

        private void RefreshKeyboardAndMouse()
        {
            deviceList.RemoveAll(p => p.IsRawMouse || p.IsRawKeyboard);

            if (Game.SupportsMultipleKeyboardsAndMice)///Raw mice/keyboards
            {
                deviceList.AddRange(RawInputManager.GetDeviceInputInfos());
            }

            for (int i = 0; i < deviceList.Count(); i++)
            {
                deviceList[i].EditBounds = BoundsFunctions.GetDefaultBounds(i);
            }
        }

        internal static (UserScreen, int) FindScreenOrAlternative(ProfilePlayer profilePlayer)
        {
            UserScreen screen = BoundsFunctions.screens.ElementAtOrDefault(profilePlayer.ScreenIndex);
            int ogIndex = profilePlayer.ScreenIndex;

            while (screen == null)
            {
                --ogIndex;
                screen = BoundsFunctions.screens.ElementAtOrDefault(ogIndex);
            }

            screen.Type = (UserScreenType)profilePlayer.OwnerType;

            BoundsFunctions.GetScreenDivisionBounds(screen);
            return (screen, ogIndex);
        }

        internal static (Rectangle, RectangleF) TranslateBounds(ProfilePlayer profilePlayer, UserScreen screen)
        {
            RectangleF ogScrUiBounds = GameProfile.AllScreens[profilePlayer.ScreenIndex];
            RectangleF ogEditBounds = profilePlayer.EditBounds;

            Vector2 ogScruiLoc = new Vector2(ogScrUiBounds.X, ogScrUiBounds.Y);//original screen ui location
            Vector2 ogpEb = new Vector2(ogEditBounds.X, ogEditBounds.Y);//original on ui screen player location(editbounds)
            Vector2 ogOnUIScrLoc = Vector2.Subtract(ogpEb, ogScruiLoc);//relative og ui player loc on og player ui screen

            float ratioEW = (float)ogScrUiBounds.Width / (float)screen.UIBounds.Width;
            float ratioEH = (float)ogScrUiBounds.Height / (float)screen.UIBounds.Height;

            RectangleF translatedEditBounds = new RectangleF(screen.UIBounds.X + (ogOnUIScrLoc.X / ratioEW), screen.UIBounds.Y + (ogOnUIScrLoc.Y / ratioEH), ogEditBounds.Width / ratioEW, ogEditBounds.Height / ratioEH);

            ///## Re-calcul & scale player monitor bounds if needed ##///
            Rectangle ogScr = profilePlayer.OwnerDisplay;
            Rectangle ogMb = profilePlayer.MonitorBounds;

            Vector2 ogscr = new Vector2(ogScr.X, ogScr.Y);//original player screen location
            Vector2 ogPMb = new Vector2(ogMb.X, ogMb.Y);//original on screen player location(monitorBounds)
            Vector2 VogOnScrLoc = Vector2.Subtract(ogPMb, ogscr);//relative og player loc on og player screen

            float ratioMW = (float)ogScr.Width / (float)screen.MonitorBounds.Width;
            float ratioMH = (float)ogScr.Height / (float)screen.MonitorBounds.Height;

            Rectangle translatedMonitorBounds = new Rectangle(screen.MonitorBounds.X + (Convert.ToInt32(VogOnScrLoc.X / ratioMW)), screen.MonitorBounds.Y + (Convert.ToInt32(VogOnScrLoc.Y / ratioMH)), Convert.ToInt32(ogMb.Width / ratioMW), Convert.ToInt32(ogMb.Height / ratioMH));

            return (translatedMonitorBounds, translatedEditBounds);
        }

        public static GameProfile CleanClone(GameProfile profile)
        {
            loadedProfilePlayers.AddRange(devicesToMerge);

            GameProfile nprof = new GameProfile
            {
                deviceList = loadedProfilePlayers,
                screens = profile.screens,
            };

            //Clear profile screens subBounds
            nprof.screens.AsParallel().ForAll(i => i.SubScreensBounds.Clear());

            foreach (PlayerInfo player in nprof.deviceList)
            {
                if (UseSplitDiv && !HideDesktopOnly)
                {
                    player.MonitorBounds = new Rectangle(player.MonitorBounds.X + 1, player.MonitorBounds.Y + 1, player.MonitorBounds.Width - 2, player.MonitorBounds.Height - 2);
                }

                //Insert only players bounds in screens sub bounds
                foreach (UserScreen screen in nprof.screens)
                {
                    if (screen.Index == player.ScreenIndex)
                    {
                        if (!screen.SubScreensBounds.Keys.Contains(player.MonitorBounds))
                        {
                            screen.SubScreensBounds.Add(player.MonitorBounds, player.EditBounds);
                        }
                    }
                }
            }

            //Remove any screens not containing players 
            nprof.Screens.RemoveAll(s => s.SubScreensBounds.Count() == 0);

            Dictionary<string, object> noptions = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> opt in profile.Options)
            {
                noptions.Add(opt.Key, opt.Value);
            }

            nprof.options = noptions;

            return nprof;
        }

        public static PlayerInfo UpdateProfilePlayerNickAndSID(PlayerInfo player)
        {
            var secondInBounds = loadedProfilePlayers.Where(pl => pl.EditBounds == player.EditBounds && pl != player && pl.ScreenIndex != -1).FirstOrDefault();

            if (loadedProfilePlayers.Contains(player) || secondInBounds != null)
            {
                PlayerInfo plToUpdate = secondInBounds ?? player;

                int playerIndex = loadedProfilePlayers.FindIndex(pl => pl == plToUpdate);
                bool getNameFromProfile = ProfilePlayersList.Count() > 0 && ProfilePlayersList.Count() >= playerIndex;

                string nickname = getNameFromProfile ? ProfilePlayersList[playerIndex].Nickname :
                                  Globals.ini.IniReadValue("ControllerMapping", "Player_" + (playerIndex + 1));

                player.Nickname = nickname;

                string steamID;

                if (getNameFromProfile)
                {
                    steamID = ProfilePlayersList[playerIndex].SteamID.ToString();
                }
                else if (Globals.ini.IniReadValue("SteamIDs", "Player_" + (playerIndex + 1)) != "")
                {
                    steamID = Globals.ini.IniReadValue("SteamIDs", "Player_" + (playerIndex + 1));
                }
                else
                {
                    steamID = "-1";
                }

                if (steamID != "")
                {
                    player.SteamID = long.Parse(steamID);
                }

                return plToUpdate;
            }
            return null;
        }

    }

}
