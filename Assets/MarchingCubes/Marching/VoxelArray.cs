using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubesProject
{
    /// <summary>
    /// A helper class to hold voxel data.
    /// </summary>
    public class VoxelArray
    {
        private float[] _voxels;
        private bool _flipNormals;

        private int _width;
        private int _height;
        private int _depth;

        private int _strideY;
        private int _strideZ;

        private float _halfWidthMinusOne;
        private float _halfHeightMinusOne;
        private float _halfDepthMinusOne;

        public VoxelArray(int width, int height, int depth)
        {
            _width = width;
            _height = height;
            _depth = depth;

            _strideY = _width;
            _strideZ = _width * _height;

            _halfWidthMinusOne = _width * 0.5f - 1;
            _halfHeightMinusOne = _height * 0.5f - 1;
            _halfDepthMinusOne = _depth * 0.5f - 1;

            _voxels = new float[_width * _height * _depth];
            _flipNormals = true;
        }

        public int Width => _width;
        public int Height => _height;
        public int Depth => _depth;
        public bool FlipNormals
        {
            get => _flipNormals;
            set => _flipNormals = value;
        }

        public float this[int x, int y, int z]
        {
            get => _voxels[x + y * _strideY + z * _strideZ];
            set => _voxels[x + y * _strideY + z * _strideZ] = value;
        }

        public float GetVoxel(int x, int y, int z)
        {
            x = Mathf.Clamp(x, 0, _width - 1);
            y = Mathf.Clamp(y, 0, _height - 1);
            z = Mathf.Clamp(z, 0, _depth - 1);
            return _voxels[x + y * _strideY + z * _strideZ];
        }

        public float GetVoxel(float u, float v, float w)
        {
            float x = u * _halfWidthMinusOne;
            float y = v * _halfHeightMinusOne;
            float z = w * _halfDepthMinusOne;

            int xi = (int)Mathf.Floor(x);
            int yi = (int)Mathf.Floor(y);
            int zi = (int)Mathf.Floor(z);

            float tx = Mathf.Clamp01(x - xi);
            float ty = Mathf.Clamp01(y - yi);
            float tz = Mathf.Clamp01(z - zi);

            float v000 = GetVoxel(xi, yi, zi);
            float v100 = GetVoxel(xi + 1, yi, zi);
            float v010 = GetVoxel(xi, yi + 1, zi);
            float v110 = GetVoxel(xi + 1, yi + 1, zi);

            float v001 = GetVoxel(xi, yi, zi + 1);
            float v101 = GetVoxel(xi + 1, yi, zi + 1);
            float v011 = GetVoxel(xi, yi + 1, zi + 1);
            float v111 = GetVoxel(xi + 1, yi + 1, zi + 1);

            float v0 = BLerp(v000, v100, v010, v110, tx, ty);
            float v1 = BLerp(v001, v101, v011, v111, tx, ty);

            return Lerp(v0, v1, tz);
        }

        public Vector3 GetNormal(int x, int y, int z)
        {
            var n = GetFirstDerivative(x, y, z);

            return _flipNormals ? -n.normalized : n.normalized;
        }

        public Vector3 GetNormal(float u, float v, float w)
        {
            var n = GetFirstDerivative(u, v, w);

            return _flipNormals ? -n.normalized : n.normalized;
        }

        public Vector3 GetFirstDerivative(int x, int y, int z)
        {
            float dx_p1 = GetVoxel(x + 1, y, z);
            float dy_p1 = GetVoxel(x, y + 1, z);
            float dz_p1 = GetVoxel(x, y, z + 1);

            float dx_m1 = GetVoxel(x - 1, y, z);
            float dy_m1 = GetVoxel(x, y - 1, z);
            float dz_m1 = GetVoxel(x, y, z - 1);

            float dx = (dx_p1 - dx_m1) * 0.5f;
            float dy = (dy_p1 - dy_m1) * 0.5f;
            float dz = (dz_p1 - dz_m1) * 0.5f;

            return new Vector3(dx, dy, dz);
        }

        public Vector3 GetFirstDerivative(float u, float v, float w)
        {
            const float h = 0.005f;
            const float hh = h * 0.5f;
            const float ih = 1.0f / h;

            float dx_p1 = GetVoxel(u + hh, v, w);
            float dy_p1 = GetVoxel(u, v + hh, w);
            float dz_p1 = GetVoxel(u, v, w + hh);

            float dx_m1 = GetVoxel(u - hh, v, w);
            float dy_m1 = GetVoxel(u, v - hh, w);
            float dz_m1 = GetVoxel(u, v, w - hh);

            float dx = (dx_p1 - dx_m1) * ih;
            float dy = (dy_p1 - dy_m1) * ih;
            float dz = (dz_p1 - dz_m1) * ih;

            return new Vector3(dx, dy, dz);
        }

        private static float Lerp(float v0, float v1, float t)
        {
            return v0 + (v1 - v0) * t;
        }

        private static float BLerp(float v00, float v10, float v01, float v11, float tx, float ty)
        {
            return Lerp(Lerp(v00, v10, tx), Lerp(v01, v11, tx), ty);
        }
    }



}
