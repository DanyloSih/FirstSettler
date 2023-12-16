using UnityEngine;

namespace FirstSettler.Extensions
{
    public static class Vector3Extensions
    {
        public static Vector3Int FloorToVector3Int(this Vector3 vector3)
        {
            return new Vector3Int(
                Mathf.FloorToInt(vector3.x), 
                Mathf.FloorToInt(vector3.y), 
                Mathf.FloorToInt(vector3.z));
        }
    }
}