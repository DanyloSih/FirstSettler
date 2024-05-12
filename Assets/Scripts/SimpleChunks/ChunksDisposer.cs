using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using SimpleChunks.Extensions;
using Zenject;

namespace SimpleChunks
{
    public class ChunksDisposer
    {
        [Inject] private IChunksContainer _activeChunksContainer;

        public async Task DisposeArea(
            IEnumerable<Vector3Int> disposeArea,
            ChunksDisposingParams chunksDisposerParams,
            CancellationToken? cancellationToken = null)
        {
            int i = 0;
            foreach (var position in disposeArea)
            {
                if (cancellationToken != null && cancellationToken.Value.IsCancellationRequested)
                {
                    return;
                }

                if (!_activeChunksContainer.IsChunkExist(position))
                {
                    continue;
                }

                IChunk chunk = _activeChunksContainer.GetChunk(position);
                MonoBehaviour.Destroy(chunk.RootGameObject);
                _activeChunksContainer.RemoveChunk(position);

                if (i % chunksDisposerParams.BatchLength == 0 && chunksDisposerParams.BatchDelay > 0)
                {
                    if (cancellationToken != null)
                    {
                        await Task.Delay(chunksDisposerParams.BatchDelay, cancellationToken.Value);
                    }
                    else
                    {
                        await Task.Delay(chunksDisposerParams.BatchDelay);
                    }
                }

                i++;
            }
        }
    }
}
