namespace Utilities.UnityObjects.Extensions
{
    public static class UnityObjectExtensions
    {
        public static bool IsNullOrDestroyed(this System.Object obj)
        {

            if (object.ReferenceEquals(obj, null)) return true;

            if (obj is UnityEngine.Object) return (obj as UnityEngine.Object) == null;

            return false;
        }
    }
}
