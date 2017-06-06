using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using WW.UGUI;

namespace Turing{
  public class trAuthoringMissionButtonBase :  uGUIDragDrop{
    public InputField NameInput;
    public InputField PointsInput;
    public Text IndexText;
    public Image Img;
    public Button DeleteButton;
    public Button RunButton;

    public delegate void ButtonDelegate(trAuthoringMissionButtonBase ch);
    public ButtonDelegate OnDeleteButtonClick;

    public ButtonDelegate OnDragEnd;
    public ButtonDelegate OnDragging;
    public ButtonDelegate OnButtonClick;
    public ButtonDelegate OnNameInputChange;
    public ButtonDelegate OnRunButtonClick;
    public ButtonDelegate OnPointsInputChange;

    public void SetView(int id, string name, bool isShowRunButton = false){
      IndexText.text = id.ToString();
      NameInput.text = name;
      if(isShowRunButton){
        RunButton.gameObject.SetActive(true);
      }
    }

    void Start(){
      DeleteButton.onClick.AddListener(()=>onDeleteButtonClicked());
      RunButton.onClick.AddListener(()=>onRunButtonClicked());
      this.GetComponent<Button>().onClick.AddListener(()=>onButtonClicked());
      NameInput.onEndEdit.AddListener(onNameInputChange);
      PointsInput.onEndEdit.AddListener(onPointsInputChange);
    }

    void onPointsInputChange(string s){
      if(OnPointsInputChange != null){
        OnPointsInputChange(this);
      }
    }

    void onNameInputChange(string s){
      if(OnNameInputChange != null){
        OnNameInputChange(this);
      }
    }

    void onDeleteButtonClicked(){
      if(OnDeleteButtonClick != null){
        OnDeleteButtonClick(this);
      }
    }

    void onRunButtonClicked(){
      if(OnRunButtonClick != null){
        OnRunButtonClick(this);
      }
    }

    void onButtonClicked(){
      if(OnButtonClick != null){
        OnButtonClick(this);
      }
    }

    public override void OnDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
      base.OnDrag (eventData);
      if(OnDragging != null){
        OnDragging(this);
      }
    }

    public override void OnEndDrag (UnityEngine.EventSystems.PointerEventData eventData)
    {
      base.OnEndDrag (eventData);

      if(OnDragEnd != null){
        OnDragEnd(this);
      }
    }




  }
}
