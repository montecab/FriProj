using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;


namespace Turing {
  public class trNormalDialogBase : MonoBehaviour {

  	public TextMeshProUGUI Title;
    public TextMeshProUGUI Description;
    public Button OKButton;
    public Button Background;

    public string TitleText{
      get{
        return Title.text;
      }
      set{
        if (!string.IsNullOrEmpty(value))
          Title.text = value;
      }
    }
    public string DescriptionText{
      get{
        return Description.text;
      }
      set{
        if (!string.IsNullOrEmpty(value))
          Description.text = value;
      }
    }
    public UnityEngine.Events.UnityAction OnOKButtonClicked;

    void Start() {
      SetupDialog();
    }

    public void SetActive(bool active) {
      if (active) {
        SetupDialog();
      }
      gameObject.SetActive(active);
    }

    protected virtual void SetupDialog() {
      if (OKButton != null) {
        OKButton.onClick.AddListener(onOkayButtonPress);
      }

      if (Background != null) {
        Background.onClick.AddListener(onBackgroundPress);
      }
    }

    protected virtual void onBackgroundPress() {
      onOkayButtonPress();
    }

    void onOkayButtonPress() {
      gameObject.SetActive(false);
      if (OnOKButtonClicked != null) {
        OnOKButtonClicked();
      }
    }
  }
}
