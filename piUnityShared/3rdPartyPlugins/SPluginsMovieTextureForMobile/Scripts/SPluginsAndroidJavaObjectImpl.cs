using System;
using UnityEngine;


public class SPluginsAndroidJavaObjectImpl : SPlugins.MovieTexture.IJavaObject
{
#if UNITY_ANDROID
	public void Initialize(string className_, params object[] args_)
	{
		this._javaObject = new AndroidJavaObject(className_, args_);
	}
	public ReturnType Call<ReturnType>(string methodName_, params object[] args_)
	{
		return this._javaObject.Call<ReturnType>(methodName_, args_);
	}
	public void Call(string methodName_, params object[] args_)
	{
		this._javaObject.Call(methodName_, args_);
	}
	public void CallStatic(string methodName_, params object[] args_)
	{
		this._javaObject.CallStatic(methodName_, args_);
	}
	public ReturnType CallStatic<ReturnType>(string methodName_, params object[] args_)
	{
		return this._javaObject.CallStatic<ReturnType>(methodName_, args_);
	}
	public void Dispose()
	{
		this._javaObject.Dispose();
	}
#else
	public void Initialize(string className_, params object[] args_) { throw new NotImplementedException(); }
	public ReturnType Call<ReturnType>(string methodName_, params object[] args_) { throw new NotImplementedException(); }
	public void Call(string methodName_, params object[] args_) { throw new NotImplementedException(); }
	public void CallStatic(string methodName_, params object[] args_) { throw new NotImplementedException(); }
	public ReturnType CallStatic<ReturnType>(string methodName_, params object[] args_) { throw new NotImplementedException(); }
	public void Dispose() { throw new NotImplementedException(); }
#endif

#if UNITY_ANDROID
	private AndroidJavaObject _javaObject = null;
#endif
}

