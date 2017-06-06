using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Attach this script to a Button(UGUI) and assign a scene name.
/// When clicking the button, it will go to the assgned scene.
/// </summary>
public class ButtonSceneLoad : MonoBehaviour {

  public string SceneName;
  
  // Use this for initialization
  void Start () {
    Button btn = this.GetComponent<Button>();
    btn.onClick.AddListener(() => OnNextButtonClicked());
  }
  
  void OnNextButtonClicked(){
    SceneManager.LoadScene(SceneName, LoadSceneMode.Single);
  }
}
