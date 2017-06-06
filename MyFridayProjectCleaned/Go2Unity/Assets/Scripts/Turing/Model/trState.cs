
using UnityEngine;
using System.Collections.Generic;
using WW.SimpleJSON;

namespace Turing {
  public class trState : trBase {

    public string             UserFacingName;
    public trBehavior         Behavior;
    public List<trTransition> OutgoingTransitions = new List<trTransition>();
    public Vector2            LayoutPosition = Vector2.zero;
    public bool               IsOmniState = false;
    public trMoodType         Mood = trMoodType.NO_CHANGE;
    public bool               IsCheckingParameter = true;  //Only used in challenges
    public bool               IsObscured = false; //Only used in hints
    private int               activationCount = 1;
    public int                ActivationCount{
      set{
        activationCount = (value < 0) ? 0 : value;
      }
      get{
        return activationCount;
      }
    }

    private List<float> behaviorParameterValues = new List<float>();

    public int BehaviorParaValuesCount{
      get{
        return behaviorParameterValues.Count;
      }
    }

    // support existing behavior of having just 1 normalized value
    public float BehaviorParameterValue {
      get {
        return GetBehaviorParameterValue(0);
      }
      set {
        SetBehaviorParameterValue(0, value);
      }
    }
    public float NormalizedBehaviorParameterValue {
      set {
        SetNormalizedBehaviorParameterValue(0, value);
      }
      get {
        return GetNormalizedBehaviorParameterValue(0);
      }
    }

    public bool IsBehaviorParameterDifferent(trState s){
      if(Behavior.Type != s.Behavior.Type){
        return false;
      }

      if(!Behavior.IsParameterized){
        return false;
      }
      for(int i = 0; i< this.BehaviorParaValuesCount; ++i){
        if(this.GetBehaviorParameterValue(i) != s.GetBehaviorParameterValue(i)){
         
          return true;
        }
      }

      return false;
    }

    public float GetBehaviorParameterValue(int index=0){
      float paramValue = 0; // by default we return 0
      if (IsParameterValueSet(index)){
        paramValue = behaviorParameterValues[index];
      }
      return paramValue;
    }
    public void SetBehaviorParameterValue(int index, float newValue){
      if (IsParameterValueSet(index)) {
        behaviorParameterValues[index] = newValue;
      }
      else {
        behaviorParameterValues.Add (newValue);
      }
    }

    public void Copy(trState state){
      LayoutPosition = state.LayoutPosition;
      IsOmniState = state.IsOmniState;
      Mood = state.Mood;
      Behavior = state.Behavior;
      IsObscured = state.IsObscured;
      IsCheckingParameter = state.IsCheckingParameter;
      for(int i = 0; i< state.BehaviorParaValuesCount; ++i){
        SetBehaviorParameterValue(i, state.GetBehaviorParameterValue(i));
      }
      ActivationCount = state.ActivationCount;
    }

    public void ResetBehaviorParameterValue(){
      behaviorParameterValues.Clear();
    }

    public bool IsParameterValueSet(int index=0){
      return (index >= 0) && (index < behaviorParameterValues.Count) && !float.IsNaN(behaviorParameterValues[index]);
    }

    public float GetNormalizedBehaviorParameterValue(int index=0){
      return Behavior.Normalize(GetBehaviorParameterValue(index));
    }

    //Note that this function only cares about if the target's setting 
    //of checking state para or not. Eg. if you want to check if an orig
    // state is similar to a target state, please use Orig.IsSimilarTo(Target)
    public bool IsSimilarTo(trState target, ref bool isParaWrong){
      if(this.Behavior.Type != target.Behavior.Type){
        return false;
      }

      isParaWrong = false;
      if(this.Behavior.IsParameterized && target.IsCheckingParameter){ 
        if(this.Behavior.Type == trBehaviorType.MOODY_ANIMATION){
          isParaWrong = this.Behavior.Animation.id != target.Behavior.Animation.id;
        }
        else if(this.Behavior.isSoundBehaviour()
           ||this.Behavior.Type == trBehaviorType.EYERING){
          isParaWrong = this.BehaviorParameterValue != target.BehaviorParameterValue;
        }
        else{
          for(int i = 0; i<this.BehaviorParaValuesCount; ++i){
            if(!piMathUtil.withinSpecifiedEpsilon(this.GetNormalizedBehaviorParameterValue(i), target.GetNormalizedBehaviorParameterValue(i), 0.1f)){
              isParaWrong = true;
              break;
            }
          }
        } 
      }

      return !isParaWrong;
    }

