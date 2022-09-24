using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> �ٿ�ε� ���� ��Ȳ ���� </summary>
public struct DownloadProgressStatus
{
    public long downloadedBytes;      /* �ٿ�ε�� ����Ʈ ������ */
    public long totalBytes;                   /* �ٿ�ε� ���� ��ü ������ */
    public long remainedBytes;            /* ���� ����Ʈ ������ */
    public float totalProgress;               /* ��ü ����� 0 ~ 1 */

    public DownloadProgressStatus(long downloadedBytes, long totalBytes, long remainedBytes, float totalProgress)
    {
        this.downloadedBytes = downloadedBytes;
        this.totalBytes = totalBytes;
        this.remainedBytes = remainedBytes;
        this.totalProgress = totalProgress;
    }
}