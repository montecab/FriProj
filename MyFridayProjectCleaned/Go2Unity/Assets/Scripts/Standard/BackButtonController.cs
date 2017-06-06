using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace WW {
  public class BackButtonController : Singleton<BackButtonController> {

    public delegate void InputEventHandler();

    private List<InputEventHandler> callbacks = new List<InputEventHandler>();

    private void Update ()
    {
      if (Input.GetKeyDown (KeyCode.Escape)) {
        if (callbacks.Count > 0) {
          callbacks[callbacks.Count-1].Invoke();
        }
      }
    }

    public void AddListener(InputEventHandler listener){
      callbacks.Add(listener);
    }

    public void RemoveListener(InputEventHandler listener){
      callbacks.Remove(listener);
    }

  }
}