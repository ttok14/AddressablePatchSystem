using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopup : MonoBehaviour
{
	/// <summary> �˾� ���� </summary>
	public enum State
	{
		None = 0,

		CalculatingSize,		   /* �ٿ�ε� ���� �������� �����ϴ��� Ȯ�� */
		NothingToDownload,	/* �ٿ�ε� ���� �� ���� */

		AskingDownload,		    /* �ٿ�ε� ������ ��� */
		Downloading,               /* �ٿ�ε� ������ */
		DownloadFinished        /* �ٿ�ε� �� */
	}

	/// <summary> ���º� ���ӿ�����Ʈ Ȱ��ȭ ���ۿ� </summary>
	[Serializable]
	public class Root
	{
		public State state;
		public Transform root;
	}

	/// <summary> ���º� ��Ʈ ���ӿ�����Ʈ�� </summary>
	[SerializeField]
	List<Root> roots;

	/// <summary> �˾� Ÿ��Ʋ �ؽ�Ʈ </summary>
	[SerializeField]
	Text txtTitle;

	/// <summary> �˾� ���� ���� �ؽ�Ʈ </summary>
	[SerializeField]
	Text txtDesc;

	/// <summary> �ٿ�ε� ���� ���α׷����ٿ� ��ġ�� �ؽ�Ʈ </summary>
	[SerializeField]
	Text downloadingBarStatus;

	/// <summary> �ٿ�ε� ���α׷��� �� </summary>
	[SerializeField]
	Slider downloadProgressBar;

	/// <summary> �ٿ�ε� ��Ʈ�ѷ� - �ٿ�ε� ���� �۾� ���� </summary>
	[SerializeField]
	DownloadController Downloader;

	/// <summary> �ٿ�ε� ������ ������  </summary>
	DownloadProgressStatus progressInfo;
	/// <summary> �ٿ�ε� ������ ���� </summary>
	SizeUnits sizeUnit;
	/// <summary> ���� ��ȯ�� ���� �ٿ�ε�� ������ </summary>
	long curDownloadedSizeInUnit;
	/// <summary> ���� ��ȯ�� �� ������ </summary>
	long totalSizeInUnit;

	/// <summary> ���� ���� </summary>
	public State CurrentState { get; private set; } = State.None;

	/// <summary> �ٿ�ε� ��ƾ ���� , �� ���۽� ȣ�� </summary>
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

	/// <summary> �˾� ���� �����Ѵ� </summary>
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

	/// <summary> UI �� ������Ʈ�Ѵ� </summary>
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

	// ---------------------------------------------------------------------------------------------------- //

	/// <summary> �ٿ�ε� ���� ��ư Ŭ���� ȣ�� </summary>
	public void OnClickStartDownload()
	{
		Debug.Log("�ٿ�ε带 �����մϴ�");

		SetState(State.Downloading, true);
		Downloader.GoNext();
	}

	/// <summary> ��� ��ư Ŭ���� ȣ�� </summary>
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

	/// <summary> �ΰ��� ���� ��ư Ŭ���� ȣ�� </summary>
	public void OnClickEnterGame()
	{
		Debug.Log("Start Game!");

		UnityEngine.SceneManagement.SceneManager.LoadScene(1);
	}

	/// <summary> �ʱ�ȭ �Ϸ�� ȣ�� </summary>
	private void OnInitialized()
	{
		Downloader.GoNext();
	}

	/// <summary> īŻ�α� ������Ʈ �Ϸ�� ȣ�� </summary>
	private void OnCatalogUpdated()
	{
		Downloader.GoNext();
	}

	/// <summary> ������ �ٿ�ε� �Ϸ�� ȣ�� </summary>
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

	/// <summary> �ٿ�ε� ������ ȣ�� </summary>
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

	/// <summary> �ٿ�ε� �������� ȣ�� </summary>
	private void OnDownloadFinished(bool isSuccess)
	{
		Debug.Log("�ٿ�ε� �Ϸ� ! ��� : " + isSuccess);

		SetState(State.DownloadFinished, true);
		Downloader.GoNext();
	}
}
