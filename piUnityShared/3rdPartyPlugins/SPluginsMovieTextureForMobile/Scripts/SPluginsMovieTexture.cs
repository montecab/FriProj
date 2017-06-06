using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SPlugins.MovieTexture;
using SPlugins;

public class SPluginsMovieTexture : AbstractMovieTextureComponent
{
	protected override SPlugins.MovieTexture.IJavaObject InstanceJavaObject()
	{
		return new SPluginsAndroidJavaObjectImpl();
	}
	protected override IEditorMovieTextureProxy InstanceEditorMovieTexture()
	{
		return new SPluginsEditorMovieTextureFacadeImpl();
	}
	protected override int GetNativeMovieTexutreID()
	{
#if (!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR
		UnityEngine.MovieTexture movieTexture = movieTextureObject as UnityEngine.MovieTexture;
		if( null != movieTexture )
		{
			return movieTexture.GetNativeTextureID();
		}
#endif
		return -1;
	}
#if (!UNITY_ANDROID && !UNITY_IPHONE) || UNITY_EDITOR
	public UnityEngine.Object movieTextureObject = null;
#endif
}
