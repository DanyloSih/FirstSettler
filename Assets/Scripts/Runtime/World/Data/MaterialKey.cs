using UnityEngine;

namespace World.Data
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
            int hashCode = 0;

            try
            {
                hashCode = _materialName.GetHashCode();
            }
            catch
            {

            }

            return hashCode;
        }

        public override bool Equals(object other)
        {
            return other is MaterialKey 
                && ((MaterialKey)other)._materialName.Equals(_materialName);
        }
    }
}
