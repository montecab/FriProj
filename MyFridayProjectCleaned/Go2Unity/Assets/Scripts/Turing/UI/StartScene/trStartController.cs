using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

namespace Turing{
  public class trStartController : MonoBehaviour {

    [SerializeField]
    private Image _splashScreenImage;
    [SerializeField]
    private Button _goButton;

    private void Start(){
    
      // Important!  Be sure to make some call into piConnectionManager at least once per scene.
      piConnectionManager.Instance.hideChromeButton();
    
      trDataManager.Instance.Init();
      _splashScreenImage.gameObject.SetActive(true);
      _goButton.onClick.AddListener(onGoButtonPressed);
    }

    private void onGoButtonPressed(){
        trNavigationRouter.Instance.ShowSceneWithName(trNavigationRouter.SceneName.LOBBY);
    }

  }
}
