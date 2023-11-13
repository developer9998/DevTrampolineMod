using DevTrampolineMod.Behaviours;
using GorillaLocomotion;
using UnityEngine;
using Zenject;

namespace DevTrampolineMod
{
    public class MainInstaller : Installer
    {
        public static GameObject Player(InjectContext ctx) => Object.FindObjectOfType<Player>().gameObject;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<Main>().FromNewComponentOn(Player).AsSingle();
            Container.Bind<AssetLoader>().AsSingle();
        }
    }
}
