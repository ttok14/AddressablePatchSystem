using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableDownloader
{
    public static string DownloadURL;

    DownloadEvents Events;

    string LabelToDownload;

    long TotalSize;

    AsyncOperationHandle DownloadHandle;

    public DownloadEvents InitializedSystem(string label, string downloadURL)
    {
        Events = new DownloadEvents();

        Addressables.InitializeAsync().Completed += OnInitialized;

        DownloadURL = downloadURL;
        LabelToDownload = label;

        ResourceManager.ExceptionHandler += OnException;

        return Events;
    }

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

    public void DownloadSize()
    {
        Addressables.GetDownloadSizeAsync(LabelToDownload).Completed += OnSizeDownloaded;
    }

    public void StartDownload()
    {
        DownloadHandle = Addressables.DownloadDependenciesAsync(LabelToDownload);
        DownloadHandle.Completed += OnDependenciesDownloaded;
    }

    private void OnInitialized(AsyncOperationHandle<IResourceLocator> result)
    {
        Events.NotifyInitialized();
    }

    void OnCatalogUpdated(AsyncOperationHandle<List<IResourceLocator>> result)
    {
        Events.NotifyCatalogUpdated();
    }

    void OnSizeDownloaded(AsyncOperationHandle<long> result)
    {
        TotalSize = result.Result;
        Events.NotifySizeDownloaded(result.Result);
    }

    void OnDependenciesDownloaded(AsyncOperationHandle result)
    {
        Events.NotifyDownloadFinished(result.Status == AsyncOperationStatus.Succeeded);
    }

    void OnException(AsyncOperationHandle handle, Exception exception)
    {
        Debug.LogError(exception.Message);
    }
}
