using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExampleMainNoneGUI : MonoBehaviour
{
	void Start()
	{
		if (null != movieTexture)
		{
			//movieTexture.LoadAsync("Example/SampleMovie.mp4", HandleOnLoadCompleted);
			movieTexture.LoadAsync(HandleOnLoadCompleted);
		}
	}
	void Update()
	{
		if (null != movieTexture)
		{
			if (true == this._canPlay && false == movieTexture.IsPlaying())
			{
				movieTexture.SetVolume(1f);
				movieTexture.Play(HandleOnPlayCompleted);
			}
		}
	}
	void OnDestroy()
	{
		if (null != movieTexture)
		{
			movieTexture.Stop();
		}
	}
	void HandleOnLoadCompleted(SPlugins.MovieTexture.AbstractMovieTextureComponent.ResultType resultType_,
		string resultDescription_)
	{
		if (SPlugins.MovieTexture.AbstractMovieTextureComponent.ResultType.SUCCESS == resultType_)
		{
			this._canPlay = true;
		}
		else
		{
			Debug.LogError(string.Format("Result:{0} - {1}", resultType_.ToString(), resultDescription_));
		}
	}
	void HandleOnPlayCompleted(SPlugins.MovieTexture.AbstractMovieTextureComponent.CompletedType completedType_)
	{
		this._canPlay = false;
		if (SPlugins.MovieTexture.AbstractMovieTextureComponent.CompletedType.PLAYBACK_COMPLETION == completedType_)
		{
			if (null != movieTexture)
			{
				movieTexture.Stop();
			}
		}
	}

	public SPluginsMovieTexture movieTexture = null;
	private bool _canPlay = false;
}
