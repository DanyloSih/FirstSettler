using Utilities.Common;
using Zenject;

namespace FirstSettler.Installers
{
    public class SceneStateProviderInstaller : MonoInstaller
    {
        private SceneStateProvider _sceneStateProvider;

        public override void InstallBindings()
        {
            _sceneStateProvider = gameObject.AddComponent<SceneStateProvider>();
            Container.BindInstance(_sceneStateProvider).AsSingle();
        }
    }
}
