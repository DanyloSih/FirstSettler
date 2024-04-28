using FirstSettler.Player;
using MarchingCubesProject;
using MarchingCubesProject.Tools;
using UnityEngine;
using Utilities.GameObjects;
using World.Organization;
using Zenject;

namespace FirstSettler.Installers
{
    public class PlaySceneEssentialsInstaller : MonoInstaller
    {
        [SerializeField] private ChunksContainer _chunksContainer;
        [SerializeField] private GPUChunkDataGenerator _gpuChunkDataGenerator;
        [SerializeField] private MarchingCubesChunk _chunkPrefab;
        [SerializeField] private ChunksGeneratorBase _chunkGenerator;
        [SerializeField] private ChunkPositionProvider _generationChunkPositionProvider;
        [SerializeField] private PlayerSpawner _playerSpawner;

        public override void InstallBindings()
        {          
            Container.BindInterfacesAndSelfTo<ChunksContainer>()
                .FromInstance(_chunksContainer).AsSingle();

            Container.BindInterfacesAndSelfTo<MarchingCubesChunksEditor>()
                .AsSingle();

            Container.BindInterfacesAndSelfTo<GPUChunkDataGenerator>()
                .FromInstance(_gpuChunkDataGenerator).AsSingle();

            Container.BindInterfacesAndSelfTo<MarchingCubesChunk>()
                .FromInstance(_chunkPrefab).AsSingle();

            Container.Bind<ChunkPositionProvider>()
                .FromInstance(_generationChunkPositionProvider)
                .AsSingle();

            Container.Bind<IObjectAppearanceListner>()
                   .FromInstance(_generationChunkPositionProvider)
                   .AsSingle()
                   .WhenInjectedIntoInstance(_playerSpawner);

            Container.Bind<ChunksGeneratorBase>()
                .FromInstance(_chunkGenerator)
                .AsSingle();

            Container.Bind<PlayerSpawner>().FromInstance(_playerSpawner).AsSingle();
        }
    }
}
