using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MarchingCubesProject
{
    public abstract class Marching
    {
        public float Surface { get; set; }
        protected int[] WindingOrder { get; private set; }

        private float[] _cube = new float[8];

        protected Marching(float surface)
        {
            Surface = surface;
            WindingOrder = new int[] { 0, 1, 2 };
        }

        public virtual void Generate(VoxelArray voxels, IList<Vector3> verts, IList<int> indices)
        {
            UpdateWindingOrder();
            int width = voxels.Width;
            int height = voxels.Height;

            int widthMinusOne = width - 1;
            int heightMinusOne = height - 1;
            int depthMinusOne = voxels.Depth - 1;

            int totalVertices = widthMinusOne * heightMinusOne * depthMinusOne;

            for (int i = 0; i < totalVertices; i++)
            {
                int x = i % widthMinusOne;
                int y = (i / widthMinusOne) % heightMinusOne;
                int z = i / (widthMinusOne * heightMinusOne);

                ApplyVertexOffsets(voxels, x, y, z, _cube);

                March(x, y, z, _cube, verts, indices);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected void UpdateWindingOrder()
        {
            if (Surface > 0.0f)
            {
                WindingOrder[0] = 2;
                WindingOrder[1] = 1;
                WindingOrder[2] = 0;
            }
            else
            {
                WindingOrder[0] = 0;
                WindingOrder[1] = 1;
                WindingOrder[2] = 2;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected float GetOffset(float v1, float v2)
        {
            float delta = v2 - v1;
            return (delta == 0.0f) ? Surface : (Surface - v1) / delta;
        }

        protected static readonly int[,] VertexOffset = new int[,]
        {
            {0, 0, 0}, {1, 0, 0}, {1, 1, 0}, {0, 1, 0},
            {0, 0, 1}, {1, 0, 1}, {1, 1, 1}, {0, 1, 1}
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ApplyVertexOffsets(VoxelArray voxels, int x, int y, int z, float[] cube)
        {
            cube[0] = voxels[x, y, z];
            cube[1] = voxels[x + 1, y, z];
            cube[2] = voxels[x + 1, y + 1, z];
            cube[3] = voxels[x, y + 1, z];
            cube[4] = voxels[x, y, z + 1];
            cube[5] = voxels[x + 1, y, z + 1];
            cube[6] = voxels[x + 1, y + 1, z + 1];
            cube[7] = voxels[x, y + 1, z + 1];
        }
    }
}
