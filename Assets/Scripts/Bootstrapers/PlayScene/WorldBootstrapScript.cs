using System.Threading.Tasks;
using FirstSettler.Player;
using SimpleBootstrap;
using SimpleChunks;
using Zenject;

namespace FirstSettler.Bootstrappers.PlayScene
{
    public class WorldBootstrapScript : AsyncBootstrapScript
    {
        [Inject] private PlayerSpawner _playerSpawner;
        [Inject] private ChunksGenerationBehaviour _chunksGenerator;

        protected override async Task OnRunAsync(BootstrapContext bootstrapContext)
        {
            _chunksGenerator.StartGenerationProcess();
            await Task.Delay(1);
            //await _chunksGenerator.AwaitGenerationProcess();
            _playerSpawner.RespawnPlayer();
        }
    }
}
