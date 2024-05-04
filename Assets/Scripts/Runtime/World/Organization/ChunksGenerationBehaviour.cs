using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace World.Organization
{
    public abstract class ChunksGenerationBehaviour : MonoBehaviour
    {
        private CancellationTokenSource _generationTokenSource;
        private bool _isGenerationProcessStarted;

        protected CancellationToken GenerationCancellationToken { get => _generationTokenSource.Token; }

        public void StartGenerationProcess()
        {
            if (_isGenerationProcessStarted)
            {
                throw new InvalidOperationException(
                    $"Generation process already started. " +
                    $"To use this method, you must first call method {nameof(StopGenerationProcess)}");
            }
            _generationTokenSource = new CancellationTokenSource();
            _isGenerationProcessStarted = true;
            OnGenerationProcessStart();
        }

        public void StopGenerationProcess()
        {
            if (!_isGenerationProcessStarted)
            {
                throw new InvalidOperationException(
                    $"The generation process has not started yet. " +
                    $"To use this method, you must first call method {nameof(StartGenerationProcess)}");
            }
            _generationTokenSource.Cancel();
            OnGenerationProcessStop();
            _generationTokenSource.Dispose();
            _generationTokenSource = null;
            _isGenerationProcessStarted = false;
        }

        public abstract Task AwaitGenerationProcess();

        protected abstract void OnGenerationProcessStart();

        protected abstract void OnGenerationProcessStop();
    }
}
