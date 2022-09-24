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
			txtTitle.text = "�˸�";
			txtDesc.text = "�ٿ�ε� ������ �������� �ֽ��ϴ�. ��ø� ��ٷ��ּ���.";
		}
		else if (CurrentState == State.NothingToDownload)
		{
			txtTitle.text = "�Ϸ�";
			txtDesc.text = "�ٿ�ε� ���� �����Ͱ� �����ϴ�.";
		}
		else if (CurrentState == State.AskingDownload)
		{
			txtTitle.text = "����";
			txtDesc.text = $"�ٿ�ε带 �����ðڽ��ϱ� ? �����Ͱ� ���� ���� �� �ֽ��ϴ�. <color=green>({$"{this.totalSizeInUnit}{this.sizeUnit})</color>"}";
		}
		else if (CurrentState == State.Downloading)
		{
			txtTitle.text = "�ٿ�ε���";
			txtDesc.text = $"�ٿ�ε����Դϴ�. ��ø� ��ٷ��ּ���. {(progressInfo.totalProgress * 100).ToString("0.00")}% �Ϸ�";

			downloadProgressBar.value = progressInfo.totalProgress;
			downloadingBarStatus.text = $"{this.curDownloadedSizeInUnit}/{this.totalSizeInUnit}{sizeUnit}";
		}
		else if (CurrentState == State.DownloadFinished)
		{
			txtTitle.text = "�Ϸ�";
			txtDesc.text = "�ٿ�ε尡 �Ϸ�Ǿ����ϴ�. ������ �����Ͻðڽ��ϱ�?";
		}
	}

	public void OnClickStartDownload()
	{
		Debug.Log("�ٿ�ε带 �����մϴ�");

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
		Debug.Log($"�ٿ�ε� ������ �ٿ�ε� �Ϸ� ! : {size} ����Ʈ");

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
		Debug.Log("�ٿ�ε� �Ϸ� ! ��� : " + isSuccess);

		SetState(State.DownloadFinished, true);
		Downloader.GoNext();
	}
}
