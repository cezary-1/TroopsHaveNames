using HarmonyLib;
using System.Reflection;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace TroopsHaveNames
{
    public class TroopsHaveNamesModule : MBSubModuleBase
    {
        private Harmony _harmony;
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(
                new InformationMessage("Troops Have Names Mod loaded successfully."));
            // Create a Harmony instance with a unique ID
            _harmony = new Harmony("TroopsHaveNames");
            // Tell Harmony to scan your assembly for [HarmonyPatch] classes
            _harmony.PatchAll(Assembly.GetExecutingAssembly());

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
        }

    }
}