    public void SetNormalizedBehaviorParameterValue(int index, float normalizedValue){
      if (!Behavior.IsParameterized) {
        WWLog.logError("This behavior is not parameterized.");
        return;
      }
      SetBehaviorParameterValue(index, Behavior.Denomalize(normalizedValue));
    }

    public string StateConfigText(piBotBase robot) {
      string result = "";
      if (Behavior.Type == trBehaviorType.SOUND_USER){
        piRobotType type = (robot != null ? robot.robotType : piRobotType.DASH);
        int firstSound = (int)trRobotSounds.Instance.GetCategory(Behavior.Type, type)[0].id;
        result = string.Format("<color=#7747abff>{0}</color>", (int)BehaviorParameterValue - firstSound + 1);
      }
      return result;

    }

    private float startMoment = 0;
    private float startTravelLinear = 0;
    private float startTravelAngular = 0;

    private piBotBo activeRobot = null;
    private piBotBo ActiveRobot {
      set {
        activeRobot = value;
      }

      get {
        if (activeRobot == null && IsOmniState) {
          if (piConnectionManager.Instance != null) {
            activeRobot = piConnectionManager.Instance.AnyConnectedBo;
          }
        }

        return activeRobot;
      }
    }

    private bool active = false;

    public trState() : this("noname") {}

    public trState(string name) {
      UserFacingName = name;
    }

    #region serialization
    protected override void IntoJson(JSONClass jsc) {
      jsc[TOKENS.USER_FACING_NAME       ] = UserFacingName;
      jsc[TOKENS.BEHAVIOR_ID            ] = Behavior.UUID;
      jsc[TOKENS.LAYOUT_POSITION        ] = trFactory.ToJson(LayoutPosition);
      jsc[TOKENS.ACTIVATION_TIME  ].AsInt = ActivationCount;
      jsc[TOKENS.MOOD             ].AsInt = (int)Mood;
      jsc[TOKENS.CHECK_STATE_PARA].AsBool = IsCheckingParameter;
      jsc[TOKENS.OBSCURED].AsBool = IsObscured;

      if (Behavior.IsParameterized) {
        jsc[TOKENS.PARAMETER_VALUE      ].AsFloat = BehaviorParameterValue; // backward compatibility
        JSONArray values = new JSONArray();
        for(int i = 0; i < behaviorParameterValues.Count; i++){
          values.Add(GetBehaviorParameterValue(i).ToString());          
        }
        jsc[TOKENS.PARAMETER_VALUES] = values;
      }

      base.IntoJson(jsc);
    }

    protected override void OutOfJson(JSONClass jsc) {
      base.OutOfJson(jsc);
      UserFacingName = jsc[TOKENS.USER_FACING_NAME];
      Behavior       = trFactory.GetItem<trBehavior>(jsc[TOKENS.BEHAVIOR_ID]);
      LayoutPosition = trFactory.FromJson(jsc[TOKENS.LAYOUT_POSITION].AsObject);
      
      if (Behavior == null) {
        WWLog.logError("Could not deserialize behavior: " + jsc.ToString(""));
      }
      
      if(jsc[TOKENS.ACTIVATION_TIME] != null){
        ActivationCount = jsc[TOKENS.ACTIVATION_TIME].AsInt;
      }

      if(jsc[TOKENS.MOOD] != null){
        Mood = (trMoodType)(jsc[TOKENS.MOOD].AsInt);
      }

      if(jsc[TOKENS.CHECK_STATE_PARA] != null){
        IsCheckingParameter = jsc[TOKENS.CHECK_STATE_PARA].AsBool;
      }

      if(jsc[TOKENS.OBSCURED] != null){
        IsObscured = jsc[TOKENS.OBSCURED].AsBool;
      }

      if (Behavior.IsParameterized) {
        if (jsc[TOKENS.PARAMETER_VALUES] != null) {
          behaviorParameterValues = new List<float>();
          JSONArray values = jsc[TOKENS.PARAMETER_VALUES].AsArray;
          if (values.Count == 0){
            behaviorParameterValues.Add(0); //set default value;
          } else {
            for(int i = 0; i < values.Count; i++){
              behaviorParameterValues.Add(values[i].AsFloat);
            }
          }
        }
        else {
          BehaviorParameterValue = jsc[TOKENS.PARAMETER_VALUE].AsFloat;          
        }
      }
      fixDeprecatedBehaviours();
    }

