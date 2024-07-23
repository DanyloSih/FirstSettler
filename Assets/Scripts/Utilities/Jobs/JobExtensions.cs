using Unity.Jobs;

namespace Utilities.Jobs
{
    public static class JobExtensions
    {
        public static JobHandle RunSequential<T>(this T jobData, int length) where T : struct, IJobParallelFor
        {
            for (int i = 0; i < length; i++)
            {
                jobData.Execute(i);
            }

            return default;
        }
    }

}