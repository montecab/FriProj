using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using WW.UGUI;


namespace Turing{
  public class trBehaviorInfoConfigurePanelController : uGUISegmentedController {

    public InputField NameInput;

    public GameObject ButtonParent;

    private trBehavior behaviorData;
    public trBehavior BehaviorData{
      set{
        if(behaviorData == value){
          return;
        }

        behaviorData = value;
        SetUpView();
      }
    }

    private Dictionary<string, uGUISegment> typeToButtonDic = null;
    private Dictionary<uGUISegment, string> buttonToTypeDic = null;

    public trProtoController ProtoCtrl;

  	// Use this for initialization
  	void Start () {
      InitView();
    }

    public void SetUpView(){
      InitView();
      if(behaviorData != null){
        //TODO: NameInput.text sometimes causes a Unity bug: http://issuetracker.unity3d.com/issues/inputfield-argumentoutofrangeexception-is-thrown-when-inputting-text-into-email-field
        // The first time running after opening unity caused this bug : 
//          ArgumentOutOfRangeException: Argument is out of range.
//          Parameter name: index
//            System.Collections.Generic.List`1[UnityEngine.UICharInfo].get_Item (Int32 index) (at /Users/builduser/buildslave/mono-runtime-and-classlibs/build/mcs/class/corlib/System.Collections.Generic/List.cs:633)
//            UnityEngine.UI.InputField.SetDrawRangeToContainCaretPosition (UnityEngine.TextGenerator gen, Int32 caretPos, System.Int32& drawStart, System.Int32& drawEnd) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/InputField.cs:1488)
//            UnityEngine.UI.InputField.UpdateLabel () (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/InputField.cs:1360)
//            UnityEngine.UI.InputField.OnPointerDown (UnityEngine.EventSystems.PointerEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/UI/Core/InputField.cs:773)
//            UnityEngine.EventSystems.ExecuteEvents.Execute (IPointerDownHandler handler, UnityEngine.EventSystems.BaseEventData eventData) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:38)
//            UnityEngine.EventSystems.ExecuteEvents.Execute[IPointerDownHandler] (UnityEngine.GameObject target, UnityEngine.EventSystems.BaseEventData eventData, UnityEngine.EventSystems.EventFunction`1 functor) (at /Users/builduser/buildslave/unity/build/Extensions/guisystem/UnityEngine.UI/EventSystem/ExecuteEvents.cs:269)
//            UnityEngine.EventSystems.EventSystem:Update()

       
        NameInput.text = behaviorData.MapSet.Name;
        uGUISegment button = typeToButtonDic[behaviorData.MapSet.IconName];
        ActivateSegment(button);
      }
    }

    void onNameInputChanged(string name){
      behaviorData.MapSet.Name = name;
      trDataManager.Instance.SaveBehavior();
    }

    public override void ActivateSegment (uGUISegment seg)
    {
      base.ActivateSegment (seg);
      if(!buttonToTypeDic.ContainsKey(seg)){
        Debug.LogError("seg not in dic: "  + seg.GetComponent<trButtonBase>().Img.sprite.name);
      }
      behaviorData.MapSet.IconName = buttonToTypeDic[seg];
      trDataManager.Instance.SaveBehavior();
    }

    protected override void OnDoubleClickOnSegment (){
      OnBackgroundClicked();
    }
  	
    public void InitView(){
      if(typeToButtonDic != null){
        return;
      }
      typeToButtonDic = new Dictionary<string, uGUISegment>();
      buttonToTypeDic = new Dictionary<uGUISegment, string>();
      for(int i = 0; i< trIconFactory.UserIconNames.Length; ++i){
        string name = trIconFactory.UserIconNames[i];
        GameObject newButton = trButtonFactory.CreateRoundButton();
        newButton.transform.SetParent(ButtonParent.transform, false);
        trButtonBase buttonbase = newButton.GetComponent<trButtonBase>();
        buttonbase.Img.sprite = trIconFactory.GetIcon(name);

        uGUISegment segment = newButton.AddComponent<uGUISegment>();
        segment.Contents.Add(buttonbase.Focus.gameObject);
        segment.SegmentsController = this;
        segment.SegButton = buttonbase.Btn;

        typeToButtonDic.Add(name, segment);
        buttonToTypeDic.Add(segment, name);

      }
      NameInput.onEndEdit.AddListener(onNameInputChanged);
    }

    public void OnBackgroundClicked(){
      this.gameObject.SetActive(false);
      ProtoCtrl.BehaviorMakerPanelCtrl.SetInfoView();
    }
  }
}
