using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

namespace Turing{
  public class trAnimationPlayerController : MonoBehaviour {
    private string animationId;
    public string AnimationId{
      get{
        return animationId;
      }
      set {
        animationId = value;
      }
    }

    public delegate void AnimationPlayerDelegate();
    public AnimationPlayerDelegate onFinished;
  }
}