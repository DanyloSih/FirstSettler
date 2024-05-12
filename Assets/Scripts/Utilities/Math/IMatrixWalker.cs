using System.Collections.Generic;
using UnityEngine;

namespace Utilities.Math
{
    public interface IMatrixWalker
    {
        public IEnumerable<Vector3Int> WalkMatrix(Vector3Int matrixSize);
    }
}