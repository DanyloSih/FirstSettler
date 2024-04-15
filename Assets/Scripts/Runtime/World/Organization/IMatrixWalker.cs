using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace World.Organization
{
    public interface IMatrixWalker
    {
        public IEnumerable<Vector3Int> WalkMatrix(Vector3Int matrixSize);
    }
}