using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopup : MonoBehaviour
{
	public enum State
	{
		None = 0,

		CalculatingSize,
		NothingToDownload,

		AskingDownload,
		Downloading,
		DownloadFinished
	}

	[Serializable]
	public class Root
	{
		public State state;
		public Transform root;
	}

	[SerializeField]
	List<Root> roots;

	[SerializeField]
	Text txtTitle;

	[SerializeField]
	Text txtDesc;

	[SerializeField]
	Text downloadingBarStatus;

	[SerializeField]
	Slider downloadProgressBar;

	[SerializeField]
	DownloadController Downloader;

	DownloadProgressStatus progressInfo;
	SizeUnits sizeUnit;
	long curDownloadedSizeInUnit;
	long totalSizeInUnit;

	public State CurrentState { get; private set; } = State.None;

	IEnumerator Start()
    {
		SetState(State.CalculatingSize, true);

		yield return Downloader.StartDownloadRoutine((events) =>
		{
			events.SystemInitializedListener += OnInitialized;
			events.CatalogUpdatedListener += OnCatalogUpdated;
			events.SizeDownloadedListener += OnSizeDownloaded;
			events.DownloadProgressListener += OnDownloadProgress;
			events.DownloadFinished += OnDownloadFinished;
		});
	}

	void SetState(State newState, bool updateUI)
	{
		var prevRoot = roots.Find(t => t.state == CurrentState);
		var newRoot = roots.Find(t => t.state == newState);

		CurrentState = newState;

		if (prevRoot != null)
		{
			prevRoot.root.gameObject.SetActive(false);
		}

		if (newRoot != null)
		{
			newRoot.root.gameObject.SetActive(true);
		}

		if (updateUI)
		{
			UpdateUI();
		}
	}

	void UpdateUI()
	{
		if (CurrentState == State.CalculatingSize)
		{
			txtTitle.text = "알림";
			txtDesc.text = "다운로드 정보를 가져오고 있습니다. 잠시만 기다려주세요.";
		}
		else if (CurrentState == State.NothingToDownload)
		{
			txtTitle.text = "완료";
			txtDesc.text = "다운로드 받을 데이터가 없습니다.";
		}
		else if (CurrentState == State.AskingDownload)
		{
			txtTitle.text = "주의";
			txtDesc.text = $"다운로드를 받으시겠습니까 ? 데이터가 많이 사용될 수 있습니다. <color=green>({$"{this.totalSizeInUnit}{this.sizeUnit})</color>"}";
		}
		else if (CurrentState == State.Downloading)
		{
			txtTitle.text = "다운로드중";
			txtDesc.text = $"다운로드중입니다. 잠시만 기다려주세요. {(progressInfo.totalProgress * 100).ToString("0.00")}% 완료";

			downloadProgressBar.value = progressInfo.totalProgress;
			downloadingBarStatus.text = $"{this.curDownloadedSizeInUnit}/{this.totalSizeInUnit}{sizeUnit}";
		}
		else if (CurrentState == State.DownloadFinished)
		{
			txtTitle.text = "완료";
			txtDesc.text = "다운로드가 완료되었습니다. 게임을 진행하시겠습니까?";
		}
	}

	public void OnClickStartDownload()
	{
		Debug.Log("다운로드를 시작합니다");

		SetState(State.Downloading, true);
		Downloader.GoNext();
	}

	public void OnClickCancelBtn()
	{
#if UNITY_EDITOR
		if (Application.isEditor)
		{
			UnityEditor.EditorApplication.isPlaying = false;
		}
#else
            Application.Quit();
#endif
	}

	public void OnClickEnterGame()
	{
		Debug.Log("Start Game!");

		UnityEngine.SceneManagement.SceneManager.LoadScene(1);
	}

	private void OnInitialized()
	{
		Downloader.GoNext();
	}

	private void OnCatalogUpdated()
	{
		Downloader.GoNext();
	}

	private void OnSizeDownloaded(long size)
	{
		Debug.Log($"다운로드 사이즈 다운로드 완료 ! : {size} 바이트");

		if (size == 0)
		{
			SetState(State.NothingToDownload, true);
		}
		else
		{
			sizeUnit = Utilities.GetProperByteUnit(size);
			totalSizeInUnit = Utilities.ConvertByteByUnit(size, sizeUnit);

			SetState(State.AskingDownload, true);
		}
	}

	private void OnDownloadProgress(DownloadProgressStatus newInfo)
	{
		bool changed = this.progressInfo.downloadedBytes != newInfo.downloadedBytes;

		progressInfo = newInfo;

		if (changed)
		{
			UpdateUI();

			curDownloadedSizeInUnit = Utilities.ConvertByteByUnit(newInfo.downloadedBytes, sizeUnit);
		}
	}

	private void OnDownloadFinished(bool isSuccess)
	{
		Debug.Log("다운로드 완료 ! 결과 : " + isSuccess);

		SetState(State.DownloadFinished, true);
		Downloader.GoNext();
	}
}
