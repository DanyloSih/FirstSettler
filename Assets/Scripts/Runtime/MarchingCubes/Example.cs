using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using ProceduralNoiseProject;
using System;
using Random = UnityEngine.Random;
using System.Linq;

namespace MarchingCubesProject
{
    public class Example : MonoBehaviour
    {
        [SerializeField] private MaterialKeyAndUnityMaterialAssociations _materialAssociations;
        [SerializeField] private MaterialKeysPack _materialKeysPack;
        [SerializeField] private int seed = 0;
        [SerializeField] private int _width;
        [SerializeField] private int _height;
        [SerializeField] private int _depth;

        private Dictionary<int, int> _materialKeyAndSubmeshAssociation = new Dictionary<int, int>();
        private List<Vector3> _normals = new List<Vector3>();
        private Marching _marching;
        private FractalNoise _fractal;
        private Vector3 _meshPosition;
        private ChunkData _voxels;
        private (MeshFilter MeshFilter, MeshCollider MeshCollider) _currentMeshComponents;
        private MeshData _meshData;
        private bool _meshColliderUpdated;

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
            int cubesCount = _width * _height * _depth;
            _meshData = new MeshData(
                maxVerticesInCube * cubesCount, 
                maxTrianglesInCube * cubesCount,
                _materialKeysPack);
        }

        private void Update()
        {
            CreateNormals();
            UpdateMesh(_meshData, _normals, _meshPosition);
        }

        private void FixedUpdate()
        {
            if (!_meshColliderUpdated)
            {
                _currentMeshComponents.MeshCollider.sharedMesh = null;
                _currentMeshComponents.MeshCollider.sharedMesh
                    = _currentMeshComponents.MeshFilter.sharedMesh;
                _meshColliderUpdated = true;
            }
        }

        private void UpdateMesh(MeshData meshData, List<Vector3> normals, Vector3 position)
        {
            var mesh = _currentMeshComponents.MeshFilter.mesh;
            mesh.SetVertices(meshData.CashedVertices, 0, meshData.VerticesTargetLength);
            InitializeTriangles(meshData, mesh);
            mesh.SetUVs(0, meshData.CashedUV, 0, meshData.UvTargetLength);

            if (normals.Count > 0)
                mesh.SetNormals(normals);
            else
                mesh.RecalculateNormals();

            mesh.RecalculateBounds();

            _currentMeshComponents.MeshFilter.transform.localPosition = position;
            _meshColliderUpdated = false;
        }

        private void InitializeTriangles(MeshData meshData, Mesh mesh)
        {
            //mesh.subMeshCount = meshData.GetMaterialKeyHashAndTriangleListAssociations().Count();
            foreach (var item in meshData.GetMaterialKeyHashAndTriangleListAssociations())
            {
                List<int> triangles = item.Value;
                int minTriangleValue = triangles.Min();
                if (_materialKeyAndSubmeshAssociation.TryGetValue(item.Key, out var submeshId))
                {
                    //mesh.SetSubMesh(submeshId, new SubMeshDescriptor(minTriangleValue, triangles.Count));
                    mesh.SetTriangles(triangles, 0, triangles.Count, submeshId);
                }
                else
                {
                    throw new InvalidOperationException(
                        $"There no {nameof(submeshId)} associated with hash: {item.Key}");
                }
            }
        }

        private void UpdateNoise()
        {
            INoise perlin = new PerlinNoise(seed, 1.0f);
            _fractal = new FractalNoise(perlin, 3, 1.0f);
        }

        private void CreateNormals()
        {
            _meshData = _marching.GenerateMeshData(_voxels, _meshData);
            _meshPosition = new Vector3(-_width / 2, -_height / 2, -_depth / 2);
        }

        private ChunkData UpdateVoxelArray()
        {
            int keysCount = _materialKeysPack.MaterialKeys.Count;
            
            _voxels = _voxels??new ChunkData(_width, _height, _depth);
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int z = 0; z < _depth; z++)
                    {
                        float u = x / (_width - 1.0f);
                        float v = y / (_height - 1.0f);
                        float w = z / (_depth - 1.0f);

                        _voxels.SetVolume(x, y, z, _fractal.Sample3D(u, v, w));
                        var hash = _materialKeysPack.MaterialKeys[Random.Range(0, keysCount)].GetHashCode();
                        _voxels.SetMaterialHash(x, y, z, hash);
                    }
                }
            }

            return _voxels;
        }

        private (MeshFilter, MeshCollider) CreateMesh32()
        {
            List<Material> materials = new List<Material>();
            List<SubMeshDescriptor> descriptors = new List<SubMeshDescriptor>();
            _materialKeyAndSubmeshAssociation.Clear();
            int submeshCounter = 0;
            foreach (var item in _materialAssociations.GetMaterialKeyHashAndMaterialAssociations())
            {
                _materialKeyAndSubmeshAssociation.Add(item.Key, submeshCounter);
                materials.Add(item.Value);
                submeshCounter++;
            }

            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.subMeshCount = materials.Count;
            mesh.indexFormat = IndexFormat.UInt32;

            GameObject go = new GameObject("Mesh");
            go.transform.parent = transform;
            var filter =  go.AddComponent<MeshFilter>();
            var meshCollider = go.AddComponent<MeshCollider>();
            go.AddComponent<MeshRenderer>();
            Renderer renderer = go.GetComponent<Renderer>();
            filter.mesh = mesh;
            filter.mesh.subMeshCount = materials.Count;
            renderer.materials = materials.ToArray();
            
            return (filter, meshCollider);
        }
    }
}
