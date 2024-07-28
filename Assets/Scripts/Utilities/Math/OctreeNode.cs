using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Utilities.Math
{
    public struct OctreeNode<T> : IEquatable<OctreeNode<T>>
        where T : unmanaged, IEquatable<T>
    {
        public T Data;
        /// <summary>
        /// Determines how many steps to the left you need to 
        /// shift the bits of the number 1 to get the "<see cref="Size"/>". <br/>
        /// It also determines the maximum degree of nesting of "<see cref="OctreeNode"/>s". <br/>
        /// For example rank = 4: <br/>
        /// size = 1 &lt;&lt; 4; <br/>
        /// size == 16; <br/>
        /// Nesting struct: Node4 -&gt; Node3 -&gt; Node2 -&gt; Node1 -&gt; Node0;
        /// </summary>
        public readonly byte Rank;
        /// <summary>
        /// Defines the position of a node within a rank
        /// </summary>
        public readonly Vector3Int RankPosition;

        /// <summary>
        /// Defines the position of a node within the entire Octree
        /// </summary>
        public Vector3Int GlobalPosition => RankPosition * (1 << Rank);

        /// <param name="rank">Rank >= 0 and Rank < 8 </param>
        public OctreeNode(T data, int rank, Vector3Int rankPosition)
        {
            Data = data;
            Rank = (byte)rank;
            RankPosition = rankPosition;
        }

        /// <summary>
        /// Doesn't take <see cref="Data"/> into account!
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(OctreeNode<T> other)
        {
            return Rank == other.Rank && other.RankPosition == RankPosition;
        }

        /// <summary>
        /// <inheritdoc cref="Equals(OctreeNode{T})"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is OctreeNode<T>)
            {
                return Equals((OctreeNode<T>)obj);
            }

            return false;
        }

        /// <summary>
        /// <inheritdoc cref="Equals(OctreeNode{T})"/>
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return (RankPosition.GetHashCode() << 3) | Rank;
        }

        /// <param name="rank">Rank >= 0 and Rank < 8 </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static OctreeNode<T> FromGlobalPosition(T data, int rank, Vector3Int globalPosition)
        {
            return new OctreeNode<T>(data, rank, GlobalToRankVector(rank, globalPosition));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int GlobalToRankVector(int rank, Vector3Int globalVector)
        {
            int size = 1 << rank;
            return new Vector3Int(globalVector.x / size, globalVector.y / size, globalVector.z / size);
        }
    }
}