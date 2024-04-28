using System.Threading.Tasks;
using FirstSettler.Player;
using SimpleBootstrap;
using World.Organization;
using Zenject;

namespace FirstSettler.Bootstrappers.PlayScene
{
    public class WorldBootstrapScript : AsyncBootstrapScript
    {
        [Inject] private PlayerSpawner _playerSpawner;
        [Inject] private ChunksGeneratorBase _chunksGenerator;

        protected override async Task OnRunAsync(BootstrapContext bootstrapContext)
        {
            _chunksGenerator.StartGenerationProcess();
            //await _chunksGenerator.AwaitGenerationProcess();
            _playerSpawner.RespawnPlayer();
        }
    }
}
