using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadController : MonoBehaviour
{
	/// <summary> 상태 타입 </summary>
	public enum State
	{
		Idle,

		Initialize,
		UpdateCatalog,
		DownloadSize,
		DownloadDependencies,
		Downloading,

		Finished
	}

	/// <summary> 다운로드 클래스 </summary>
	AddressableDownloader Downloader;

	/// <summary> 다운로드 받을 Label </summary>
	[SerializeField]
	string LabelToDownload;
	/// <summary> 다운로드 받을 URL </summary>
	[SerializeField]
	string DownloadURL;

	/// <summary> 현재 상태 </summary>
	public State CurrentState { get; private set; } = State.Idle;
	/// <summary> 마지막 상태 저장. 다음으로 넘기기 위함, Idle 상태는 무시 </summary>
	State LastValidState = State.Idle;

	/// <summary> 이벤트 클래스 인스턴스 전달용 </summary>
	Action<DownloadEvents> OnEventObtained;

	/// <summary> 루틴 시작 함수 </summary>
	public IEnumerator StartDownloadRoutine(Action<DownloadEvents> onEventObtained)
	{
		this.Downloader = new AddressableDownloader();
		OnEventObtained = onEventObtained;

		LastValidState = CurrentState = State.Initialize;

		while (CurrentState != State.Finished)
		{
			OnExecute();
			yield return null;
		}
	}

	/// <summary> 다음 작업 수행 </summary>
	public void GoNext()
	{
		if (LastValidState == State.Initialize)
		{
			CurrentState = State.UpdateCatalog;
		}
		else if (LastValidState == State.UpdateCatalog)
		{
			CurrentState = State.DownloadSize;
		}
		else if (LastValidState == State.DownloadSize)
		{
			CurrentState = State.DownloadDependencies;
		}
		else if (LastValidState == State.Downloading || LastValidState == State.DownloadDependencies)
		{
			CurrentState = State.Finished;
		}

		LastValidState = CurrentState;
	}

	/// <summary> 현재 상태에 따라 분기 처리 - 항시 호출 </summary>
	void OnExecute()
	{
		if (CurrentState == State.Idle)
		{
			return;
		}

		if (CurrentState == State.Initialize)
		{
			var events = Downloader.InitializedSystem(this.LabelToDownload, this.DownloadURL);
			OnEventObtained?.Invoke(events);

			CurrentState = State.Idle;
		}
		else if (CurrentState == State.UpdateCatalog)
		{
			Downloader.UpdateCatalog();
			CurrentState = State.Idle;
		}
		else if (CurrentState == State.DownloadSize)
		{
			Downloader.DownloadSize();
			CurrentState = State.Idle;
		}
		else if (CurrentState == State.DownloadDependencies)
		{
			Downloader.StartDownload();
			CurrentState = State.Downloading;
		}
		else if (CurrentState == State.Downloading)
		{
			Downloader.Update();
		}
	}
}