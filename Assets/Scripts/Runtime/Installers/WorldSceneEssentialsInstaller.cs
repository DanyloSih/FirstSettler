using MarchingCubesProject;
using UnityEngine;
using World.Organization;
using Zenject;

namespace FirstSettler.Installers
{
    public class WorldSceneEssentialsInstaller : MonoInstaller
    {
        [SerializeField] private ChunksContainer _chunksContainer;
        [SerializeField] private GPUChunkDataGenerator _gpuChunkDataGenerator;

        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ChunksContainer>()
                .FromInstance(_chunksContainer).AsSingle();

            Container.BindInterfacesAndSelfTo<GPUChunkDataGenerator>()
                .FromInstance(_gpuChunkDataGenerator).AsSingle();
        }
    }
}
