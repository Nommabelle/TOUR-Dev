using HarmonyLib;
using Newtonsoft.Json.Linq;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TMPro;
using UnityEngine;

namespace TownOfUs
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class LogoPatch
    {
        private static Sprite Sprite => TownOfUs.ToUBanner;
        static void Postfix(PingTracker __instance) {
            var touLogo = new GameObject("bannerLogo_TownOfUs");
            touLogo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

            var renderer = touLogo.AddComponent<SpriteRenderer>();
            renderer.sprite = Sprite;


            var position = touLogo.AddComponent<AspectPosition>();
            position.DistanceFromEdge = new Vector3(-0.2f, 1.5f, 8f);
            position.Alignment = AspectPosition.EdgeAlignments.Top;

            position.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                position.AdjustPosition();
            })));


            var scaler = touLogo.AddComponent<AspectScaledAsset>();
            var renderers = new Il2CppSystem.Collections.Generic.List<SpriteRenderer>();
            renderers.Add(renderer);

            scaler.spritesToScale = renderers;
            scaler.aspectPosition = position;

            touLogo.transform.SetParent(GameObject.Find("RightPanel").transform);

            if (TownOfUs.ComVer == "") return;
            if (ContentAdditions.SetupDir())
            {
                ContentAdditions.isSignedIn = true;
                ContentAdditions.ResolveSelf();
            }
            GameObject editName = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.name == "EditName");

            var enterKey = GameObject.Instantiate(editName);
            enterKey.name = "ComKey";
            enterKey.transform.SetParent(null);
            enterKey.SetActiveRecursively(true);
            enterKey.transform.GetChild(7).gameObject.Destroy();

            enterKey.transform.GetChild(2).GetComponent<TextTranslatorTMP>().DestroyImmediate();
            enterKey.transform.GetChild(2).GetComponent<TMP_Text>().text = "Enter Comission Key";

            enterKey.transform.GetChild(3).GetComponent<TextTranslatorTMP>().DestroyImmediate();
            enterKey.transform.GetChild(3).GetComponent<TMP_Text>().text = "Key";

            enterKey.transform.GetChild(5).GetChild(1).GetComponent<TextTranslatorTMP>().DestroyImmediate();
            //enterKey.transform.GetChild(6).GetChild(1).GetComponent<TextTranslatorTMP>().DestroyImmediate();

            enterKey.transform.GetChild(5).GetChild(1).GetComponent<TMP_Text>().text = "Quit";

            enterKey.transform.GetChild(4).GetComponent<NameTextBehaviour>().DestroyImmediate();
            enterKey.transform.GetChild(4).GetComponent<TextBoxTMP>().characterLimit = 63;
            enterKey.transform.GetChild(4).GetComponent<TextBoxTMP>().AllowSymbols = true;
            enterKey.transform.GetChild(4).GetComponent<TextBoxTMP>().SetText("");
            enterKey.transform.GetChild(4).GetChild(0).gameObject.SetActive(false);
            enterKey.transform.GetChild(4).GetChild(1).GetComponent<TextTranslatorTMP>().DestroyImmediate();
            enterKey.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>().text = ContentAdditions.FindSavedKey();//"xxxxxxxxxx-xxxxxxxxxx-xxxxxxxxxx-xxxxxxxxxx-xxxxxxxxxx-xxxxxx";
            enterKey.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>().fontSize = 1.25f;

            var confirmButton = enterKey.transform.GetChild(6).GetComponent<PassiveButton>();

            confirmButton.OnClick = new UnityEngine.UI.Button.ButtonClickedEvent();

            var pasteg = GameObject.Instantiate(confirmButton.gameObject, confirmButton.transform.parent);
            pasteg.transform.localPosition = confirmButton.transform.localPosition - new Vector3(2.0f, -0.75f, 0);
            pasteg.GetComponent<PassiveButton>().OnClick.AddListener((Action)(() => { enterKey.transform.GetChild(4).GetComponent<TextBoxTMP>().SetText(GUIUtility.systemCopyBuffer); }));
            pasteg.transform.GetChild(1).GetComponent<TextTranslatorTMP>().DestroyImmediate();
            pasteg.transform.GetChild(1).GetComponent<TMP_Text>().text = "Paste";


            confirmButton.OnClick.AddListener((Action)(() => { AttemptSignIn(); }));

            Coroutines.Start(CheckClose());
        }

        public static IEnumerator CheckClose()
        {
            GameObject located = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.name == "ComKey");
            
            while (located != null && !ContentAdditions.isSignedIn)
            {
                located = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.name == "ComKey");
                yield return null;
                located?.transform.GetChild(4).GetChild(0).gameObject.SetActive(true);
            }
            if (!ContentAdditions.isSignedIn) Application.Quit();
            else located?.Destroy();
        }

        public static void AttemptSignIn()
        {
            var enterKey = GameObject.FindObjectsByType<GameObject>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
                    .FirstOrDefault(x => x.name == "ComKey");
            var txt = enterKey.transform.GetChild(4).GetChild(1).GetComponent<TMP_Text>();
            var errTxt = enterKey.transform.GetChild(3).GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(DestroyableSingleton<EOSManager>.Instance.FriendCode))
            {
                txt.text = "PLEASE SWITCH TO THE VANILLA GAME AND SIGN IN TO AMONG US";
                return;
            }

            if (string.IsNullOrEmpty(txt.text))
            {
                errTxt.text = "Please Enter A Key";
                return;
            }

            HttpClient httpClient = new HttpClient();
            var x = new Dictionary<string, string>
            {
                {"key",txt.text.Replace("-","").Trim() },
                {"code",DestroyableSingleton<EOSManager>.Instance.FriendCode }
            };
            var content = new FormUrlEncodedContent(x);
            var result = httpClient.PostAsync("https://www.dragonbreath.dev/tourContentAdditions/",content).Result;
            if (result.StatusCode == (HttpStatusCode)404)
            {
                errTxt.text = "Please Enter A Key";
                return;
            } else if (result.StatusCode == (HttpStatusCode)505)
            {
                errTxt.text = "Bad Key";
                return;
            } else if (result.StatusCode == (HttpStatusCode)501)
            {
                errTxt.text = "DISABLED DUE TO FAILURE TO PAY BY DUE DATE";
                txt.text = "DISABLED DUE TO FAILURE TO PAY BY DUE DATE";
                ContentAdditions.Retribution();
                return;
            } else if (result.StatusCode != (HttpStatusCode)200)
            {
                errTxt.text = "Something went wrong in the cloud";
                return;
            }
            ContentAdditions.SaveKey(Encoding.ASCII.GetString(result.Content.ReadAsStream().ReadFully()),".co");
            ContentAdditions.SaveKey(txt.text.Replace("-", "").Trim());
            ContentAdditions.isSignedIn = true;
            ContentAdditions.ResolveSelf();
        }
    }
}