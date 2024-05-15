using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace SimpleChunks
{
    public class ChunksDisposer
    {
        [Inject] private ChunksContainer _activeChunksContainer;

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

                if (!_activeChunksContainer.IsValueExist(position))
                {
                    continue;
                }

                _activeChunksContainer.TryGetValue(position, out var chunk);
                MonoBehaviour.Destroy(chunk.RootGameObject);
                _activeChunksContainer.RemoveValue(position);

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
