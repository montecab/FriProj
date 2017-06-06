using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using TMPro;
using Turing;
using WW.SimpleJSON;

namespace Turing {
  public class trVaultPopUpController : MonoBehaviour {

    public TextMeshProUGUI Title;

    // middle section is mutually exclusive
    public Image EliHappy;
    public Image EliSurprised;
    public TextMeshProUGUI DescriptionText;
    public Image ChallengeMap;
    public GameObject ProgramDiagram;
    public trVaultStateMachineController ProgramSM;

    public TextMeshProUGUI CallToActionText;
    public InputField InputText;
    public TextMeshProUGUI InputTextLabel;
    public Button ButtonNo;
    public Button ButtonYes;
    public Button ButtonCopyYes; // for copying challenge into a new program
    
    Action callbackYes;
    Action<string> callbackInputTextUpdate;
    
    void Start () {
      ButtonNo.onClick.AddListener(dismissModal);
      //ButtonOkay.onClick.AddListener(dismissModal);
      ButtonYes.onClick.AddListener(onClickPopUpYes);
      ButtonCopyYes.onClick.AddListener(onClickPopUpYes);
      InputText.onEndEdit.AddListener(onEndEditInput);

      // make sure things are always setup correctly
      Title.gameObject.SetActive(true);
      ButtonNo.gameObject.SetActive(true);
    }

    public void ShowPanel() {
      gameObject.SetActive(true);
    }

    public void ShowDescription(string text) {
      DescriptionText.text = text;
      DescriptionText.gameObject.SetActive(true);
      ProgramDiagram.gameObject.SetActive(false);
      ChallengeMap.gameObject.SetActive(false);

      EliSurprised.gameObject.SetActive(true);
      EliHappy.gameObject.SetActive(false);
    }

    public void ShowChallengeMap() {
      ChallengeMap.gameObject.SetActive(true);
      DescriptionText.gameObject.SetActive(false);
      ProgramDiagram.gameObject.SetActive(false);

      EliSurprised.gameObject.SetActive(true);
      EliHappy.gameObject.SetActive(false);
    }

    public void ShowProgram(trProgram program) {
      ProgramSM.SetUpView(program);
      ProgramDiagram.gameObject.SetActive(true);
      DescriptionText.gameObject.SetActive(false);
      ChallengeMap.gameObject.SetActive(false);

      EliSurprised.gameObject.SetActive(false);
      EliHappy.gameObject.SetActive(true);
    }

    public void ShowCallToActionText(string text) {
      CallToActionText.gameObject.SetActive(true);
      CallToActionText.text = text;

      InputText.gameObject.SetActive(false);
      InputTextLabel.gameObject.SetActive(false);
    }

    public void ShowCopyYes(bool showCopyYes, Action callback) {
      ButtonCopyYes.gameObject.SetActive(showCopyYes);
      ButtonYes.gameObject.SetActive(!showCopyYes);

      callbackYes = callback;
      callbackInputTextUpdate = null;
    }

    public void ShowEditInput(string placeholderText, Action<string> callback) {
      InputText.gameObject.SetActive(true);
      InputTextLabel.gameObject.SetActive(true);
      CallToActionText.gameObject.SetActive(false);
      ButtonCopyYes.gameObject.SetActive(false);
      ButtonYes.gameObject.SetActive(true);
      InputText.text = placeholderText;

      callbackYes = null;
      callbackInputTextUpdate = callback;
    }
    
    void dismissModal() {
      gameObject.SetActive(false);
    }
    
    void onClickPopUpYes() {
      if (callbackYes != null) {
        callbackYes();
      }
      else if (callbackInputTextUpdate != null) {
        callbackInputTextUpdate(InputText.text);
      }
      dismissModal();
    }
    
    void onEndEditInput(string s) {    
      InputText.text = trProgram.sanitizeFilename(s);
    }
  }
}