using System.Collections.Generic;
using NUnit.Framework;
using Turing;

[TestFixture]
public class trProgramTest{
  [SetUp]
  public void SetUp(){
    trDataManager.Instance.Init();
  }

  [Test]
  public void ProgramNotInitializedTest(){
    trProgram p = new trProgram();
    Assert.False(p.IsInitialized);
    Assert.Null(p.StateCurrent);
    Assert.Null(p.StateOmni);
  }

  [Test]
  public void NoThumbnailForNewProgramTest(){
    trProgram p = new trProgram();
    Assert.False(p.IsThumbnailExist);
  }

  [Test]
  public void FileNameWordsTest(){
    Assert.Greater(trProgram.FileNameWord.Count, 1);
  }

  [Test]
  public void RobotProgramInitializationTest(){
    trProgram p = trProgram.NewProgram(piRobotType.DASH);
    Assert.AreEqual(piRobotType.DASH, p.RobotType);
  }

  [Test]
  public void UniqueFilenameTest(){
    HashSet<string> filenames = new HashSet<string>();
    List<trProgram> programs = new List<trProgram>();
    const int testCount = 1000;
    for(int i = 0; i < testCount; i++){
      trProgram p = trProgram.NewProgram(piRobotType.DASH);
      trDataManager.Instance.AppSaveInfo.Programs.Add(p);
      filenames.Add(p.UserFacingName);
    }
    foreach(trProgram p in programs){
      trDataManager.Instance.AppSaveInfo.Programs.Remove(p);
    }
    Assert.AreEqual(testCount, filenames.Count);
  }

