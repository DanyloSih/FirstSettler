using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProceduralNoiseProject;
using Common.Unity.Drawing;

namespace MarchingCubesProject
{
    public class Example : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private int seed = 0;
        [SerializeField] private bool smoothNormals = false;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;

        private List<Vector3> _normals = new List<Vector3>();
        private Marching _marching;
        private FractalNoise _fractal;
        private Vector3 _meshPosition;
        private VoxelArray _voxels;
        private (MeshFilter MeshFilter, MeshCollider MeshCollider) _currentMeshComponents;
        private MeshData _meshData;

        protected void OnEnable()
        {
            InitializeMeshData();
            UpdateNoise();
            UpdateVoxelArray();

            _marching = new MarchingCubes();
            _marching.Surface = 0.0f;

            if (_currentMeshComponents.MeshFilter != null)
            {
                Destroy(_currentMeshComponents.MeshFilter.gameObject);
            }

            _currentMeshComponents = CreateMesh32();
        }

        private void InitializeMeshData()
        {
            int maxVerticesInCube = 15;
            int maxTrianglesInCube = 15;
            int maxUVInCube = 15;
            int cubesCount = _width * _height * _depth;
            _meshData = new MeshData(
                maxVerticesInCube * cubesCount, 
                maxTrianglesInCube * cubesCount, 
                maxUVInCube * cubesCount);
        }

        private void Update()
        {
            CreateNormals();
            UpdateMesh(_meshData, _normals, _meshPosition);
        }

        private void UpdateMesh(MeshData meshData, List<Vector3> normals, Vector3 position)
        {
            var mesh = _currentMeshComponents.MeshFilter.mesh;
            mesh.vertices = meshData.GetCopyOfCashedVerticesWithTargetLength();
            mesh.triangles = meshData.GetCopyOfCashedTrianglesWithTargetLength();
            //mesh.uv = meshData.GetCopyOfCashedUVWithTargetLength();

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            _currentMeshComponents.MeshCollider.sharedMesh = null;
            _currentMeshComponents.MeshCollider.sharedMesh = mesh;
            _currentMeshComponents.MeshFilter.transform.localPosition = position;
        }
        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(seed, 1.0f);
            _fractal = new FractalNoise(perlin, 3, 1.0f);
        }

        private void CreateNormals()
        {
            _meshData = _marching.GenerateMeshData(_voxels, _meshData);

            //Create the normals from the voxel.

            if (smoothNormals)
            {
                for (int i = 0; i < _meshData.VerticesTargetLength; i++)
                {
                    //Presumes the vertex is in local space where
                    //the min value is 0 and max is width/height/depth.
                    Vector3 p = _meshData.CashedVertices[i];

                    float u = p.x / (_width - 1.0f);
                    float v = p.y / (_height - 1.0f);
                    float w = p.z / (_depth - 1.0f);

                    Vector3 n = _voxels.GetNormal(u, v, w);

                    _normals.Add(n);
                }
            }

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

        private (MeshFilter, MeshCollider) CreateMesh32()
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = IndexFormat.UInt32;
           
            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            var filter =  go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            var meshCollider = go.AddComponent<MeshCollider>();
            go.GetComponent<Renderer>().material = material;
            go.GetComponent<MeshFilter>().mesh = mesh;
            
            return (filter, meshCollider);
        }
    }
}
