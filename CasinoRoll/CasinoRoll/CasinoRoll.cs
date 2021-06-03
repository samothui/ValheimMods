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
        public const string Version = "0.3";
        public const string ModName = "Casino Roll";
        private Harmony _harmony;


        private static ConfigEntry<bool> isModEnabled;
        private static ConfigEntry<string> rollAnnouncement;
        private static ConfigEntry<bool> isRollAnnouncementEnabled;
        private static ConfigEntry<string> rules;
        private static ConfigEntry<bool> isRulesEnabled;
        private static ConfigEntry<bool> isShowCommandEnabled;

        void Awake()
        {
            isModEnabled = Config.Bind<bool>("1. General", "Enable Mod", true, "Enable mod -  on /roll, posts announcements in chat and rolls the dice with a random number between 1 and 100");
            isRollAnnouncementEnabled = Config.Bind<bool>("1. General", "Enable /roll announcement", true, "Sets whether the announcement on /roll is typed");
            isRulesEnabled = Config.Bind<bool>("1. General", "Enable /rollrules announcement", true, "Sets whether the announcement on /rollrules is typed");
            isShowCommandEnabled = Config.Bind<bool>("1. General", "Enable commands", true, "Sets whether the /roll or /rollrules commands are typed in chat");

            rollAnnouncement = Config.Bind<string>("2. Announcement", "Roll announcement", "This will be the first line of announcements", "The announcement to be typed when you /roll");
            rules = Config.Bind<string>("3. Rules", "Rules", "These are the rules", "Details the rules posted upon /rollrulles");

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
                    if (isRulesEnabled.Value)
                    {
                        __instance.SendText(type, rules.Value);
                    }
                    Player.m_localPlayer.StartEmote("challenge");
                    runOriginalMethod = false;
                } else
                {
                    runOriginalMethod = true;
                }
                return runOriginalMethod;
            }
        }
    }
}
