using UnityEngine;
using Utilities.Math;
using World.Data;
using Zenject;

namespace FirstSettler.Installers
{
    public class CommonDataObjectsInstaller : MonoInstaller
    {
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialKeyAndUnityMaterialAssociations;

        public override void InstallBindings()
        {
            Container.BindInstance(_basicChunkSettings).AsSingle();

            Container.BindInterfacesAndSelfTo(_materialKeyAndUnityMaterialAssociations.GetType())
                .FromInstance(_materialKeyAndUnityMaterialAssociations).AsSingle();

            Container.Bind<ChunkCoordinatesCalculator>()
                .FromMethod(() => new ChunkCoordinatesCalculator(
                    _basicChunkSettings.Size, _basicChunkSettings.Scale));

            Container.BindInterfacesAndSelfTo<ChunkPrismsProvider>().AsSingle();
        }
    }
}
