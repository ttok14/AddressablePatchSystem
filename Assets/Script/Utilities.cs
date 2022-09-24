using UnityEngine;

/// <summary> �ٿ�ε� ������ ���� </summary>
public enum SizeUnits
{
    Byte, KB, MB, GB
}

public static class Utilities
{
    /// <summary> ��Ʈ��ũ�� ����Ǿ� �ִ°� ? </summary>
    public static bool IsNetworkValid()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }

    /// <summary> ���� Cache �� <paramref name="requiredSize"/> �̻��� ���� ������ �ִ°�? </summary>
    public static bool IsDiskSpaceEnough(long requiredSize)
    {
        return Caching.defaultCache.spaceFree >= requiredSize;
    }

    #region ====:: Size ���� ::====

    public static long OneGB = 1000000000;
    public static long OneMB = 1000000;
    public static long OneKB = 1000;

    /// <summary> ����Ʈ <paramref name="byteSize"/> ����� �°Բ� ������ ���� <see cref="SizeUnits"/> Ÿ���� �����´� </summary>
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

    /// <summary> ����Ʈ�� <paramref name="byteSize"/> <paramref name="unit"/> ������ �°� ���ڸ� ��ȯ�Ѵ� </summary>
    public static long ConvertByteByUnit(long byteSize, SizeUnits unit)
    {
        return (long)((byteSize / (double)System.Math.Pow(1024, (long)unit)));
    }

    /// <summary> ����Ʈ�� <paramref name="byteSize"/> ������ �Բ� ����� ������ ���ڿ� ���·� ��ȯ�Ѵ� </summary>
    public static string GetConvertedByteString(long byteSize, SizeUnits unit, bool appendUnit = true)
    {
        string unitStr = appendUnit ? unit.ToString() : string.Empty;
        return $"{ConvertByteByUnit(byteSize, unit).ToString("0.00")}{unitStr}";
    }

    #endregion
}
