using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> 다운로드 진행 상황 정보 </summary>
public struct DownloadProgressStatus
{
    public long downloadedBytes;      /* 다운로드된 바이트 사이즈 */
    public long totalBytes;                   /* 다운로드 받을 전체 사이즈 */
    public long remainedBytes;            /* 남은 바이트 사이즈 */
    public float totalProgress;               /* 전체 진행률 0 ~ 1 */

    public DownloadProgressStatus(long downloadedBytes, long totalBytes, long remainedBytes, float totalProgress)
    {
        this.downloadedBytes = downloadedBytes;
        this.totalBytes = totalBytes;
        this.remainedBytes = remainedBytes;
        this.totalProgress = totalProgress;
    }
}