using UnityEngine.Profiling;

public sealed class UnityMemoryChecker
{
    public float Used { get; private set; }
    public float Unused { get; private set; }
    public float Total { get; private set; }

    public string UsedText => Used.ToString( "0.0" ) + " MB";
    public string UnusedText => Unused.ToString( "0.0" ) + " MB";
    public string TotalText => Total.ToString( "0.0" ) + " MB";

    public void Update()
    {
        // Unity によって割り当てられたメモリ
        Used = ( Profiler.GetTotalAllocatedMemoryLong() >> 10 ) / 1024f;

        // 予約済みだが割り当てられていないメモリ
        Unused = ( Profiler.GetTotalUnusedReservedMemoryLong() >> 10 ) / 1024f;

        // Unity が現在および将来の割り当てのために確保している総メモリ
        Total = ( Profiler.GetTotalReservedMemoryLong() >> 10 ) / 1024f;
    }
}