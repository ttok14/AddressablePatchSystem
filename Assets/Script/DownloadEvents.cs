using System;

/// <summary> �ٿ�ε� ���� �̺�Ʈ ���� </summary>
public class DownloadEvents
{
    // �ý��� �ʱ�ȭ
    public event Action SystemInitializedListener;
    public void NotifyInitialized() => SystemInitializedListener?.Invoke();

    // Catalog ������Ʈ �Ϸ� 
    public event Action CatalogUpdatedListener;
    public void NotifyCatalogUpdated() => CatalogUpdatedListener?.Invoke();

    // Size �ٿ�ε� �Ϸ� 
    public event Action<long> SizeDownloadedListener;
    public void NotifySizeDownloaded(long size) => SizeDownloadedListener?.Invoke(size);

    // �ٿ�ε� ����
    public event Action<DownloadProgressStatus> DownloadProgressListener;
    public void NotifyDownloadProgress(DownloadProgressStatus status) => DownloadProgressListener?.Invoke(status);

    // Bundle �ٿ�ε� �Ϸ�
    public event Action<bool> DownloadFinished;
    public void NotifyDownloadFinished(bool isSuccess) => DownloadFinished?.Invoke(isSuccess);
}