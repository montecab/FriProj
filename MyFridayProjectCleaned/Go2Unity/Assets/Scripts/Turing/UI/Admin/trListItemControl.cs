using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using TMPro;

public class trListItemControl : MonoBehaviour{

  public TextMeshProUGUI IndexText;
  public TextMeshProUGUI NameText;
  public Text ValueText;
  public Image MImage;
  public OnClickedEvent onItemClicked;

  void Start(){
    this.gameObject.GetComponent<Button>().onClick.AddListener(OnClick);
  }

  void OnClick(){
    if (onItemClicked != null){
      onItemClicked.Invoke(this);
    }
  }
	
  [System.Serializable]
  public class OnClickedEvent: UnityEvent<trListItemControl> {}
}
