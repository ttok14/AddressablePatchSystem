using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadPopup : MonoBehaviour
{
	/// <summary> 팝업 상태 </summary>
	public enum State
	{
		None = 0,

		CalculatingSize,		   /* 다운로드 받을 컨텐츠가 존재하는지 확인 */
		NothingToDownload,	/* 다운로드 받을 게 없음 */

		AskingDownload,		    /* 다운로드 받을지 물어봄 */
		Downloading,               /* 다운로드 진행중 */
		DownloadFinished        /* 다운로드 끝 */
	}

	/// <summary> 상태별 게임오브젝트 활성화 조작용 </summary>
	[Serializable]
	public class Root
	{
		public State state;
		public Transform root;
	}

	/// <summary> 상태별 루트 게임오브젝트들 </summary>
	[SerializeField]
	List<Root> roots;

	/// <summary> 팝업 타이틀 텍스트 </summary>
	[SerializeField]
	Text txtTitle;

	/// <summary> 팝업 상태 설명 텍스트 </summary>
	[SerializeField]
	Text txtDesc;

	/// <summary> 다운로드 진행 프로그레스바에 위치한 텍스트 </summary>
	[SerializeField]
	Text downloadingBarStatus;

	/// <summary> 다운로드 프로그레스 바 </summary>
	[SerializeField]
	Slider downloadProgressBar;

	/// <summary> 다운로드 컨트롤러 - 다운로드 관련 작업 수행 </summary>
	[SerializeField]
	DownloadController Downloader;

	/// <summary> 다운로드 진행중 데이터  </summary>
	DownloadProgressStatus progressInfo;
	/// <summary> 다운로드 사이즈 단위 </summary>
	SizeUnits sizeUnit;
	/// <summary> 단위 변환된 현재 다운로드된 사이즈 </summary>
	long curDownloadedSizeInUnit;
	/// <summary> 단위 변환된 총 사이즈 </summary>
	long totalSizeInUnit;

	/// <summary> 현재 상태 </summary>
	public State CurrentState { get; private set; } = State.None;

	/// <summary> 다운로드 루틴 시작 , 씬 시작시 호출 </summary>
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

	/// <summary> 팝업 상태 변경한다 </summary>
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

	/// <summary> UI 를 업데이트한다 </summary>
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

	// ---------------------------------------------------------------------------------------------------- //

	/// <summary> 다운로드 시작 버튼 클릭시 호출 </summary>
	public void OnClickStartDownload()
	{
		Debug.Log("다운로드를 시작합니다");

		SetState(State.Downloading, true);
		Downloader.GoNext();
	}

	/// <summary> 취소 버튼 클릭시 호출 </summary>
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

	/// <summary> 인게임 진입 버튼 클릭시 호출 </summary>
	public void OnClickEnterGame()
	{
		Debug.Log("Start Game!");

		UnityEngine.SceneManagement.SceneManager.LoadScene(1);
	}

	/// <summary> 초기화 완료시 호출 </summary>
	private void OnInitialized()
	{
		Downloader.GoNext();
	}

	/// <summary> 카탈로그 업데이트 완료시 호출 </summary>
	private void OnCatalogUpdated()
	{
		Downloader.GoNext();
	}

	/// <summary> 사이즈 다운로드 완료시 호출 </summary>
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

	/// <summary> 다운로드 진행중 호출 </summary>
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

	/// <summary> 다운로드 마무리시 호출 </summary>
	private void OnDownloadFinished(bool isSuccess)
	{
		Debug.Log("다운로드 완료 ! 결과 : " + isSuccess);

		SetState(State.DownloadFinished, true);
		Downloader.GoNext();
	}
}
