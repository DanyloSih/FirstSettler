using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MarchingCubesProject
{
    public class MarchingTertrahedron : Marching
    {

        private readonly Vector3[] EdgeVertex = new Vector3[6];
        private readonly Vector3[] CubePosition = new Vector3[8];
        private readonly Vector3[] TetrahedronPosition = new Vector3[4];
        private readonly float[] TetrahedronValue = new float[4];

        private readonly Vector3[] EdgeVertexCache = new Vector3[6];
        private readonly int[] TetrahedronEdgeFlagsCache = new int[16];

        public MarchingTertrahedron(float surface = 0.0f)
            : base(surface)
        {
            InitializeCache();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void March(float x, float y, float z, float[] cube, IList<Vector3> vertList, IList<int> indexList)
        {
            CacheCubePositions(x, y, z);

            for (int i = 0; i < 6; i++)
            {
                CacheTetrahedronPositionsAndValues(i, cube);
                MarchTetrahedron(vertList, indexList);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CacheCubePositions(float x, float y, float z)
        {
            var vertexOffset = VertexOffset;

            for (int i = 0; i < 8; i++)
            {
                CubePosition[i].x = x + vertexOffset[i, 0];
                CubePosition[i].y = y + vertexOffset[i, 1];
                CubePosition[i].z = z + vertexOffset[i, 2];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void CacheTetrahedronPositionsAndValues(int i, float[] cube)
        {
            for (int j = 0; j < 4; j++)
            {
                int vertexInACube = TetrahedronsInACube[i, j];
                TetrahedronPosition[j] = CubePosition[vertexInACube];
                TetrahedronValue[j] = cube[vertexInACube];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InitializeCache()
        {
            for (int i = 0; i < 6; i++)
            {
                EdgeVertexCache[i] = new Vector3();
            }

            for (int i = 0; i < 16; i++)
            {
                TetrahedronEdgeFlagsCache[i] = TetrahedronEdgeFlags[i];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void MarchTetrahedron(IList<Vector3> vertList, IList<int> indexList)
        {
            int flagIndex = 0, edgeFlags;
            float offset, invOffset;

            for (int i = 0; i < 4; i++) if (TetrahedronValue[i] <= Surface) flagIndex |= 1 << i;

            edgeFlags = TetrahedronEdgeFlagsCache[flagIndex];

            if (edgeFlags == 0) return;

            for (int i = 0; i < 6; i++)
            {
                if ((edgeFlags & (1 << i)) != 0)
                {
                    int vert0 = TetrahedronEdgeConnection[i, 0];
                    int vert1 = TetrahedronEdgeConnection[i, 1];
                    offset = GetOffset(TetrahedronValue[vert0], TetrahedronValue[vert1]);
                    invOffset = 1.0f - offset;

                    EdgeVertex[i].x = invOffset * TetrahedronPosition[vert0].x + offset * TetrahedronPosition[vert1].x;
                    EdgeVertex[i].y = invOffset * TetrahedronPosition[vert0].y + offset * TetrahedronPosition[vert1].y;
                    EdgeVertex[i].z = invOffset * TetrahedronPosition[vert0].z + offset * TetrahedronPosition[vert1].z;
                }
            }

            for (int i = 0; i < 2; i++)
            {
                if (TetrahedronTriangles[flagIndex, 3 * i] < 0) break;

                int idx = vertList.Count;

                for (int j = 0; j < 3; j++)
                {
                    int vert = TetrahedronTriangles[flagIndex, 3 * i + j];
                    indexList.Add(idx + WindingOrder[j]);
                    vertList.Add(EdgeVertex[vert]);
                }
            }
        }

        /// <summary>
        /// TetrahedronEdgeConnection lists the index of the endpoint vertices for each of the 6 edges of the tetrahedron.
        /// tetrahedronEdgeConnection[6][2]
        /// </summary>
        private static readonly int[,] TetrahedronEdgeConnection = new int[,]
	    {
	        {0,1},  {1,2},  {2,0},  {0,3},  {1,3},  {2,3}
	    };

        /// <summary>
        /// TetrahedronEdgeConnection lists the index of verticies from a cube 
        /// that made up each of the six tetrahedrons within the cube.
        /// tetrahedronsInACube[6][4]
        /// </summary>
        private static readonly int[,] TetrahedronsInACube = new int[,]
	    {
	        {0,5,1,6},
	        {0,1,2,6},
	        {0,2,3,6},
	        {0,3,7,6},
	        {0,7,4,6},
	        {0,4,5,6}
	    };

        /// <summary>
        /// For any edge, if one vertex is inside of the surface and the other is outside of 
        /// the surface then the edge intersects the surface
        /// For each of the 4 vertices of the tetrahedron can be two possible states, 
        /// either inside or outside of the surface
        /// For any tetrahedron the are 2^4=16 possible sets of vertex states.
        /// This table lists the edges intersected by the surface for all 16 possible vertex states.
        /// There are 6 edges.  For each entry in the table, if edge #n is intersected, then bit #n is set to 1.
        /// tetrahedronEdgeFlags[16]
        /// </summary>
        private static readonly int[] TetrahedronEdgeFlags = new int[]
	    {
		    0x00, 0x0d, 0x13, 0x1e, 0x26, 0x2b, 0x35, 0x38, 0x38, 0x35, 0x2b, 0x26, 0x1e, 0x13, 0x0d, 0x00
	    };

        /// <summary>
        /// For each of the possible vertex states listed in tetrahedronEdgeFlags there
        /// is a specific triangulation of the edge intersection points.  
        /// TetrahedronTriangles lists all of them in the form of 0-2 edge triples 
        /// with the list terminated by the invalid value -1.
        /// tetrahedronTriangles[16][7]
        /// </summary>
        private static readonly int[,] TetrahedronTriangles = new int[,]
	    {
            {-1, -1, -1, -1, -1, -1, -1},
            { 0,  3,  2, -1, -1, -1, -1},
            { 0,  1,  4, -1, -1, -1, -1},
            { 1,  4,  2,  2,  4,  3, -1},

            { 1,  2,  5, -1, -1, -1, -1},
            { 0,  3,  5,  0,  5,  1, -1},
            { 0,  2,  5,  0,  5,  4, -1},
            { 5,  4,  3, -1, -1, -1, -1},

            { 3,  4,  5, -1, -1, -1, -1},
            { 4,  5,  0,  5,  2,  0, -1},
            { 1,  5,  0,  5,  3,  0, -1},
            { 5,  2,  1, -1, -1, -1, -1},

            { 3,  4,  2,  2,  4,  1, -1},
            { 4,  1,  0, -1, -1, -1, -1},
            { 2,  3,  0, -1, -1, -1, -1},
            {-1, -1, -1, -1, -1, -1, -1}
	    };

    }

}