    private void fixDeprecatedBehaviours(){
      trBehaviorType validType = Behavior.Type;
      float parameter = float.NaN;
      switch(Behavior.Type){
      case trBehaviorType.MOVE_FB0:
        parameter = 0;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_LR0:
        parameter = 0;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_F1:
        parameter = trBehavior.ANGSPD_1;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_F2:
        parameter = trBehavior.LINSPD_2;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_F3:
        parameter = trBehavior.LINSPD_3;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_B1:
        parameter = -trBehavior.LINSPD_1;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_B2:
        parameter = -trBehavior.LINSPD_2;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_B3:
        parameter = -trBehavior.LINSPD_3;
        validType = trBehaviorType.MOVE_CONT_STRAIGHT;
        break;
      case trBehaviorType.MOVE_L1:
        parameter = trBehavior.ANGSPD_1;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_L2:
        parameter = trBehavior.ANGSPD_2;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_L3:
        parameter = trBehavior.ANGSPD_3;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_R1:
        parameter = -trBehavior.ANGSPD_1;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_R2:
        parameter = -trBehavior.ANGSPD_2;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;
      case trBehaviorType.MOVE_R3:
        parameter = -trBehavior.ANGSPD_3;
        validType = trBehaviorType.MOVE_CONT_SPIN;
        break;

      case trBehaviorType.SOUND_USER_1:
        parameter = 1000;
        validType = trBehaviorType.SOUND_USER;
        break;
      case trBehaviorType.SOUND_USER_2:
        parameter = 1001;
        validType = trBehaviorType.SOUND_USER;
        break;
      case trBehaviorType.SOUND_USER_3:
        parameter = 1002;
        validType = trBehaviorType.SOUND_USER;
        break;
      case trBehaviorType.SOUND_USER_4:
        parameter = 1003;
        validType = trBehaviorType.SOUND_USER;
        break;
      case trBehaviorType.SOUND_USER_5:
        parameter = 1004;
        validType = trBehaviorType.SOUND_USER;
        break;
      }
      
      if (!float.IsNaN(parameter)){
        Behavior.Type = validType;
        NormalizedBehaviorParameterValue = parameter;
      }
    }

    #endregion serialization

    public override string ToString () {
      return string.Format ("[trState: UserFacingName={0}, Active={1}, Behaviour={2}, Transitions={3}, LayoutPosition={4}, IsOmniState={5}, BehaviorParameterValue={6}",
                            UserFacingName, Active, Behavior, OutgoingTransitions, LayoutPosition, IsOmniState, BehaviorParameterValue);
    }

    public string ToString(bool active, piBotBase robot) {
      string name = UserFacingName;

      if (active) {
        name = name.ToUpper();
      }

      string ret = "";
      ret += "( ";
      ret += active ? UserFacingName.ToUpper() : UserFacingName;
      ret += " -- ";
      ret += Behavior.Type.ToString().ToLower();
      ret += " )";
      ret += active ? " <------------------------------ active" : "";
      ret += "\n";

      foreach (trTransition trn in OutgoingTransitions) {
        ret += Indent;
        string s = trn.Trigger.Type.ToString();

        switch (trn.Trigger.conditionMatches(robot, this)) {
          case trTriggerConditionIsMet.UNKNOWN:
            s = "? " + s.ToLower();
            break;

          case trTriggerConditionIsMet.YES:
            s = "* " + s.ToUpper();
            break;

          case trTriggerConditionIsMet.NO:
            s = "  " + s.ToLower();
            break;
        }

        ret += s.PadRight(20) + " --> " + "( " + trn.StateTarget.UserFacingName + " )";
        ret += "\n";
      }

      ret += "\n";
      return ret;
    }

    public bool AddOutgoingTransition(trTransition transition) {
      if (OutgoingTransitions.Contains(transition) && !trTrigger.AllowMultiple(transition.Trigger.Type)) {
        WWLog.logError("adding duplicate transition to state. " + UserFacingName);
        return false;
      }

      if (transition.StateSource != this) {
        WWLog.logError("adding outgoing transition to wrong state. actualState:" + UserFacingName + ", transition expects:" + transition.StateSource.UserFacingName);
      }

      foreach (trTransition t2 in OutgoingTransitions) {
        if (trTrigger.AllowMultiple(t2.Trigger.Type)) {
          continue;
        }

        if (!transition.Trigger.Type.IsTriggerSet() && transition.Trigger.Type == t2.Trigger.Type) {
          WWLog.logError("adding transition w/ duplicate trigger to state. sourceState=" + UserFacingName + " trigger=" + t2.Trigger.Type.ToString() + " targetState=" + t2.StateTarget.UserFacingName);
          return false;
        }

        //TODO: add trigger set check
      }

      OutgoingTransitions.Add(transition);
      return true;
    }
    
