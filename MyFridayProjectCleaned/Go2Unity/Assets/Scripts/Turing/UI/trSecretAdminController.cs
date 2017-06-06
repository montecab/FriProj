using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class trSecretAdminController : Singleton<trSecretAdminController> {

	private const string AdminSceneName = "AdminScene";
  private const string OptionsSceneName = "Options";
  private string previousSceneName = null;
  private Dictionary<string, System.Action> secretActions = new Dictionary<string, System.Action>();
  private string secretPhraseString = "";
  private float lastKeyTime = 0;

  private void remeberCurrentScene(){
    previousSceneName = SceneManager.GetActiveScene().name;
  }

  private void loadAdminScene(){
    SceneManager.LoadScene(AdminSceneName, LoadSceneMode.Single);
  }

  private void restoreScene(){
    if (previousSceneName != null){
      SceneManager.LoadScene(previousSceneName, LoadSceneMode.Single);
      previousSceneName = null;
    }
  }

  private void actionShowAdminScene(){
    remeberCurrentScene();
    //loadAdminScene();
    Turing.trDataManager.Instance.AdminPanel.gameObject.SetActive(true);
  }
  
  private void actionShowOptionsTeachers() {
    Turing.trDataManager.Instance.optionsPanelShowInternal = false;
    SceneManager.LoadScene(OptionsSceneName, LoadSceneMode.Single);
  }
  
  private void actionShowOptionsInternal() {
    Turing.trDataManager.Instance.optionsPanelShowInternal = true;
    SceneManager.LoadScene(OptionsSceneName, LoadSceneMode.Single);
  }

  private void actionShowOptionsAdmin (){
    if (Debug.isDebugBuild) {
      Turing.trDataManager.Instance.AdminPanel.gameObject.SetActive(true);
    }
  }
  
  public void clearKeyPhrase() {
    secretPhraseString = "";
  }

  public void addToKeyPhrase(string appendix){
    
    if(Time.fixedTime - lastKeyTime > 3.0f){
      secretPhraseString = "";
    }
    lastKeyTime = Time.fixedTime;
    
    secretPhraseString += appendix;
    
    if (secretActions.ContainsKey(secretPhraseString)){
      WWLog.logDebug(string.Format("activating secret action!"));
      secretActions[secretPhraseString]();
      secretPhraseString = "";
    } else {
      bool isPrefixForCode = false;
      foreach(var code in secretActions.Keys){
        isPrefixForCode = code.StartsWith(secretPhraseString);
        if (isPrefixForCode){
          break;
        }
      }
      if (!isPrefixForCode){
        secretPhraseString = "";
      }
    }
  }

  public void ClosePanel(){
    Turing.trDataManager.Instance.AdminPanel.gameObject.SetActive(false);
    SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
  }

  public trSecretAdminController () {
    secretActions.Add("RRRRLLLLRRRR"                                    , actionShowAdminScene);
    secretActions.Add("LFP LIL LFP "                                    , actionShowOptionsTeachers);
    secretActions.Add("LIL LIL LIL LIL LFP LFP LFP LFP LIL LIL LIL LIL ", actionShowOptionsInternal);
    secretActions.Add("LFP LFP LFP LFP LFP LFP "                        , actionShowOptionsAdmin);
  }
}
