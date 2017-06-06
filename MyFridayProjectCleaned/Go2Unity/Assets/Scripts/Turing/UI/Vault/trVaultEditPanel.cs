using UnityEngine;
using UnityEngine.Events;
using TMPro;

namespace Turing{
  public class trVaultEditPanel : MonoBehaviour{

    [SerializeField]
    private TMP_InputField _editNameField;

    public UnityAction<string> nameChangedAction{
      set {
        _editNameField.onValueChanged.AddListener(value);
      }
    }

    public UnityAction<string> endEditAction{
      set {
        _editNameField.onEndEdit.AddListener(value);
      }
    }

    public void activateInputField(){
      _editNameField.ActivateInputField();
    }

    public string text{
      set {
        _editNameField.text = value;
      }
    }
  }
}
