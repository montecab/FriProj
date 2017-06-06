using UnityEngine;
using System.Collections;
using TMPro;

namespace Turing{
  public class trLoadingScreenPanelController : MonoBehaviour {
    public TextMeshProUGUI LoadingLabel;
    public const string DEFAULT_TEXT = "@!@Loading...@!@";

    public void SetEnable(bool isActive, string text = DEFAULT_TEXT){
      this.gameObject.SetActive(isActive);
      UpdateText(text);
    }

    public void UpdateText(string text){
      LoadingLabel.text = wwLoca.Format(text);
    }
  }
}
