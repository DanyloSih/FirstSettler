using System;
using UnityEngine;

namespace World.Data
{

    [Serializable]
    public struct KeyAndMaterialAssociation 
    {
        [SerializeField] private MaterialKey _materialKey;
        [SerializeField] private Material _unityMaterial;

        public MaterialKey MaterialKey { get => _materialKey; }
        public Material UnityMaterial { get => _unityMaterial; }
    }
}
