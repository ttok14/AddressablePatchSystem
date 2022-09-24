using UnityEngine;

/// <summary> 다운로드 사이즈 단위 </summary>
public enum SizeUnits
{
    Byte, KB, MB, GB
}

public static class Utilities
{
    /// <summary> 네트워크가 연결되어 있는가 ? </summary>
    public static bool IsNetworkValid()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary> 현재 Cache 에 <paramref name="requiredSize"/> 이상의 여유 공간이 있는가? </summary>
    public static bool IsDiskSpaceEnough(long requiredSize)
    {
        return Caching.defaultCache.spaceFree >= requiredSize;
    }

    #region ====:: Size 관련 ::====

    public static long OneGB = 1000000000;
    public static long OneMB = 1000000;
    public static long OneKB = 1000;

    /// <summary> 바이트 <paramref name="byteSize"/> 사이즈에 맞게끔 적절한 단위 <see cref="SizeUnits"/> 타입을 가져온다 </summary>
    public static SizeUnits GetProperByteUnit(long byteSize)
    {
        if (byteSize >= OneGB)
            return SizeUnits.GB;
        else if (byteSize >= OneMB)
            return SizeUnits.MB;
        else if (byteSize >= OneKB)
            return SizeUnits.KB;
        return SizeUnits.Byte;
    }

    /// <summary> 바이트를 <paramref name="byteSize"/> <paramref name="unit"/> 단위에 맞게 숫자를 변환한다 </summary>
    public static long ConvertByteByUnit(long byteSize, SizeUnits unit)
    {
        return (long)((byteSize / (double)System.Math.Pow(1024, (long)unit)));
    }

    /// <summary> 바이트를 <paramref name="byteSize"/> 단위와 함께 출력이 가능한 문자열 형태로 변환한다 </summary>
    public static string GetConvertedByteString(long byteSize, SizeUnits unit, bool appendUnit = true)
    {
        string unitStr = appendUnit ? unit.ToString() : string.Empty;
        return $"{ConvertByteByUnit(byteSize, unit).ToString("0.00")}{unitStr}";
    }

    #endregion
}
