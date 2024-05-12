using FirstSettler.Player;
using SimpleChunks;
using UnityEngine;
using Utilities.GameObjects;
using Zenject;

namespace FirstSettler.Installers
{
    public class PlaySceneEssentialsInstaller : MonoInstaller
    {
        [SerializeField] private ChunkPositionProvider _generationChunkPositionProvider;
        [SerializeField] private PlayerSpawner _playerSpawner;

        public override void InstallBindings()
        {          
            Container.Bind<ChunkPositionProvider>()
                .FromInstance(_generationChunkPositionProvider)
                .AsSingle();

            Container.Bind<IObjectAppearanceListner>()
                   .FromInstance(_generationChunkPositionProvider)
                   .AsSingle()
                   .WhenInjectedIntoInstance(_playerSpawner);

            Container.Bind<PlayerSpawner>().FromInstance(_playerSpawner).AsSingle();
        }
    }
}
