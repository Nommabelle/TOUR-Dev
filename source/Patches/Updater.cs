using HarmonyLib;
using UnityEngine;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Twitch;
using Reactor;

namespace TownOfUs {
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public class ModUpdaterButton {
        private static void Prefix(MainMenuManager __instance) {
            //Check if there's an update
            ModUpdater.LaunchUpdater();
            if (!ModUpdater.hasUpdate) return;

            //If there's an update, create and show the update button
            var template = GameObject.Find("ExitGameButton");
            if (template == null) return;

            var button = UnityEngine.Object.Instantiate(template, null);
            button.transform.localPosition = new Vector3(button.transform.localPosition.x, button.transform.localPosition.y + 0.6f, button.transform.localPosition.z);

            PassiveButton passiveButton = button.GetComponent<PassiveButton>();
            SpriteRenderer buttonSprite = button.GetComponent<SpriteRenderer>();
            passiveButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();

            //Add onClick event to run the update on button click
            passiveButton.OnClick.AddListener((Action) (() =>
            {
                ModUpdater.ExecuteUpdate();
                button.SetActive(false);
            }));
            
            //Set button text
            var text = button.transform.GetChild(0).GetComponent<TMPro.TMP_Text>();
            __instance.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) => {
                text.SetText("<color=#FFFF00>Update\nTown Of Us</color>");
            })));

            //Set popup stuff
            TwitchManager man = DestroyableSingleton<TwitchManager>.Instance;
            ModUpdater.InfoPopup = UnityEngine.Object.Instantiate<GenericPopup>(man.TwitchPopup);
            ModUpdater.InfoPopup.TextAreaTMP.fontSize *= 0.7f;
            ModUpdater.InfoPopup.TextAreaTMP.enableAutoSizing = false;

        }
    }

    public class ModUpdater { 
        public static bool running = false;
        public static bool hasUpdate = false;
        public static string updateURI = null;
        private static Task updateTask = null;
        public static GenericPopup InfoPopup;

        public static void LaunchUpdater() {
            if (running) return;
            running = true;
            checkForUpdate().GetAwaiter().GetResult();
            clearOldVersions();
        }

        public static void ExecuteUpdate() {
            string info = "Updating Town Of Us\nPlease wait...";
            ModUpdater.InfoPopup.Show(info);
            if (updateTask == null) {
                if (updateURI != null) {
                    updateTask = downloadUpdate();
                } else {
                    info = "Unable to auto-update\nPlease update manually";
                }
            } else {
                info = "Update might already\nbe in progress";
            }
            ModUpdater.InfoPopup.StartCoroutine(Effects.Lerp(0.01f, new System.Action<float>((p) => { ModUpdater.setPopupText(info); })));
        }
        
        public static void clearOldVersions() {
            //Removes any old versions (Denoted by the suffix `.old`)
            try {
                DirectoryInfo d = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath) + @"\BepInEx\plugins");
                string[] files = d.GetFiles("*.old").Select(x => x.FullName).ToArray();
                foreach (string f in files)
                    File.Delete(f);
            } catch (System.Exception e) {
                PluginSingleton<TownOfUs>.Instance.Log.LogMessage("Exception occured when clearing old versions:\n" + e);
            }
        }
        public static async Task<bool> checkForUpdate() {
            //Checks the github api for Town Of Us tags. Compares current version (from VersionString in TownOfUs.cs) to the latest tag version(on GitHub)
            try {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TownOfUs Updater");
                //var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/ItsTheNumberH/Town-Of-H/releases/latest"), HttpCompletionOption.ResponseContentRead);
                var response = await http.GetAsync(new System.Uri("https://api.github.com/repos/eDonnes124/Town-Of-Us-R/releases/latest"), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null) {
                    PluginSingleton<TownOfUs>.Instance.Log.LogMessage("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string json = await response.Content.ReadAsStringAsync();
                JObject data = JObject.Parse(json);
                
                string tagname = data["tag_name"]?.ToString();
                if (tagname == null) {
                    return false; // Something went wrong
                }
                // Check version
                System.Version ver = System.Version.Parse(tagname.Replace("v", ""));
                int diff = TownOfUs.Version.CompareTo(ver);
                if (diff < 0) { // Update required
                    hasUpdate = true;
                    JToken assets = data["assets"];
                    if (!assets.HasValues)
                        return false;

                    for (JToken current = assets.First; current != null; current = current.Next) {
                        string browser_download_url = current["browser_download_url"]?.ToString();
                        if (browser_download_url != null && current["content_type"] != null) {
                            if (browser_download_url.EndsWith(".dll")) {
                                updateURI = browser_download_url;
                                return true;
                            }
                        }
                    }
                }  
            } catch (System.Exception ex) {
                PluginSingleton<TownOfUs>.Instance.Log.LogMessage(ex);
            }
            return false;
        }

        public static async Task<bool> downloadUpdate() {
            //Downloads the new TownOfUs dll from GitHub into the plugins folder
            try {
                HttpClient http = new HttpClient();
                http.DefaultRequestHeaders.Add("User-Agent", "TownOfUs Updater");
                var response = await http.GetAsync(new System.Uri(updateURI), HttpCompletionOption.ResponseContentRead);
                if (response.StatusCode != HttpStatusCode.OK || response.Content == null) {
                    PluginSingleton<TownOfUs>.Instance.Log.LogMessage("Server returned no data: " + response.StatusCode.ToString());
                    return false;
                }
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                System.UriBuilder uri = new System.UriBuilder(codeBase);
                string fullname = System.Uri.UnescapeDataString(uri.Path);
                if (File.Exists(fullname + ".old")) // Clear old file in case it wasnt;
                    File.Delete(fullname + ".old");

                File.Move(fullname, fullname + ".old"); // rename current executable to old

                using (var responseStream = await response.Content.ReadAsStreamAsync()) {
                    using (var fileStream = File.Create(fullname)) {
                        responseStream.CopyTo(fileStream);
                    }
                }
                showPopup("Town Of Us\nupdated successfully.\nPlease RESTART the game.");
                return true;
            } catch (System.Exception ex) {
                PluginSingleton<TownOfUs>.Instance.Log.LogMessage(ex);
            }
            showPopup("Update wasn't successful\nTry again later,\nor update manually.");
            return false;
        }
        private static void showPopup(string message) {
            setPopupText(message);
            InfoPopup.gameObject.SetActive(true);
        }

        public static void setPopupText(string message) {
            if (InfoPopup == null)
                return;
            if (InfoPopup.TextAreaTMP != null) {
                InfoPopup.TextAreaTMP.text = message;
            }
        }
    }
}