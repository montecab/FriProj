using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Turing;
using TMPro;

public class trAdminPanel : MonoBehaviour {

  public Button CloseButton;
  public Button MultivariateButton;
  public Button AuthoringPanelButton;
  public Button AnimCopyButton;
  public TextMeshProUGUI AnimCopyProgress;
  public Toggle AnimCacheBust;

  public trMultivariatePanelController MultivariatePanel;

	// Use this for initialization
	void Awake () {
	  if (CloseButton != null){
      CloseButton.onClick.AddListener(() => {
        trSecretAdminController.Instance.ClosePanel();
      });
    }

    MultivariateButton.onClick.AddListener(() => onMultivariateButtonClicked());
    AuthoringPanelButton.onClick.AddListener(trDataManager.Instance.OpenMissionAuthoringPanel);
	}

  void Start(){
    if (Application.isPlaying) {
      DontDestroyOnLoad(piConnectionManager.Instance);
      DontDestroyOnLoad(trDataManager.Instance);
    }
  }

  void onMultivariateButtonClicked(){
    MultivariatePanel.gameObject.SetActive(true);
  }
}
