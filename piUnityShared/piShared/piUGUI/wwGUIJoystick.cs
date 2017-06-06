using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;

public class wwGUIJoystick : MonoBehaviour, IBeginDragHandler, IDragHandler, IEventSystemHandler, IPointerDownHandler, IPointerUpHandler {

#if UNITY_EDITOR
  [UnityEditor.MenuItem("GameObject/UI/WW/Joystick")]
  static void CreateJoystick() {
    GameObject holder = new GameObject("Joystick");

    if (UnityEditor.Selection.activeTransform != null) {

      holder.AddComponent<Image>();
      holder.AddComponent<GraphicRaycaster>();
      holder.AddComponent<wwGUIJoystick>();

      GameObject thumb = new GameObject("Thumb");
      thumb.AddComponent<Image>();

      thumb.transform.parent = holder.transform;
      thumb.transform.Translate(holder.transform.position);


      wwGUIJoystick logic = holder.GetComponent<wwGUIJoystick>();
      logic.BackgroundImage = holder.GetComponent<Image>();
      logic.ThumbImage = thumb.GetComponent<Image>();


      holder.transform.parent = UnityEditor.Selection.activeTransform;
      holder.transform.Translate(UnityEditor.Selection.activeTransform.position);
    }
  }
#endif

  public Image        BackgroundImage;
  public Image        ThumbImage;

  public Graphic      HighlightGraphic;
  public Color        HighlightColor;

  public JoystickMode CurrentMode = JoystickMode.Circular;

  public bool BackToCenterWhenRelease = true;

  public JoystickEvent     OnPress;
  public JoystickEvent     OnDragStart;
  public JoystickEvent     OnRelease;
  public JoystickMoveEvent OnRestoreThumbPosition;
  public JoystickMoveEvent OnValueChange;              // When thumb position changed
  public JoystickMoveEvent OnValueChangeBeforeRelease; // when thumb position changed before release

  private IThumbMath ThumbMathProcessor;

  private Color HighlightGraphicOriginalColor;


  void Start() {
    if (BackgroundImage == null) {
      BackgroundImage = GetComponent<Image>();
    }

    if (ThumbImage == null) {
      ThumbImage = GetComponentInChildren<Image>();
    }

    if (HighlightGraphic != null) {
      HighlightGraphicOriginalColor = HighlightGraphic.color;
    }

    ThumbMathProcessor = CreateMathForType(CurrentMode);
    ThumbMathProcessor.SetupInitialValues(BackgroundImage.rectTransform.rect, ThumbImage.rectTransform.rect);
  }

  // mechanism to prevent the thumb from "popping" on the intial pointer-down.
  private Vector2 localCursorOffset = Vector2.zero;

  private void latchCursorOffset(PointerEventData eventData) {
    localCursorOffset = Vector2.zero;
    localCursorOffset = (Vector2)(ThumbImage.transform.localPosition) - screenToLocal(eventData);
  }

  private Vector2 screenToLocal(PointerEventData eventData) {
    Vector2 ret;
    RectTransformUtility.ScreenPointToLocalPointInRectangle(BackgroundImage.rectTransform, eventData.position, eventData.pressEventCamera, out ret);
    ret += localCursorOffset;
    return ret;
  }

  public void OnPointerDown(PointerEventData eventData){
    latchCursorOffset(eventData);
    ThumbImage.transform.localPosition = ThumbMathProcessor.DoTransformMoveThumbPosition(screenToLocal(eventData));
    NotifyDragPositionChange();

    if (HighlightGraphic != null) {
      HighlightGraphic.color = HighlightColor;
    }

    if(OnPress != null){
      OnPress.Invoke(this);
    }
  }

  public void OnBeginDrag (PointerEventData eventData) {
    // todo: is it possible this eventData has different position than both OnPointerDown and OnDrag ?
    if (OnDragStart != null) {
      OnDragStart.Invoke(this);
    }
  }

  public void OnDrag (PointerEventData eventData) {
    ThumbImage.transform.localPosition = ThumbMathProcessor.DoTransformMoveThumbPosition(screenToLocal(eventData));
    NotifyDragPositionChange();
  }

