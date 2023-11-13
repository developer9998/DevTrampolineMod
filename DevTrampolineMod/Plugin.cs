using BepInEx;
using Bepinject;
using HarmonyLib;
using System;
using System.ComponentModel;
using Utilla;

namespace DevTrampolineMod
{
    [ModdedGamemode, Description("HauntedModMenu")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.5.0")]
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static bool IsEnabled;
        public static event Action Enabled, Disabled, Joined, Left;

        public Plugin()
        {
            Zenjector.Install<MainInstaller>().OnProject().WithConfig(Config).WithLog(Logger);
            new Harmony(Constants.Guid).PatchAll(typeof(Plugin).Assembly);
        }

        public void OnEnable()
        {
            IsEnabled = true;
            Enabled?.Invoke();
        }

        public void OnDisable()
        {
            IsEnabled = false;
            Disabled?.Invoke();
        }

        [ModdedGamemodeJoin]
        public void Join() => Joined?.Invoke();

        [ModdedGamemodeLeave]
        public void Leave() => Left?.Invoke();
    }
}