    public trTransition GetTransition(piBotBase robot) {
      trTransition result = null;

      foreach (trTransition trn in OutgoingTransitions) {
        if (trn.Trigger.Evaluate(robot, this)) {
          result = trn;

          if (trn.Trigger.Type == trTriggerType.RANDOM) {
            List<trTransition> options = new List<trTransition>();

            foreach (var item in OutgoingTransitions) {
              if (item.Trigger.Type == trn.Trigger.Type) {
                options.Add(item);
              }
            }

            result = options[Random.Range(0, options.Count)];
          }

          break;
        }
      }

      return result;
    }

    public void BehaveContinuous(piBotBase robot) {
      Behavior.BehaveContinuous(robot);
    }

    public bool Active {
      get {
        return active;
      }
    }

    public void Tare(piBotBase robot) {
      startMoment = Time.time;
      ActiveRobot = (piBotBo)robot;

      if (ActiveRobot != null) {
        startTravelLinear = ActiveRobot.TotalTravelLinear;
        startTravelAngular = ActiveRobot.TotalTravelAngular;
      }
    }

    public void SetActive(bool value, piBotBase robot, trMoodType mood = trMoodType.NO_CHANGE) {
      if (value) {
        Tare(robot);

        if (!active) {
          Behavior.SetActive(true, robot, mood, behaviorParameterValues);
        }
        
        ResetTriggers(robot);
      }
      else {
        if (active) {
          Behavior.SetActive(false, robot, mood, behaviorParameterValues);
        }
        
        ActiveRobot = null;
      }
      
      active = value;
    }

    public void SetActive(bool value, piBotBase robot, ref trMoodType mood) {
      // if this state has a meaningful mood value, set the value of the passed-in mood.
      // this is how the main program gets the mood updated.
      if(value && Mood != trMoodType.NO_CHANGE){
        mood = Mood;
      }
      SetActive(value, robot, mood);
    }
    
    // prime/unprime triggers
    public void ResetTriggers(piBotBase robot) {
      foreach (trTransition trTrn in OutgoingTransitions) {
        trTrn.Trigger.Evaluate(robot, this);
      }
    }

    public float TimeInState {
      get {
        if (active || IsOmniState) {
          return Time.time - startMoment;
        } else {
          return 0;
        }
      }
      set {
        startMoment = Time.time - value;
      }
    }

    public float TravelInStateLinear {
      get {
        float result = 0;

        if (ActiveRobot != null) {
          result = ActiveRobot.WheelLeft.encoderDistance.Value - startTravelLinear;
        }

        return result;
      }
    }

    public float TravelInStateAngular {
      get {
        float result = 0;

        if (ActiveRobot != null) {
          result = (ActiveRobot.BodyPoseSensor.radians - startTravelAngular) * Mathf.Rad2Deg;
        }

        return result;
      }
    }

    // this value is NOT linear velocity!
    public float AbsSpeedFiltered {
      get {
        float result = 0;
        
        if (ActiveRobot != null) {
          result = ActiveRobot.AbsSpeedFiltered;
        }
        
        return result;
      }
    }
    
    public float LinearSpeedFiltered {
      get {
        float result = 0;
        
        if (ActiveRobot != null) {
          result = ActiveRobot.LinearSpeedFiltered;
        }
        
        return result;
      }
    }
    
    public bool HasOutgoingTransitionWithTriggerType(trTriggerType trTT) {
      foreach (trTransition trTrn in OutgoingTransitions) {
        if (trTrn.Trigger.Type == trTT) {
          return true;
        }
      }
      return false;
    }

    public List<trTransition> AllOutgoingTransitionsToState(trState target) {
      List<trTransition>ret = new List<trTransition>();

      foreach (trTransition ttr in OutgoingTransitions) {
        if (ttr.StateTarget == target) {
          ret.Add(ttr);
        }
      }

      return ret;
    }

    // two transitions with the same SourceState and the same TargetState are called "twins".
    // this utility function provides a deterministic ordering of twins,
    // and given an outgoing transition of a state returns that transitions index in the list of twins.
    // note, indexing starts with zero.
    // so if there is only one transition from A to B, the twinIndex of that transition is 0.
    public int TwinOutgoingTransitionIndex(trTransition theTwin) {
      List<trTransition> twins = AllOutgoingTransitionsToState(theTwin.StateTarget);

      for (int n = 0; n < twins.Count; ++n) {
        if (twins[n] == theTwin) {
          return n;
        }
      }

      WWLog.logError("state machine is corrupt.");
      return -1;
    }
    
    // TUR-850 
    public void NormalizeTransitionOrdering() {
      OutgoingTransitions.Sort(
        delegate(trTransition t1, trTransition t2) {
          int t1v = t1.Trigger.Type.evaluationOrder();
          int t2v = t2.Trigger.Type.evaluationOrder();
          return t1v.CompareTo(t2v);
        }
      );
    }
  }
}