  public void OnPointerUp(PointerEventData eventData){
    Vector2 destinationPoint = Vector2.zero;
    
    if (OnRelease != null) {
      OnRelease.Invoke(this);
    }
    
    if (OnRestoreThumbPosition != null && OnRestoreThumbPosition.GetPersistentEventCount() > 0) {
      OnRestoreThumbPosition.Invoke(this, destinationPoint);
    } else if(BackToCenterWhenRelease){
      ThumbImage.transform.localPosition = destinationPoint;
    }

    if (HighlightGraphic != null) {
      HighlightGraphic.color = HighlightGraphicOriginalColor;
    }

    NotifyDragPositionChange(true);
  }

  private void NotifyDragPositionChange(bool isReleasing = false) {
    Vector2 position = ThumbMathProcessor.GetNormalizedPosition(ThumbImage.transform.localPosition);
    if (OnValueChange != null) {
      OnValueChange.Invoke(this, position);
    }

    if(!isReleasing && OnValueChangeBeforeRelease != null){
      OnValueChangeBeforeRelease.Invoke(this, position);
    }
  }

  private IThumbMath CreateMathForType(JoystickMode mode) {
    IThumbMath result = null;

    switch (mode) {
      case JoystickMode.Circular:
        result = new ThumbMathCircular();
        break;
      case JoystickMode.Rectangular:
        result = new ThumbMathRectangular();
        break;
      default:
        break;
    };
    return result;
  }

  private class ThumbMathRectangular: IThumbMath {

    private Rect ActiveZone;

    public void SetupInitialValues (Rect background, Rect thumb) {
      ActiveZone = background;
      Vector2 thumpSize = new Vector2(thumb.width / 2, thumb.height / 2);
      ActiveZone.xMin += thumpSize.x;
      ActiveZone.xMax -= thumpSize.x;
      ActiveZone.yMin += thumpSize.y;
      ActiveZone.yMax -= thumpSize.y;
    }

    public Vector2 DoTransformMoveThumbPosition (Vector2 newLocalPosition) {
      Vector2 result = new Vector2(newLocalPosition.x, newLocalPosition.y);

      result.x = Mathf.Clamp(result.x, ActiveZone.xMin, ActiveZone.xMax);
      result.y = Mathf.Clamp(result.y, ActiveZone.yMin, ActiveZone.yMax);

      return result;
    }

    public Vector2 GetNormalizedPosition (Vector2 localPosition) {
      return new Vector2(
       (localPosition.x) / (ActiveZone.xMax - ActiveZone.xMin) * 2,
       (localPosition.y) / (ActiveZone.yMax - ActiveZone.yMin) * 2);
    }
  }

  private class ThumbMathCircular: IThumbMath {

    private float Radius = 0;

    public void SetupInitialValues (Rect placeholder, Rect thumb) {
      Radius = (getMinSide(placeholder) - getMinSide(thumb)) / 2;
    }

    public Vector2 DoTransformMoveThumbPosition (Vector2 localPosition) {
      Vector2 result = new Vector2(localPosition.x, localPosition.y);
      float radiusProportion = result.magnitude / Radius;

      if (radiusProportion > 1.0) {
        result /= radiusProportion;
      }
      return result;
    }

    public Vector2 GetNormalizedPosition (Vector2 localPosition) {
      return localPosition / Radius;
    }

    private float getMinSide(Rect rect) {
      return Mathf.Min(rect.width, rect.height);
    }
  }

  public enum JoystickMode {
    Rectangular,
    Circular
  }
  
  private interface IThumbMath {
    void SetupInitialValues(Rect background, Rect thumb);
    Vector2 DoTransformMoveThumbPosition(Vector2 newLocalPosition);
    Vector2 GetNormalizedPosition(Vector2 localPosition);
  }

  #region Events
  [System.Serializable]
  public class JoystickEvent: UnityEvent<wwGUIJoystick> {}

  [System.Serializable]
  public class JoystickMoveEvent: UnityEvent<wwGUIJoystick, Vector2> {}
  #endregion

}

