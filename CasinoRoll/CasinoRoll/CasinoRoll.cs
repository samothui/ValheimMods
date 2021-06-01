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
        public const string Version = "0.2";
        public const string ModName = "Casino Roll";
        private Harmony _harmony;


        private static ConfigEntry<bool> isModEnabled;
        private static ConfigEntry<string> rollAnnouncement;
        private static ConfigEntry<string> rules;

        void Awake()
        {
            isModEnabled = Config.Bind<bool>("1. General", "Enable Mod", true, "Enable mod let out announcements on /roll");
            rollAnnouncement= Config.Bind<string>("2. Announcement", "Roll announcement", "This will be the first line of announcements", "The announcement to be said when you /roll");
            rules = Config.Bind<string>("3. Rules", "Rules", "These are the rules", "Detail the rules posted upon /rollrulles");

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

            [HarmonyPostfix]
            [HarmonyPatch(nameof(Chat.InputText))]
            private static void InputText_Patch(ref Chat __instance)
            {
                if (!isModEnabled.Value)
                {
                    return;
                }

                string text = __instance.m_input.text;
                Talker.Type type = Talker.Type.Normal;

                if (text == "/roll")
                {
                    __instance.SendText(type, rollAnnouncement.Value);
                    Player.m_localPlayer.StartEmote("cheer");
                    __instance.SendText(type, Convert.ToString(rnd.Next(1, 100)));

                }
                else if (text =="/rollrules")
                {
                    Player.m_localPlayer.StartEmote("challenge");
                    __instance.SendText(type, rules.Value);
                }

            }
        }
    }
}
