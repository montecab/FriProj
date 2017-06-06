using UnityEngine;
using System.Collections;
using Turing;
using WW;

public class trVaultStateMachineController : trStateMachinePanelBase {


  public override void SetUpView(trProgram program) {
    IsDisableInteraction = true;
    program.CenterStatesOnCanvas();
    base.SetUpView(program);
  }
}
