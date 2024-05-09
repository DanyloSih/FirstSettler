using MarchingCubesProject;
using MarchingCubesProject.Tools;
using MeshGeneration;
using UnityEngine;
using Utilities.Math;
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
        [SerializeField] private MeshGenerator _meshGenerator;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private ChunksGenerationBehaviour _chunksGenerationBehaviour;

        public override void InstallBindings()
        {
            Container.BindInstance(_basicChunkSettings);

            Container.BindInterfacesAndSelfTo<ChunkPrismsProvider>().AsSingle();

            Container.Bind<ChunkCoordinatesCalculator>()
                .FromMethod(() => new ChunkCoordinatesCalculator(
                    _basicChunkSettings.Size, _basicChunkSettings.Scale));

            Container.BindInterfacesAndSelfTo<ChunksContainer>()
                .FromInstance(_chunksContainer).AsSingle();

            Container.BindInterfacesAndSelfTo<GPUChunkDataGenerator>()
                .FromInstance(_gpuChunkDataGenerator).AsSingle();

            Container.BindInterfacesAndSelfTo<MarchingCubesChunksEditor>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<Chunk>()
                .FromInstance(_chunkPrefab).AsSingle();

            Container.Bind<ChunksDisposer>().ToSelf().AsSingle();
            Container.Bind<ChunksGenerator>().ToSelf().AsSingle().WithArguments(_chunksRoot);

            Container.Bind<ChunksGenerationBehaviour>()
                .FromInstance(_chunksGenerationBehaviour).AsSingle();

            Container.Bind(typeof(MeshGenerator), typeof(IInitializable)).FromInstance(_meshGenerator).AsSingle();
        }
    }
}
