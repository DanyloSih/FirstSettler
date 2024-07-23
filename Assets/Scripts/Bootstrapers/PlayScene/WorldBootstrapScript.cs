using System.Threading.Tasks;
using FirstSettler.Player;
using SimpleBootstrap;
using SimpleChunks;
using UnityEngine;
using Zenject;

namespace FirstSettler.Bootstrappers.PlayScene
{
    public class WorldBootstrapScript : AsyncBootstrapScript
    {
        [SerializeField] private Camera _initialCamera;
        [Tooltip("In milliseconds")]
        [SerializeField] private int _awaitTimeBeforeSpawn = 1000;

        [Inject] private PlayerSpawner _playerSpawner;
        [Inject] private ChunksGenerationBehaviour _chunksGenerator;

        protected override async Task OnRunAsync(BootstrapContext bootstrapContext)
        {
            _initialCamera.gameObject.SetActive(true);
            _chunksGenerator.StartGenerationProcess();
            await Task.Delay(_awaitTimeBeforeSpawn);
            _initialCamera.gameObject.SetActive(false);
            _playerSpawner.RespawnPlayer();
        }


    }
}
