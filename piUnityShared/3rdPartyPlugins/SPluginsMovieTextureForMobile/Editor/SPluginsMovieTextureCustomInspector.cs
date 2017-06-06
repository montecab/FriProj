using UnityEditor;
using System;
using System.Collections.Generic;
using SPlugins.MovieTexture;


[CustomEditor(typeof(SPluginsMovieTexture))]
public class SPluginsMovieTextureCustomInspector : Editor
{
	public override void OnInspectorGUI()
	{
		if (null == this._customInspector)
		{
			this._customInspector = new MovieTextureCustomInspector();
		}
		SPluginsMovieTexture movieTexture = base.target as SPluginsMovieTexture;
		if (null != movieTexture )
		{
			this._customInspector.InspectorGUI(this, ref movieTexture.movieTextureObject);
		}
		
	}
	private MovieTextureCustomInspector _customInspector = null;
}
