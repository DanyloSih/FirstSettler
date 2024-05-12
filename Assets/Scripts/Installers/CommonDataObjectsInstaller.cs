using SimpleChunks.DataGeneration;
using UnityEngine;
using Utilities.Math;
using Zenject;

namespace FirstSettler.Installers
{
    public class CommonDataObjectsInstaller : MonoInstaller
    {
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialKeyAndUnityMaterialAssociations;
        [SerializeField] private GenerationAlgorithmInfo _generationAlgorithmInfo;

        public override void InstallBindings()
        {
            Container.BindInstance(_basicChunkSettings).AsSingle();

            Container.BindInterfacesAndSelfTo(_materialKeyAndUnityMaterialAssociations.GetType())
                .FromInstance(_materialKeyAndUnityMaterialAssociations).AsSingle();

            Container.Bind<ChunkCoordinatesCalculator>()
                .FromMethod(() => new ChunkCoordinatesCalculator(
                    _basicChunkSettings.SizeInCubes, _basicChunkSettings.Scale));

            Container.BindInterfacesAndSelfTo<ChunkPrismsProvider>().AsSingle();

            Container.BindInstance(_generationAlgorithmInfo).AsSingle();
        }
    }
}
