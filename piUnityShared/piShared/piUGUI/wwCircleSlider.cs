using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using WW;

namespace WW.UGUI{

  [RequireComponent(typeof(RectTransform))]
  public class wwCircleSlider : Selectable,IDragHandler, ICanvasElement {

#if UNITY_EDITOR
    [UnityEditor.MenuItem("GameObject/UI/WW/CircleSlider")]
    static void CreateCircleSlider(){
      GameObject circleSlider = Instantiate(UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/piShared/piPrefabs/UI/CircleSlider.prefab", typeof(GameObject) )) as GameObject;
      if(circleSlider != null){
        circleSlider.transform.SetParent( UnityEditor.Selection.activeTransform);
        circleSlider.transform.localScale = Vector3.one;
        circleSlider.gameObject.name = "CircleSlider";
      }
      else{
        Debug.LogError( "Can't find the circle slider object ");    
      }
    }
#endif

    public float normalizedValue
    {
      get
      {
        if (Mathf.Approximately(minValue, maxValue))
          return 0;
        return Mathf.InverseLerp(minValue, maxValue, Value);
      }
      set
      {
        this.Value = Mathf.Lerp(minValue, maxValue, value);
      }
    }

    [System.Serializable]
    public class SliderEvent : UnityEvent<float> {}

    [SerializeField]
    private RectTransform fillRect;
    public RectTransform FillRect { get { return fillRect; } set { if (wwUtility.SetClass(ref fillRect, value)) {UpdateCachedReferences(); UpdateVisuals(); } } }
    
    [SerializeField]
    private RectTransform handleRect;
    public RectTransform HandleRect { get { return handleRect; } set { if (wwUtility.SetClass(ref handleRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

    [SerializeField]
    private RectTransform backgroundRect;
    public RectTransform BGRect { get { return backgroundRect; } set { if (wwUtility.SetClass(ref backgroundRect, value)) { UpdateCachedReferences(); UpdateVisuals(); } } }

    [SerializeField]
    private float minValue = 0;
    public float MinValue { get { return minValue; } set { if (wwUtility.SetStruct(ref minValue, value)) { Set(mValue); UpdateVisuals(); } } }

    [SerializeField]
    private float maxValue = 1;
    public float MaxValue { get { return maxValue; } set { if (wwUtility.SetStruct(ref maxValue, value)) { Set(mValue); UpdateVisuals(); } } }

    public bool IsDuplex = false;

    [SerializeField]
    private float wholeDegree = 360;
    public float WholeDegree { 
      get { return wholeDegree; } 
      set { 
        value = value >= 0 ? value: 0;
        if(value > 360){
          value = value%360;
        } 
        if (wwUtility.SetStruct(ref wholeDegree, value)) {
          Set(value); UpdateVisuals(); 
        }
      } 
    }



    [SerializeField]
    private bool wholeNumbers = false;
    public bool WholeNumbers { get { return wholeNumbers; } set { if (wwUtility.SetStruct(ref wholeNumbers, value)) { Set(mValue); UpdateVisuals(); } } }

    [SerializeField]
    private float mValue = 1f;
    public float Value
    {
      get
      {
        if (wholeNumbers){
          return Mathf.Round(mValue);
        }
        return mValue;
      }
      set
      {
        Set(value);
      }
    }

    // Allow for delegate-based subscriptions for faster events than 'eventReceiver', and allowing for multiple receivers.
    [SerializeField]
    private SliderEvent onValueChanged = new SliderEvent();
    public SliderEvent OnValueChanged { get { return onValueChanged; } set { onValueChanged = value; } }

    // Private fields
    private Image bgImage;
    private Image fillImage;

    // Set the valueUpdate the visible Image.
    void Set(float input)
    {
      Set(input, true);
    }
    
    void Set(float input, bool sendCallback)
    {
      // Clamp the input
      float newValue = Mathf.Clamp(input, minValue, maxValue);
      if (wholeNumbers)
        newValue = Mathf.Round(newValue);
      
      // If the stepped value doesn't match the last one, it's time to update
      if (mValue == newValue)
        return;
      
      mValue = newValue;
      UpdateVisuals();
      if (sendCallback){
        onValueChanged.Invoke(newValue);
      }
    }

    public void UpdateVisuals(){

      #if UNITY_EDITOR
      if (!Application.isPlaying)
        UpdateCachedReferences();
      #endif

      if(bgImage != null){
        bgImage.fillAmount = wholeDegree/360.0f;
      }

      float converted = normalizedValue;
      if (IsDuplex){
        converted -= 0.5f;
      }

      if(fillImage != null){
        fillImage.fillAmount = Mathf.Abs(converted) * wholeDegree/360f;
        fillImage.fillClockwise = converted > 0;
      }

      if(handleRect != null){
        handleRect.transform.localRotation = Quaternion.Euler(0,0,360f - converted * wholeDegree);
      }

    }

    void UpdateCachedReferences()
    {
      if (fillRect)
      {
        fillImage = fillRect.GetComponent<Image>();
      }
      else
      {
        fillImage = null;
      }

      if(BGRect){
        bgImage = BGRect.GetComponent<Image>();
      }else{
        bgImage = null;
      }

    }

    #region IDragHandler implementation

    public void OnDrag (PointerEventData eventData)
    {
      if (!MayDrag(eventData))
        return;
      
      UpdateDrag(eventData, eventData.pressEventCamera);
    }

    #endregion

    #region ICanvasElement implementation

    public void Rebuild (CanvasUpdate executing)
    {
      #if UNITY_EDITOR
      if (executing == CanvasUpdate.Prelayout)
        onValueChanged.Invoke(mValue);
      #endif
    }

    public void LayoutComplete(){}

    public void GraphicUpdateComplete(){}

    #endregion

    private bool MayDrag(PointerEventData eventData)
    {
      return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
    }
    
    public override void OnPointerDown(PointerEventData eventData)
    {
      if (!MayDrag(eventData))
        return;
      
      base.OnPointerDown(eventData);
      if (!RectTransformUtility.RectangleContainsScreenPoint(handleRect, eventData.position, eventData.enterEventCamera))
      {
        // Outside the slider handle - jump to this point instead
        UpdateDrag(eventData, eventData.pressEventCamera);
      }
    }

    // Update the slider's position based on the mouse.
    void UpdateDrag(PointerEventData eventData, Camera cam)
    {
      RectTransform clickRect = backgroundRect;
      if (clickRect != null && clickRect.rect.size.magnitude > 0)
      {
        Vector2 localCursor;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(clickRect, eventData.position, cam, out localCursor))
          return;
        //localCursor -= clickRect.rect.position;

        float angle = Vector2.Angle(Vector2.up, localCursor);

        float roundDegrees = 360f;

        if (IsDuplex){
          angle = localCursor.x >= 0 ? angle: - angle;
          angle += roundDegrees / 2;
        } else {
          angle = localCursor.x >= 0 ? angle: roundDegrees - angle;
        }

       // Debug.LogError(string.Format("value {0}", angle));
      
        if(angle < wholeDegree){
          normalizedValue = angle/wholeDegree;
        }
      }
    }

    
    #if UNITY_EDITOR
    protected override void OnValidate()
    {
      if (wholeNumbers)
      {
        minValue = Mathf.Round(minValue);
        maxValue = Mathf.Round(maxValue);
      }

      WholeDegree = wholeDegree;

      UpdateCachedReferences();
      Set(mValue, false);
      // Update rects since other things might affect them even if value didn't change.
      UpdateVisuals();
      
      var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
      if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
        CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }
    
    #endif 

  }
}
