using SimpleChunks;
using SimpleChunks.DataGeneration;
using SimpleChunks.MeshGeneration;
using SimpleChunks.Tools;
using UnityEngine;
using Zenject;

namespace FirstSettler.Installers
{
    public class ChunkGenerationToolsInstaller : MonoInstaller
    {
        [SerializeField] private Transform _chunksRoot;
        [SerializeField] private GPUPerlinNoiseChunkDataGenerator _gpuChunkDataGenerator;
        [SerializeField] private MeshGenerator _meshGenerator;
        [SerializeField] private Chunk _chunkPrefab;
        [SerializeField] private ChunksGenerationBehaviour _chunksGenerationBehaviour;

        public override void InstallBindings()
        {     
            Container.BindInterfacesAndSelfTo<ChunksContainer>().AsSingle();

            Container.BindInterfacesAndSelfTo<GPUPerlinNoiseChunkDataGenerator>()
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
