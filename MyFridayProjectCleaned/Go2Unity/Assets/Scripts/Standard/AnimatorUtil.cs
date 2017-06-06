using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public static class AnimatorUtil{

  private static readonly string INDEX = "Index";

  public static void ChangeState(this Animator animator, int index){
    animator.SetInteger(INDEX, index);
  }

  public static void RestartState(this Animator animator){
      animator.Play(animator.GetCurrentAnimatorStateInfo(0).fullPathHash, 0, 0f);
  }

  public static bool IsAnimationPlaying(this Animator animator){
    return (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1 || animator.IsInTransition(0));
  }

  public static IEnumerator WaitForAnimationEnd(this Animator animator, UnityAction callback){
    yield return new WaitForEndOfFrame();
    yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    callback.Invoke();
  }

}
