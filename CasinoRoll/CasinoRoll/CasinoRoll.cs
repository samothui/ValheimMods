using BepInEx;
using HarmonyLib;
using BepInEx.Configuration;
using System.Reflection;
using System;

namespace CasinoRoll
{
    [BepInPlugin("Samothui.Valheim.CasinoRoll", CasinoRoll.ModName, CasinoRoll.Version)]
    [BepInProcess("valheim.exe")]
    public class CasinoRoll : BaseUnityPlugin
    {
        public const string Version = "0.4";
        public const string ModName = "Casino Roll";
        private Harmony _harmony;


        private static ConfigEntry<bool> isModEnabled;
        private static ConfigEntry<string> rollAnnouncement;
        private static ConfigEntry<bool> isRollAnnouncementEnabled;
        private static ConfigEntry<string> rollRules;
        private static ConfigEntry<string> rouletteAnnouncement;
        private static ConfigEntry<bool> isRouletteAnnouncementEnabled;
        private static ConfigEntry<string> rouletteRules;
        private static ConfigEntry<bool> isShowCommandEnabled;

        void Awake()
        {
            isModEnabled = Config.Bind<bool>("1. General", "Enable Mod", true, "Enable mod -  on /roll and /roll2, posts announcements in chat and rolls the dice with a random number between 1 and 100, or 0 and 36");
            isRollAnnouncementEnabled = Config.Bind<bool>("1. General", "Enable /roll announcement", true, "Sets whether the announcement on /roll is typed");
            isRouletteAnnouncementEnabled = Config.Bind<bool>("1. General", "Enable /roll2 announcement", true, "Sets whether the announcement on /roll2 is typed");

            isShowCommandEnabled = Config.Bind<bool>("1. General", "Enable commands", true, "Sets whether the /roll or /rollrules commands are typed in chat");

            rollAnnouncement = Config.Bind<string>("2. Announcement", "Roll announcement", "This will be the first line of announcements", "The announcement to be typed when you /roll");
            rollRules = Config.Bind<string>("3. Rules", "Roll Rules", "These are the rules", "Details the rules posted upon /rollrules");

            rouletteAnnouncement = Config.Bind<string>("2. Announcement", "Roulette announcement", "This will be the first line of announcements", "The announcement to be typed when you /roll2");
            rouletteRules = Config.Bind<string>("3. Rules", "Roulette Rules", "These are the rules", "Details the rules posted upon /roll2rules");

            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private void OnDestroy()
        {
            if (_harmony != null)
            {
                _harmony.UnpatchAll(null);
            }
        }

        [HarmonyPatch(typeof(Chat), "Roll")]
        private static class ChatPatch
        {
            private static readonly Random rnd = new Random(Guid.NewGuid().GetHashCode());

            [HarmonyPrefix]
            [HarmonyPatch(nameof(Chat.InputText))]
            private static bool InputText_Patch(ref Chat __instance)
            {
                bool runOriginalMethod = true;

                if (!isModEnabled.Value)
                {
                    return false;
                }

                string text = __instance.m_input.text;
                Talker.Type type = Talker.Type.Normal;

                if (text == "/roll")
                {
                    if (isShowCommandEnabled.Value)
                    {
                        __instance.SendText(type, text);
                    }
                    if (isRollAnnouncementEnabled.Value)
                    {
                        __instance.SendText(type, rollAnnouncement.Value);
                    }
                    Player.m_localPlayer.StartEmote("cheer");
   
                    __instance.SendText(type, Convert.ToString(rnd.Next(1, 101)));
                    runOriginalMethod = false;
                }
                else if (text =="/rollrules")
                {
                    if (isShowCommandEnabled.Value)
                    {
                        __instance.SendText(type, text);
                    }
                    __instance.SendText(type, rollRules.Value);
                    Player.m_localPlayer.StartEmote("challenge");
                    runOriginalMethod = false;
                }
                else if (text == "/roll2")
                {
                    if (isShowCommandEnabled.Value)
                    {
                        __instance.SendText(type, "/roulette");
                    }
                    if (isRouletteAnnouncementEnabled.Value)
                    {
                        __instance.SendText(type, rouletteAnnouncement.Value);
                    }
                    Player.m_localPlayer.StartEmote("cheer");

                    __instance.SendText(type, Convert.ToString(rnd.Next(0, 37)));
                    runOriginalMethod = false;
                }
                else if (text == "/roll2rules")
                {
                    if (isShowCommandEnabled.Value)
                    {
                        __instance.SendText(type, "/rouletteRules");
                    }
                    __instance.SendText(type, rouletteRules.Value);
                    Player.m_localPlayer.StartEmote("challenge");
                    runOriginalMethod = false;
                }
                 else
                {
                    runOriginalMethod = true;
                }
                return runOriginalMethod;
            }
        }
    }
}
