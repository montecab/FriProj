using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SPlugins.MovieTexture;


public class SPluginsEditorMovieTextureFacadeImpl : IEditorMovieTextureProxy
{
	private class SEditorMovieTexture
	{
		public SEditorMovieTexture()
		{
			this.nativeTextureID = -1;
			this.mediaState = MediaState.IDLE;
			this.audioSource = null;
			this.ownedMovieTextureComponent = null;
			this.isLooping = false;
			this.normalizeVolume = 1f;
			this.delegateLoadComplete = null;
			this.delegatePlayComplete = null;
#if (!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR
			this.movieTexture = null;
#endif
		}
		public int nativeTextureID { get; set; }
		public MediaState mediaState { get; set; }
		public UnityEngine.AudioSource audioSource { get; set; }
		public AbstractMovieTextureComponent ownedMovieTextureComponent { get; set; }
		public bool isLooping { get; set; }
		public float normalizeVolume { get; set; }
		public Action<int /*nativeTextureID*/, AbstractMovieTextureComponent.ResultType, string/*description*/> delegateLoadComplete { get; set; }
		public Action<int/*nativeTextureID*/, AbstractMovieTextureComponent.CompletedType> delegatePlayComplete { get; set; }
#if (!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR
		public UnityEngine.MovieTexture movieTexture { get; set; }
#endif

	}
	private enum MediaState
	{
		IDLE = 0,
		PREPARING,
		PREPARED,
		STARTED,
		PAUSED,
		STOPPED,
		PLAYBACK_COMPLETED,
	}

#if (!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR
	public void BindRenderTarget(int nativeTextureID_, AbstractMovieTextureComponent ownedMovieTextureComponent_, Material targetMaterial_)
	{
		if (null == targetMaterial_)
			return;

		if( true == this._editorMovieTextureDic.ContainsKey(nativeTextureID_) )
		{
			this._editorMovieTextureDic.Remove(nativeTextureID_);
		}

		SPluginsMovieTexture spluginsMovieTexture = ownedMovieTextureComponent_ as SPluginsMovieTexture;
		if (null == spluginsMovieTexture)
			return;

		UnityEngine.MovieTexture movieTexture = spluginsMovieTexture.movieTextureObject as UnityEngine.MovieTexture;
		if (null == movieTexture )
			return;

		targetMaterial_.mainTexture = movieTexture;

		SEditorMovieTexture editorMovieTexture = new SEditorMovieTexture();
		editorMovieTexture.nativeTextureID = nativeTextureID_;
		editorMovieTexture.movieTexture = movieTexture;
		editorMovieTexture.ownedMovieTextureComponent = ownedMovieTextureComponent_;
		editorMovieTexture.mediaState = MediaState.IDLE;
		if( null != editorMovieTexture.ownedMovieTextureComponent )
		{
			editorMovieTexture.audioSource = editorMovieTexture.ownedMovieTextureComponent.GetComponent<AudioSource>();
			if (null == editorMovieTexture.audioSource)
			{
				editorMovieTexture.audioSource = editorMovieTexture.ownedMovieTextureComponent.gameObject.AddComponent<AudioSource>();
			}
		}
		this._editorMovieTextureDic.Add(nativeTextureID_, editorMovieTexture);
	}
	public void LoadAsync(int nativeTextureID_,
				Action<int /*nativeTextureID*/, AbstractMovieTextureComponent.ResultType, string/*description*/> delegate_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if( true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture) )
		{
			editorMovieTexture.mediaState = MediaState.PREPARING;
			editorMovieTexture.delegateLoadComplete = delegate_;
			if (null != editorMovieTexture.ownedMovieTextureComponent)
			{
				editorMovieTexture.ownedMovieTextureComponent.StartCoroutine(_LoadAsync(editorMovieTexture));
			}
		}
		else if (null != delegate_)
		{
			delegate_(nativeTextureID_, AbstractMovieTextureComponent.ResultType.FAILED_FILE_NOT_EXIST, "MovieTexture is None");
		}	
	}
	private IEnumerator _LoadAsync(SEditorMovieTexture editorMovieTexture_)
	{
		yield return null;

		if (null != editorMovieTexture_.delegateLoadComplete)
		{
			editorMovieTexture_.delegateLoadComplete(editorMovieTexture_.nativeTextureID, AbstractMovieTextureComponent.ResultType.SUCCESS, "");
		}
		editorMovieTexture_.mediaState = MediaState.PREPARED;
	}
	public void Play(int nativeTextureID_, Action<int/*nativeTextureID*/, AbstractMovieTextureComponent.CompletedType> delegate_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			editorMovieTexture.delegatePlayComplete = delegate_;
			if (null != editorMovieTexture.movieTexture)
			{
				editorMovieTexture.movieTexture.loop = editorMovieTexture.isLooping;
				editorMovieTexture.movieTexture.Play();
				if (null != editorMovieTexture.audioSource)
				{
					editorMovieTexture.audioSource.clip = editorMovieTexture.movieTexture.audioClip;
					editorMovieTexture.audioSource.volume = editorMovieTexture.normalizeVolume;
					editorMovieTexture.audioSource.Play();
				}
				editorMovieTexture.mediaState = MediaState.STARTED;
				if (null != editorMovieTexture.ownedMovieTextureComponent)
				{
					editorMovieTexture.ownedMovieTextureComponent.StartCoroutine(_CoroutineUpdatePlaying(editorMovieTexture));
				}
			}
		}
		
	}
	private IEnumerator _CoroutineUpdatePlaying(SEditorMovieTexture editorMovieTexture_)
	{
		while (true)
		{
			if (MediaState.STARTED == editorMovieTexture_.mediaState && null != editorMovieTexture_.movieTexture && false == editorMovieTexture_.movieTexture.isPlaying)
			{
				if (null != editorMovieTexture_.delegatePlayComplete)
				{
					editorMovieTexture_.delegatePlayComplete(editorMovieTexture_.nativeTextureID, AbstractMovieTextureComponent.CompletedType.PLAYBACK_COMPLETION);
				}
			}

			if (MediaState.STOPPED == editorMovieTexture_.mediaState)
			{
				break;
			}
			yield return null;
		}
	}
	public void Stop(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			if (null != editorMovieTexture.movieTexture)
			{
				editorMovieTexture.movieTexture.Stop();
			}
			if (null != editorMovieTexture.audioSource)
			{
				editorMovieTexture.audioSource.Stop();
			}
			editorMovieTexture.mediaState = MediaState.STOPPED;
			if (null != editorMovieTexture.delegatePlayComplete)
			{
				editorMovieTexture.delegatePlayComplete(editorMovieTexture.nativeTextureID, AbstractMovieTextureComponent.CompletedType.FORCE_STOP);
			}
		}
		
	}
	public void Pause(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			if (null != editorMovieTexture.movieTexture)
			{
				editorMovieTexture.movieTexture.Pause();
			}
			if (null != editorMovieTexture.audioSource)
			{
				editorMovieTexture.audioSource.Pause();
			}
			editorMovieTexture.mediaState = MediaState.PAUSED;
		}
	}
	public void Resume(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			if (null != editorMovieTexture.movieTexture)
			{
				editorMovieTexture.movieTexture.Play();
				if (null != editorMovieTexture.audioSource)
				{
					editorMovieTexture.audioSource.clip = editorMovieTexture.movieTexture.audioClip;
					editorMovieTexture.audioSource.Play();
				}
				editorMovieTexture.mediaState = MediaState.STARTED;
			}
		}
	}
	public void SetVolume(int nativeTextureID_, float normalizeVolume_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			editorMovieTexture.normalizeVolume = normalizeVolume_;
			if (null != editorMovieTexture.audioSource)
			{
				editorMovieTexture.audioSource.volume = editorMovieTexture.normalizeVolume;
			}
		}
		
	}
	public void SetLooping(int nativeTextureID_, bool loop_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			editorMovieTexture.isLooping = loop_;
			if (null != editorMovieTexture.movieTexture)
			{
				editorMovieTexture.movieTexture.loop = editorMovieTexture.isLooping;
			}
		}
	}
	public int GetDuration(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			if (null != editorMovieTexture.audioSource && null != editorMovieTexture.audioSource.clip)
			{
				return (int)(editorMovieTexture.audioSource.clip.length * 1000f);
			}
			if (null != editorMovieTexture.movieTexture)
			{
				return (int)(editorMovieTexture.movieTexture.duration * 1000f);
			}
		}
		
		return 0;
	}
	public int GetCurrentPosition(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			if (null != editorMovieTexture.audioSource)
			{
				int currentPosition = (int)(editorMovieTexture.audioSource.time * 1000f);
				int duration = GetDuration(nativeTextureID_);
				if (currentPosition > duration)
				{
					currentPosition = currentPosition % duration;
				}
				return currentPosition;
			}
		}
		
		return 0;
	}
	public bool IsLooping(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			return editorMovieTexture.isLooping;
		}
		return false;
	}
	public bool IsPlaying(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			return editorMovieTexture.mediaState == MediaState.STARTED ? true : false;
		}
		return false;
	}
	public bool IsPaused(int nativeTextureID_)
	{
		SEditorMovieTexture editorMovieTexture = null;
		if (true == this._editorMovieTextureDic.TryGetValue(nativeTextureID_, out editorMovieTexture))
		{
			return editorMovieTexture.mediaState == MediaState.PAUSED ? true : false;
		}
		return false;
	}
