using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DownloadController : MonoBehaviour
{
	AddressableDownloader Downloader;

	[SerializeField]
	string LabelToDownload;
	[SerializeField]
	string DownloadURL;

	public State CurrentState { get; private set; } = State.Idle;
	State LastValidState = State.Idle;

	Action<DownloadEvents> OnEventObtained;

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
}