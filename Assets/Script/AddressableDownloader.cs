using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary> Addressable �ٿ�ε� ���� Ŭ���� </summary>
public class AddressableDownloader
{
    /// <summary> �ٿ�ε� ���� URL </summary>
    ///     => Addressable Profile �ʿ��� LoadPath �� {AddressableDownloader.DownloadURL} ����
    public static string DownloadURL;

    /// <summary> �̺�Ʈ Ŭ���� </summary>
    DownloadEvents Events;

    /// <summary> �ٿ�ε� ���� Label </summary>
    string LabelToDownload;

    /// <summary> �ٿ�ε� ���� ��ü ������ </summary>
    long TotalSize;

    /// <summary> ���� �ٿ�ε� �ڵ� - �񵿱�� ����Ǵ� �ٿ�ε带 �� ������ ���� ���� ��Ȳ �˼����� </summary>
    AsyncOperationHandle DownloadHandle;

    /// <summary> Addressable �ý��� �ʱ�ȭ�ϱ� </summary>
    public DownloadEvents InitializedSystem(string label, string downloadURL)
    {
        Events = new DownloadEvents();

        Addressables.InitializeAsync().Completed += OnInitialized;

        DownloadURL = downloadURL;
        LabelToDownload = label;

        ResourceManager.ExceptionHandler += OnException;

        return Events;
    }

    /// <summary> īŻ�α� ������Ʈ�ϱ� </summary>
    public void UpdateCatalog()
    {
        Addressables.CheckForCatalogUpdates().Completed += (result) =>
        {
            var catalogToUpdate = result.Result;
            if (catalogToUpdate.Count > 0)
            {
                Addressables.UpdateCatalogs(catalogToUpdate).Completed += OnCatalogUpdated;
            }
            else
            {
                Events.NotifyCatalogUpdated();
            }
        };
    }

    /// <summary> ������ �ٿ�ε� �ϱ� </summary>
    public void DownloadSize()
    {
        Addressables.GetDownloadSizeAsync(LabelToDownload).Completed += OnSizeDownloaded;
    }

    /// <summary> ���� �ٿ�ε� �����ϱ� </summary>
    public void StartDownload()
    {
        DownloadHandle = Addressables.DownloadDependenciesAsync(LabelToDownload);
        DownloadHandle.Completed += OnDependenciesDownloaded;
    }

    /// <summary> �׽� ȣ��� ������Ʈ - ���� ��Ȳ ���� </summary>
    public void Update()
    {
        if (DownloadHandle.IsValid()
            && DownloadHandle.IsDone == false
            && DownloadHandle.Status != AsyncOperationStatus.Failed)
        {
            var status = DownloadHandle.GetDownloadStatus();

            long curDownloadedSize = status.DownloadedBytes;
            long remainedSize = TotalSize - curDownloadedSize;

            Events.NotifyDownloadProgress(new DownloadProgressStatus(
                status.DownloadedBytes
                , TotalSize
                , remainedSize
                , status.Percent));
        }
    }

    //--------------------------------------------------------//

    /// <summary> �ʱ�ȭ �Ϸ�� ȣ�� </summary>
    private void OnInitialized(AsyncOperationHandle<IResourceLocator> result)
    {
        Events.NotifyInitialized();
    }

    /// <summary> īŻ�α� ������Ʈ �Ϸ�� ȣ�� </summary>
    void OnCatalogUpdated(AsyncOperationHandle<List<IResourceLocator>> result)
    {
        Events.NotifyCatalogUpdated();
    }

    /// <summary> ������ �ٿ�ε� �Ϸ�� ȣ�� </summary>
    void OnSizeDownloaded(AsyncOperationHandle<long> result)
    {
        TotalSize = result.Result;
        Events.NotifySizeDownloaded(result.Result);
    }

    /// <summary> ���� �ٿ�ε� �Ϸ�� ȣ�� </summary>
    void OnDependenciesDownloaded(AsyncOperationHandle result)
    {
        Events.NotifyDownloadFinished(result.Status == AsyncOperationStatus.Succeeded);
    }

    /// <summary> ���� �߻��� ȣ�� </summary>
    void OnException(AsyncOperationHandle handle, Exception exception)
    {
        Debug.LogError(exception.Message);
    }
}
