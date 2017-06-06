using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

namespace Turing{
	
  public class trVaultDeletePanel : MonoBehaviour{

    [SerializeField]
    private Image _deletePanelImage;

    [SerializeField]
    private TextMeshProUGUI _titleText;

    [SerializeField]
    private Button _deleteButton;

    [SerializeField]
    private Button _cancelButton;

    [SerializeField]
    private Button _backButton;

    [SerializeField]
    private Button _backgroundButton;

    public Sprite imageSprite{
      set {
        _deletePanelImage.sprite = value;
      }
    }

    public string title{
      set {
        _titleText.text = value;
      }
    }

    public UnityAction deleteAction{
      set {
        _deleteButton.onClick.AddListener(value);
      }
    }

    public UnityAction cancelAction{
      set {
        _cancelButton.onClick.AddListener(value);
        _backButton.onClick.AddListener(value);
        _backgroundButton.onClick.AddListener(value);
        trUIController uic = GetComponent<trUIController>();
        if(uic == null){
          uic = gameObject.AddComponent<trUIController>();
        }
        WW.BackButtonController.InputEventHandler handler = new WW.BackButtonController.InputEventHandler(value);
        uic.BackBtnCallback = handler;
      }
    }
  }
		
}
