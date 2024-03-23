using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Reactor.Networking.Extensions;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using TMPro;
using TownOfUs.Extensions;
using TownOfUs.Patches.Crypt;
using UnityEngine;

namespace TownOfUs
{
    public static class ExternalContentAdditions
    {
        public class ContentAddition
        {
            public string gpgUnResolved;
            public string Resolved;
            public byte Player;

            public ContentAddition(string content, byte player)
            {
                this.gpgUnResolved = content;
                var x = ContentAdditions.Confirm(content);
                if (x == null) return;
                this.Resolved = x;
                this.Player = player;
            }
        }
        public static List<ContentAddition> contentAdditions = new List<ContentAddition>();
    }


    public static class ContentAdditions
    {
        public static ExternalContentAdditions.ContentAddition? resolvedContent = null;
        public static bool IApplyContent() { return resolvedContent != null; }

        const string key = @"
-----BEGIN PGP PUBLIC KEY BLOCK-----

mDMEZaKCHxYJKwYBBAHaRw8BAQdADPygQhO5M/osqLBlikXON7J7nC6SgMhS9SLF
0mJsYzS0BVRPVS1SiJkEExYKAEEWIQS7lF3DvCrAVZmUNaLoSO4zJxlvPwUCZaKC
HwIbAwUJBaRKcQULCQgHAgIiAgYVCgkICwIEFgIDAQIeBwIXgAAKCRDoSO4zJxlv
P/I7AQCH0miBvuG0n8t2cxg/j2pz4MnCqCxB/XP1PWfaihU0hQD8Dkrfa3h1Vuqk
aHe59v1pJy7pEDyoWlvpCtKmW5BCcgC4OARlooIfEgorBgEEAZdVAQUBAQdAzegJ
oYp5dmd8KDH7OZRamsJToMiUrQK99AJpdVdZeiMDAQgHiH4EGBYKACYWIQS7lF3D
vCrAVZmUNaLoSO4zJxlvPwUCZaKCHwIbDAUJBaRKcQAKCRDoSO4zJxlvP9LHAQDw
ndkYAnqf40ME2LEwTB+sFzk14Dbi9HStNrEpn/bGJAD/Qwpq1MzLV2yr+YXYKI5y
imPx7uj0qHZ6jIQSqYrQ4QA=
=p4Kg
-----END PGP PUBLIC KEY BLOCK-----

";
        public static string? Confirm(string content)
        {
            MemoryStream messageStream = new MemoryStream(Encoding.ASCII.GetBytes(content));
            MemoryStream keyStream = new MemoryStream(Encoding.ASCII.GetBytes(key));
            try
            {
                var msg = VerifyTools.VerifyFile(messageStream,
                        PgpUtilities.GetDecoderStream(keyStream));
                return msg;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static bool isSignedIn = false;

        public static bool SetupDir()
        {
            string destination = Application.persistentDataPath + "/tour/";
            if (!Directory.Exists(destination)) { Directory.CreateDirectory(destination); return false; }
            if (!File.Exists(destination + TownOfUs.ComVer + ".k")) return false;
            if (!File.Exists(destination + TownOfUs.ComVer + ".co")) return false;
            return true;
        }

        public static string FindSavedKey(string suffix = ".k")
        {
            string destination = Application.persistentDataPath + "/tour/" + TownOfUs.ComVer + suffix;
            FileStream file;

            if (File.Exists(destination)) file = File.OpenRead(destination);
            else return "";
            return Encoding.ASCII.GetString(file.ReadFully());
        }

        public static void SaveKey(string s, string suffix = ".k")
        {
            string destination = Application.persistentDataPath + "/tour/" + TownOfUs.ComVer + suffix;
            FileStream file;

            if (File.Exists(destination)) file = File.OpenWrite(destination);
            else file = File.Create(destination);
            file.Write(Encoding.ASCII.GetBytes(s));
            file.Close();
        }

        public static void ResolveSelf()
        {
            if (!isSignedIn) return;
            resolvedContent = new ExternalContentAdditions.ContentAddition(FindSavedKey(".co"), 255);
            Logger<TownOfUs>.Info($"{resolvedContent.Resolved}"); 
        }

        public static bool CheckRetribution(ExternalContentAdditions.ContentAddition addition)
        {
            Logger<TownOfUs>.Info($"Checking {addition?.Player}'s addition");
            bool amCont = resolvedContent == addition;
            bool amHost = AmongUsClient.Instance?.AmHost == true;
            if (!(amHost || amCont)) return false;

            Logger<TownOfUs>.Info($"{addition.Resolved}");

            dynamic RelevantInformation = JObject.Parse(addition.Resolved);

            string key = (string)RelevantInformation["publicKey"];
            if (string.IsNullOrEmpty(key)) throw new Exception("FAILED TO CHECK VALIDITY");

            HttpClient httpClient = new HttpClient();
            var result = httpClient.GetAsync($"https://www.dragonbreath.dev/tourContentAdditions/checkValid?pkey={key}" + ((PlayerControl.AllPlayerControls?.Count == 0) ? "" : $"&user={PlayerControl.AllPlayerControls.ToArray().First(x => x.PlayerId == addition.Player).FriendCode}")).Result;
            Logger<TownOfUs>.Info($"Status {result.StatusCode}");
            switch (result.StatusCode)
            {
                case (HttpStatusCode)404:
                    if (amCont) Retribution("Content additions has encountered a problem.\nAnd must reset.");
                    else KickPlayerWMessage(addition.Player, "claims nonexistant contentadditions");
                    return false;
                case (HttpStatusCode)501:
                    if (amCont) Retribution();
                    else RequestRetribution(addition);
                    return false;
                case (HttpStatusCode)410:
                    if (amCont) Retribution("These contentAdditions were decomissioned");
                    else RequestRetribution(addition, "Content addition has been removed.");
                    return false;
                case (HttpStatusCode)423:
                    if (amCont) Retribution("No Access");
                    else RequestRetribution(addition, "Does not have access to the content addition.");
                    return false;
                default:
                    if (amCont) MessageForSeconds("Content additions may be outdated or incompatible.", 5f);
                    else KickPlayerWMessage(addition.Player, "Incompatible content additions.");
                    return false;
                case (HttpStatusCode)200:
                    if (amCont) MessageForSeconds("Content additions may be outdated or incompatible.",5f);
                    else NotifyAll($"{PlayerControl.AllPlayerControls.ToArray().First(x => x.PlayerId == addition.Player).GetDefaultOutfit().PlayerName} has applied content additions.");
                    return true;
            }
        }

        public static void RequestRetribution(ExternalContentAdditions.ContentAddition addition, string lobmess = "Attempted to load content additions which have not been paid for.")
        {
            if (!AmongUsClient.Instance.AmHost) throw new Exception("Non host attempted to call requestretribution.");
            Utils.Rpc(CustomRPC.ContentAddition, (byte)CustomCARPC.SendRetribution, addition.Player);
            KickPlayerWMessage(addition.Player, lobmess);
        }

        public static void KickPlayerWMessage(byte id, string message)
        {
            AmongUsClient.Instance.KickPlayer(id, false);
            NotifyAll(message);
        }

        public static void NotifyAll(string message)
        {
            if (AmongUsClient.Instance.AmHost) Utils.Rpc(CustomRPC.ContentAddition, (byte)CustomCARPC.SendNotification, message);
            DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(message);
        }

        public static void Retribution(string message = "GAME WILL BE REACTIVATED\nONCE PAYMENT IS SUBMITTED")
        {
            string destination = Application.persistentDataPath + "/tour/" + TownOfUs.ComVer;
            if (File.Exists(destination + ".key")) File.Delete(destination + ".key");
            if (File.Exists(destination + ".co")) File.Delete(destination + ".co");

            MessageForSeconds($"<color=#FF0000>{message}</color>\nGame will quit in 5s.", 5, true);
        }

        public static void MessageForSeconds(string message, float seconds, bool crash = false) {
            var ret = new GameObject("retribution");
            ret.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
            var position = ret.AddComponent<AspectPosition>();
            position.DistanceFromEdge = new Vector3(-0.2f, 2f, -100f);
            position.Alignment = AspectPosition.EdgeAlignments.Top;
            var tmp = ret.AddComponent<TextMeshPro>();
            tmp.text = message;
            tmp.fontSize = 12 * Screen.width / 1920;
            tmp.alignment = TextAlignmentOptions.Center;

            position.StartCoroutine(Effects.Lerp(0.1f, new System.Action<float>((p) =>
            {
                position.AdjustPosition();
            })));
            
            if (crash) position.StartCoroutine(Effects.Lerp(seconds, new System.Action<float>((p) =>
            {
                if (p == 1) Application.Quit();
            })));
            else position.StartCoroutine(Effects.Lerp(seconds, new System.Action<float>((p) =>
            {
                if (p == 1) ret.DestroyImmediate();
            })));
        }
    }
}
