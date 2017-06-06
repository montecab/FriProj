using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trAlertDialogController : trUIController {

  	public TextMeshProUGUI TitleText;
    public TextMeshProUGUI DescriptionText;
    public Button BackgroundButton;
    public Button OkayButton;
    public bool DestroyOnHide = true;

    public delegate void ButtonClickedDelegate(trAlertDialogController controller, Button button);
    public ButtonClickedDelegate OnButtonClicked;

    void Start(){
      OkayButton.onClick.AddListener(onOkayButtonClicked);
      BackgroundButton.onClick.AddListener(onOkayButtonClicked);
      BackBtnCallback = onOkayButtonClicked;
    }

    protected override void OnDisable (){
      if (DestroyOnHide){
        Destroy(gameObject);
      }
    }

    public void HideDialog(){
      gameObject.SetActive(false);
    }

    void onOkayButtonClicked(){
      if (OnButtonClicked != null){
        OnButtonClicked(this, OkayButton);
      }
      HideDialog();
    }
  }
}
