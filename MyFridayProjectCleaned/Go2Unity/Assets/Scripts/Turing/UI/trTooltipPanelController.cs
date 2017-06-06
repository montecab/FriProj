using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class trTooltipPanelController : MonoBehaviour {

  [SerializeField]
  private Button _backgroundButton;
  [SerializeField]
  private GameObject _leftArrow;
  [SerializeField]
  private GameObject _rightArrow;
  [SerializeField]
  private GameObject _topArrow;
  [SerializeField]
  private GameObject _downArrow;
  [SerializeField]
  private TextMeshProUGUI _description;

  private enum ArrowType{
    LEFT,
    RIGHT,
    TOP,
    DOWN,
  }
  private Animator _animator;

  public void Display (string description, UnityAction onBackgroundClicked = null){
    if(!this.gameObject.activeInHierarchy){
      _description.text = description;
      if(onBackgroundClicked!=null){
        _backgroundButton.onClick.AddListener(onBackgroundClicked);
      }
      else{
        _backgroundButton.onClick.AddListener(Close);
      }
      this.gameObject.SetActive(true);
      _animator = this.GetComponent<Animator>();
      if (_animator) {
        _animator.ChangeState(1);
      }
    }
  }

  public void Close (){
    if(this.gameObject.activeInHierarchy){
      if (_animator && !_animator.IsAnimationPlaying()) {
        _animator.ChangeState(2);
        StartCoroutine (_animator.WaitForAnimationEnd(()=>{
          this.gameObject.SetActive(false);
        }));
      } 
      else {
        this.gameObject.SetActive(false);
      }
    }
  }

}