  [Test]
  public void AddStateTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED);
    trState s = b.GetState("Red");
    Assert.True(b.Program.UUIDToStateTable.ContainsValue(s));
  }

  [Test]
  public void SetStateBehaviourTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED);
    trState s = b.GetState("Red");
    trBehavior oldBehavior = s.Behavior;
    trBehavior newBehavior = new trBehavior(trBehaviorType.COLOR_BLUE);
    b.Program.setStateBehaviour(s, newBehavior);
    Assert.AreEqual(trBehaviorType.COLOR_BLUE, s.Behavior.Type);
    Assert.True(b.Program.UUIDToBehaviorTable.ContainsValue(newBehavior));
    Assert.False(b.Program.UUIDToBehaviorTable.ContainsValue(oldBehavior));
  }

  [Test]
  public void AddTransitionTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue");
    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");
    trTransition t = b.GetTransition(0);
    Assert.AreSame(sRed, t.StateSource);
    Assert.AreSame(sBlue, t.StateTarget);
  }

  [Test]
  public void RemoveTransitionTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue");

    trTransition t = b.GetTransition(0);
    Assert.True(b.Program.UUIDToTransitionTable.ContainsValue(t));

    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");
    Assert.AreEqual(1, sRed.AllOutgoingTransitionsToState(sBlue).Count);

    b.Program.RemoveTransition(t);
    Assert.False(b.Program.UUIDToTransitionTable.ContainsValue(t));
    Assert.AreEqual(0, sRed.AllOutgoingTransitionsToState(sBlue).Count);
  }

  [Test]
  public void AllStateTransitionsTest(){
    TestProgramBuilder b = new TestProgramBuilder();
    b.AddState("Red", trBehaviorType.COLOR_RED).AddState("Green", trBehaviorType.COLOR_GREEN).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red");
    b.AddTransition("Red", "Green", trTriggerType.BUTTON_1).AddTransition("Green", "Blue", trTriggerType.BUTTON_2).AddTransition("Blue", "Red", trTriggerType.BUTTON_3);

    trState sRed = b.GetState("Red");
    List<trTransition> t = b.Program.AllStateTransitions(sRed);
    Assert.AreEqual(2, t.Count);
    Assert.Contains(b.GetTransition(0), t);
    Assert.Contains(b.GetTransition(2), t);
  }

  [Test]
  public void RemoveAllStateTransitionsTest(){
    TestProgramBuilder b = new TestProgramBuilder();
    b.AddState("Red", trBehaviorType.COLOR_RED).AddState("Green", trBehaviorType.COLOR_GREEN).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red");
    b.AddTransition("Red", "Green", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);

    trState sRed = b.GetState("Red");
    Assert.AreEqual(2, sRed.OutgoingTransitions.Count);
    b.Program.RemoveAllStateTransitions(sRed);
    Assert.AreEqual(0, sRed.OutgoingTransitions.Count);
  }

  [Test]
  public void SmallProgramValidationTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue");
    Assert.True(b.Program.Validate());
  }

  [Test]
  public void TransitionValidationTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue");
    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");
    trTransition t = b.GetTransition(0);
    Assert.True(b.Program.Validate());

    t.StateSource = null;
    Assert.False(b.Program.Validate());
    t.StateSource = sRed;
    Assert.True(b.Program.Validate());
    t.StateTarget = null;
    Assert.False(b.Program.Validate());
    t.StateTarget = sBlue;

    trState sInvalid = new trState();
    t.StateSource = sInvalid;
    Assert.False(b.Program.Validate());
    t.StateSource = sRed;
    Assert.True(b.Program.Validate());
    t.StateTarget = sInvalid;
    Assert.False(b.Program.Validate());
    t.StateTarget = sBlue;

    sRed.OutgoingTransitions.Remove(t);
    Assert.False(b.Program.Validate());
    sRed.OutgoingTransitions.Add(t);

    Assert.True(b.Program.Validate());
    t.Trigger = null;
    Assert.False(b.Program.Validate());
  }

  [Test]
  public void UniqueUUIDValidationTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");
    Assert.True(b.Program.Validate());

    string uuid = sBlue.UUID;
    sBlue.UUID = sRed.UUID;
    Assert.False(b.Program.Validate());
    sBlue.UUID = uuid;
    Assert.True(b.Program.Validate());

    uuid = sBlue.Behavior.UUID;
    sBlue.Behavior.UUID = sRed.Behavior.UUID;
    Assert.False(b.Program.Validate());
    sBlue.Behavior.UUID = uuid;
    Assert.True(b.Program.Validate());

    trTransition t0 = b.GetTransition(0);
    trTransition t1 = b.GetTransition(1);
    uuid = t1.UUID;
    t1.UUID = t0.UUID;
    Assert.False(b.Program.Validate());
    t1.UUID = uuid;
    Assert.True(b.Program.Validate());
  }

  [Test]
  public void StartStateValidationTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.AddTransition("Red", "Blue").AddTransition("Blue", "Red");

    Assert.False(b.Program.Validate());
    b.SetStart("Red");
    Assert.True(b.Program.Validate());

    b.Program.StateStart = new trState();
    Assert.False(b.Program.Validate());
    b.SetStart("Red");
    Assert.True(b.Program.Validate());
  }

  [Test]
  public void OmniStateValidationTest(){
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    trState sRed = b.GetState("Red");
    b.Program.StateOmni = sRed;
    Assert.True(b.Program.Validate());

    b.Program.StateOmni = new trState();
    Assert.False(b.Program.Validate());
    b.Program.StateOmni = sRed;
    Assert.True(b.Program.Validate());
  }

  [Test]
  public void CurrentStateTest() {
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");

    piBotBo bot = new piBotBo(wwUID.getUID(), "CurrentStateTest:bot", piRobotType.UNKNOWN);
    b.Program.SetState(sRed, bot);
    Assert.AreSame(sRed, b.Program.StateCurrent);

    b.Program.SetState(sBlue, bot);
    Assert.AreSame(sBlue, b.Program.StateCurrent);
  }

  [Test]
  public void ResetProgramTest() {
    TestProgramBuilder b = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Green", trBehaviorType.COLOR_GREEN).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b.AddTransition("Red", "Green", trTriggerType.BUTTON_1).AddTransition("Green", "Blue", trTriggerType.BUTTON_2).AddTransition("Blue", "Red", trTriggerType.BUTTON_3);
    trState sRed = b.GetState("Red");
    trState sBlue = b.GetState("Blue");
    trTransition t0 = b.GetTransition(0);
    trTransition t1 = b.GetTransition(1);
    trTransition t2 = b.GetTransition(2);
    piBotBo bot = new piBotBo(wwUID.getUID(), "ResetProgramTest:bot", piRobotType.UNKNOWN);

    b.SetStart("Red");
    b.SetOmni("Blue");
    b.Program.SetState(sBlue, bot);
    t0.Trigger.Primed = false;
    t1.Trigger.Primed = false;
    t2.Trigger.Primed = false;
    b.Program.Reset(bot);
    Assert.AreSame(sRed, b.Program.StateCurrent);
    Assert.True(t0.Trigger.Primed);
    Assert.False(t1.Trigger.Primed);
    Assert.True(t2.Trigger.Primed);
  }
}
