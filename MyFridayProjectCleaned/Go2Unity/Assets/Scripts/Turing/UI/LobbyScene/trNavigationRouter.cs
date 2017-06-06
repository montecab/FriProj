using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Turing{
  public class trNavigationRouter : Singleton<trNavigationRouter>{

    public delegate void SceneEventHandler();
    public event SceneEventHandler onSceneLoaded;

    private Dictionary<string, string> transitionParametersDict = new Dictionary<string, string>();
    public bool IsLoading = false;

    public string GetTransitionParameterForScene() {
      string result = null;
      string sceneName = SceneManager.GetActiveScene().name;
      if (transitionParametersDict.ContainsKey(sceneName)){
        result = transitionParametersDict[sceneName];
      }
      return result;
    }

    public void ShowSceneWithName(string sceneName, string parameter = null, bool addToHistory=true) {
      if(!IsLoading){
        wwEntryExitTracker.DoExit();
        transitionParametersDict[sceneName] = parameter;
        StartCoroutine (LoadScene (sceneName));
      }
    }

    private IEnumerator LoadScene(string sceneName) {
      //Using LoadSceneAsync because we want to do loading animations
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(true);
      IsLoading = true;
      AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
      while (!asyncOp.isDone) {
        yield return null;
      }

      Dictionary<string, string> parameters = new Dictionary<string, string> ();
      string forcedName = null;
      if (sceneName == SceneName.MAIN) {
        forcedName = trDataManager.Instance.IsMissionMode ? "SM_CHAL" : "SM_FP";
      } 
      parameters.Add (trTelemetryParamType.ROBOT_TYPE.ToString(), trDataManager.Instance.CurrentRobotTypeSelected.ToString ());
      wwEntryExitTracker.DoEnter(forcedName, parameters);
      IsLoading = false;
      trDataManager.Instance.LoadingScreenCtrl.SetEnable(false);

      if(onSceneLoaded!=null){
        onSceneLoaded.Invoke();
      }
    }

    public sealed class SceneName {
      public const string PROFILES       = "Profiles";
      public const string REMOTE_CONTROL = "RemoteControl";
      public const string MAP            = "Map";
      public const string MAIN           = "Main";
      public const string LOBBY          = "Lobby";
      public const string START          = "Start";
      public const string VAULT          = "Vault";
      public const string COMMUNITY      = "Community";
    }
  }
}
