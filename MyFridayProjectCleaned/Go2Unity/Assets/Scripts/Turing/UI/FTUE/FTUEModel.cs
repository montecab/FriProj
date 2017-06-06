using UnityEngine;
using System.Collections;

public class FTUEModel: ScriptableObject{
  public FTUEType ftueType;
  public Sprite backgroundSkrim;
  public Sprite eliSprite;
  public Color backgroundColor;

  public Vector3 notificationPos;
  public Vector4 notificationAnchorsMinMax;
  public Vector2 notificationPivot;

  public Vector3 speechBubblePos;
  public Vector2 speechBubbleSize;
  public Vector4 speechBubbleAnchorsMinMax;
  public Vector2 speechBubblePivot;

  public Vector3 hitboxPos;
  public Vector2 hitboxSize;
  public Vector4 hitboxAnchorsMinMax;
  public Vector2 hitboxPivot;

  public bool enableNotification;
  public bool enablePlayButton;
  public bool enableHitboxButton;
  public bool isLeft;
}
