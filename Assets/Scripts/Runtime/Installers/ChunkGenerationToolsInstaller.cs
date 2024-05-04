using MarchingCubesProject;
using MarchingCubesProject.Tools;
using UnityEngine;
using World.Data;
using World.Organization;
using Zenject;

namespace FirstSettler.Installers
{
    public class ChunkGenerationToolsInstaller : MonoInstaller
    {
        [SerializeField] private BasicChunkSettings _basicChunkSettings;
        [SerializeField] private Transform _chunksRoot;
        [SerializeField] private ChunksContainer _chunksContainer;
        [SerializeField] private GPUChunkDataGenerator _gpuChunkDataGenerator;
        [SerializeField] private MarchingCubesChunk _chunkPrefab;
        [SerializeField] private ChunksGenerationBehaviour _chunksGenerationBehaviour;

        public override void InstallBindings()
        {
            Container.BindInstance(_basicChunkSettings);

            Container.Bind<ChunkCoordinatesCalculator>()
                .FromMethod(() => new ChunkCoordinatesCalculator(
                    _basicChunkSettings.Size, _basicChunkSettings.Scale));

            Container.BindInterfacesAndSelfTo<ChunksContainer>()
                .FromInstance(_chunksContainer).AsSingle();

            Container.BindInterfacesAndSelfTo<GPUChunkDataGenerator>()
                .FromInstance(_gpuChunkDataGenerator).AsSingle();

            Container.BindInterfacesAndSelfTo<MarchingCubesChunksEditor>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<MarchingCubesChunk>()
                .FromInstance(_chunkPrefab).AsSingle();

            Container.Bind<ChunksDisposer>().ToSelf().AsSingle();
            Container.Bind<ChunksGenerator>().ToSelf().AsSingle().WithArguments(_chunksRoot);

            Container.Bind<ChunksGenerationBehaviour>()
                .FromInstance(_chunksGenerationBehaviour).AsSingle();
        }
    }
}
