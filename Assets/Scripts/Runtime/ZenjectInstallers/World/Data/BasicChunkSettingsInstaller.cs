using UnityEngine;
using World.Data;
using Zenject;

namespace ZenjectInstallers.World.Data
{
    public class BasicChunkSettingsInstaller : MonoInstaller
    {
        [SerializeField] private BasicChunkSettings _basicChunkSettings;

        public override void InstallBindings()
        {
            Container.BindInstance(_basicChunkSettings);
        }
    }
}