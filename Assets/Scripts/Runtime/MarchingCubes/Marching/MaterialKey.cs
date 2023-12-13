using UnityEngine;

namespace MarchingCubesProject
{
    [CreateAssetMenu(
        fileName = nameof(MaterialKey),
        menuName = "FirstSettler/MarchingCubes/" + nameof(MaterialKey))]
    public class MaterialKey : ScriptableObject
    {
        [SerializeField] private string _materialName;

        public string MaterialName { get => _materialName; }

        public override int GetHashCode()
        {
            return _materialName.GetHashCode();
        }
    }
}
