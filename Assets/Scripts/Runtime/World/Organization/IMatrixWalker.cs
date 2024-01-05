using System;
using System.Threading.Tasks;
using UnityEngine;

namespace World.Organization
{
    public interface IMatrixWalker
    {
        public Task WalkMatrix(Vector3Int matrixSize, Func<int, int, int, Task> matrixWalkingCallbackAsync);
    }
}