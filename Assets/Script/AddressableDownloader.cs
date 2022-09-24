using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary> Addressable 다운로드 수행 클래스 </summary>
public class AddressableDownloader
{
    /// <summary> 다운로드 받을 URL </summary>
    ///     => Addressable Profile 쪽에서 LoadPath 에 {AddressableDownloader.DownloadURL} 설정
    public static string DownloadURL;

    /// <summary> 이벤트 클래스 </summary>
    DownloadEvents Events;

    /// <summary> 다운로드 받을 Label </summary>
    string LabelToDownload;

    /// <summary> 다운로드 받을 전체 사이즈 </summary>
    long TotalSize;

    /// <summary> 번들 다운로드 핸들 - 비동기로 진행되는 다운로드를 이 변수를 통해 진행 상황 알수있음 </summary>
    AsyncOperationHandle DownloadHandle;

    /// <summary> Addressable 시스템 초기화하기 </summary>
    public DownloadEvents InitializedSystem(string label, string downloadURL)
    {
        Events = new DownloadEvents();

        Addressables.InitializeAsync().Completed += OnInitialized;

        DownloadURL = downloadURL;
        LabelToDownload = label;

        ResourceManager.ExceptionHandler += OnException;

        return Events;
    }

    /// <summary> 카탈로그 업데이트하기 </summary>
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

    /// <summary> 사이즈 다운로드 하기 </summary>
    public void DownloadSize()
    {
        Addressables.GetDownloadSizeAsync(LabelToDownload).Completed += OnSizeDownloaded;
    }

    /// <summary> 번들 다운로드 시작하기 </summary>
    public void StartDownload()
    {
        DownloadHandle = Addressables.DownloadDependenciesAsync(LabelToDownload);
        DownloadHandle.Completed += OnDependenciesDownloaded;
    }

    /// <summary> 항시 호출되 업데이트 - 진행 상황 보고 </summary>
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

    /// <summary> 초기화 완료시 호출 </summary>
    private void OnInitialized(AsyncOperationHandle<IResourceLocator> result)
    {
        Events.NotifyInitialized();
    }

    /// <summary> 카탈로그 업데이트 완료시 호출 </summary>
    void OnCatalogUpdated(AsyncOperationHandle<List<IResourceLocator>> result)
    {
        Events.NotifyCatalogUpdated();
    }

    /// <summary> 사이즈 다운로드 완료시 호출 </summary>
    void OnSizeDownloaded(AsyncOperationHandle<long> result)
    {
        TotalSize = result.Result;
        Events.NotifySizeDownloaded(result.Result);
    }

    /// <summary> 번들 다운로드 완료시 호출 </summary>
    void OnDependenciesDownloaded(AsyncOperationHandle result)
    {
        Events.NotifyDownloadFinished(result.Status == AsyncOperationStatus.Succeeded);
    }

    /// <summary> 예외 발생시 호출 </summary>
    void OnException(AsyncOperationHandle handle, Exception exception)
    {
        Debug.LogError(exception.Message);
    }
}
