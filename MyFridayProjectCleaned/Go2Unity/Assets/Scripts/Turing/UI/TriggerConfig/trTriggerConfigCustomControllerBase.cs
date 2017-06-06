using UnityEngine;
using System.Collections;
using Turing;

public abstract class trTriggerConfigCustomControllerBase : MonoBehaviour {

  protected trTrigger    trigger;
  
  public trProtoController ProtoController;
  
  public virtual void SetUp(trTransition transition, trTrigger trigger) {
    this.trigger = trigger;
  }    
}