#else
	public void BindRenderTarget(int nativeTextureID_, AbstractMovieTextureComponent ownedMovieTextureComponent_, Material targetMaterial_)	
	{ throw new NotImplementedException(); }
	public void LoadAsync(int nativeTextureID_,
				Action<int /*nativeTextureID*/, AbstractMovieTextureComponent.ResultType, string/*description*/> delegate_)
	{ throw new NotImplementedException(); }
	public void Play(int nativeTextureID_, Action<int/*nativeTextureID*/, AbstractMovieTextureComponent.CompletedType> delegate_)
	{ throw new NotImplementedException(); }
	public void Stop(int nativeTextureID_)	{ throw new NotImplementedException(); }
	public void Pause(int nativeTextureID_)	{ throw new NotImplementedException(); }
	public void Resume(int nativeTextureID_){ throw new NotImplementedException(); }
	
	public int GetDuration(int nativeTextureID_)	{ throw new NotImplementedException(); }
	public int GetCurrentPosition(int nativeTextureID_){ throw new NotImplementedException(); }
	public bool IsLooping(int nativeTextureID_){ throw new NotImplementedException(); }
	public bool IsPlaying(int nativeTextureID_){ throw new NotImplementedException(); }
	public bool IsPaused(int nativeTextureID_){ throw new NotImplementedException(); }
	public void SetVolume(int nativeTextureID_, float normalizeVolume_){ throw new NotImplementedException(); }
	public void SetLooping(int nativeTextureID_, bool loop_){ throw new NotImplementedException(); }
#endif

	private Dictionary<int/*nativeTextureID*/, SEditorMovieTexture> _editorMovieTextureDic = new Dictionary<int, SEditorMovieTexture>();
}

