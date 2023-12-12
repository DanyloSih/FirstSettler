using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

using ProceduralNoiseProject;
using Common.Unity.Drawing;
using UnityEditor.PackageManager;

namespace MarchingCubesProject
{
    public enum MARCHING_MODE {  CUBES, TETRAHEDRON };

    public class Example : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private MARCHING_MODE mode = MARCHING_MODE.CUBES;
        [SerializeField] private int seed = 0;
        [SerializeField] private bool smoothNormals = false;
        [SerializeField] private bool drawNormals = false;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;

        private NormalRenderer _normalRenderer;
        private List<Vector3> _verts = new List<Vector3>();
        private List<Vector3> _normals = new List<Vector3>();
        private List<int> _indices = new List<int>();
        private Marching _marching;
        private MeshFilter _currentMeshFilter;
        private FractalNoise _fractal;
        private Vector3 _meshPosition;
        private VoxelArray _voxels;

        protected void OnEnable()
        {
            UpdateNoise();
            UpdateVoxelArray();
            if (mode == MARCHING_MODE.TETRAHEDRON)
                _marching = new MarchingTertrahedron();
            else
                _marching = new MarchingCubes();

            _marching.Surface = 0.0f;

            if (_currentMeshFilter != null)
            {
                Destroy(_currentMeshFilter.gameObject);
            }

            _currentMeshFilter = CreateMesh32();
        }

        private void Update()
        {
            
            CreateNormals();
            UpdateMesh(_currentMeshFilter, _verts, _normals, _indices, _meshPosition);
        }

        private void UpdateMesh(MeshFilter meshFilter, List<Vector3> verts, List<Vector3> normals, List<int> indices, Vector3 position)
        {
            var mesh = meshFilter.mesh;
            mesh.SetVertices(verts);
            mesh.SetTriangles(indices, 0);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();
            meshFilter.transform.localPosition = position;
        }
        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(seed, 1.0f);
            _fractal = new FractalNoise(perlin, 3, 1.0f);
        }

        private void CreateNormals()
        {
            _verts.Clear();
            _indices.Clear();
            _normals.Clear();

            _marching.Generate(_voxels, _verts, _indices);

            //Create the normals from the voxel.

            //if (smoothNormals)
            //{
            //    for (int i = 0; i < _verts.Count; i++)
            //    {
            //        //Presumes the vertex is in local space where
            //        //the min value is 0 and max is width/height/depth.
            //        Vector3 p = _verts[i];

            //        float u = p.x / (_width - 1.0f);
            //        float v = p.y / (_height - 1.0f);
            //        float w = p.z / (_depth - 1.0f);

            //        Vector3 n = voxels.GetNormal(u, v, w);

            //        _normals.Add(n);
            //    }

            //    _normalRenderer = new NormalRenderer();
            //    _normalRenderer.DefaultColor = Color.red;
            //    _normalRenderer.Length = 0.25f;
            //    _normalRenderer.Load(_verts, _normals);
            //}

            _meshPosition = new Vector3(-_width / 2, -_height / 2, -_depth / 2);
        }

        private VoxelArray UpdateVoxelArray()
        {
            _voxels = _voxels??new VoxelArray(_width, _height, _depth);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _depth; z++)
                    {
                        float u = x / (_width - 1.0f);
                        float v = y / (_height - 1.0f);
                        float w = z / (_depth - 1.0f);

                        _voxels[x, y, z] = _fractal.Sample3D(u, v, w);
                    }
                }
            }
            return _voxels;
        }

        private MeshFilter CreateMesh32()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
           
            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            var filter =  go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            
            return filter;
        }
    }
}
