using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

namespace Turing{
  public class trVaultLeftPanel : MonoBehaviour{
    [SerializeField]
    private wwToggleableButton _robotToggleButton;

    [SerializeField]
    private Button _secretButton;

    public wwToggleableButton robotToggleButton{
      get {
        return _robotToggleButton;
      }
    }

    public UnityAction secretButtonAction{
      set {
        _secretButton.gameObject.SetActive(true);
        _secretButton.onClick.AddListener(value);
      }
    }
  }
}
