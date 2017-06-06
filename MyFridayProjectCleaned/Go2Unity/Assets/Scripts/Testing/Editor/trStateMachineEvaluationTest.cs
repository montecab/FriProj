using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework;
using Turing;


[TestFixture]
public class trStateMachineEvaluationTest{

  private trStateMachineEvaluation _eval;

  [SetUp]
  public void SetUp(){
    _eval = new trStateMachineEvaluation();
  }

  private void AssertEquivalent(trProgram p1, trProgram p2){
    Assert.True(_eval.IsEquivalent(p1, p2));
    Assert.True(_eval.IsEquivalent(p2, p1));
  }

  private void AssertNotEquivalent(trProgram p1, trProgram p2){
    Assert.False(_eval.IsEquivalent(p1, p2));
    Assert.False(_eval.IsEquivalent(p2, p1));
  }

  [Test]
  public void EmptyProgramsAreEquivalent(){
    Assert.True(_eval.IsEquivalent(new trProgram(), new trProgram()));
  }

  [Test]
  public void EmptyProgramNotEquivalentToNonEmpty(){
    trProgram empty = new trProgram();
    TestProgramBuilder tiny = new TestProgramBuilder();
    tiny.AddState("Red", trBehaviorType.COLOR_RED);
    AssertNotEquivalent(empty, tiny.Program);
  }

  [Test]
  public void ProgramEquivalentToSelf(){
    trProgram tiny = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).Program;
    Assert.True(_eval.IsEquivalent(tiny, tiny));
  }

  [Test]
  public void TinyEquivalentProgramTest(){
    trProgram p1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).Program;
    trProgram p2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).Program;
    AssertEquivalent(p1, p2);
  }

  [Test]
  public void StateCountMismatchTest(){
    trProgram p1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).Program;
    trProgram p2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE).Program;
    AssertNotEquivalent(p1, p2);
  }

  [Test]
  public void StartingStateMatchTest(){
    trProgram p1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE).SetStart("Red").Program;
    trProgram p2 = (new TestProgramBuilder()).AddState("Blue", trBehaviorType.COLOR_BLUE).AddState("Red", trBehaviorType.COLOR_RED).SetStart("Red").Program;
    AssertEquivalent(p1, p2);
  }

  [Test]
  public void StartingStateMismatchTest(){

    trProgram p1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE).SetStart("Red").Program;
    trProgram p2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE).SetStart("Blue").Program;
    AssertNotEquivalent(p1, p2);
  }

  [Test]
  public void SingleTransitionMatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue");
    AssertEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void SingleTransitionMismatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Blue", "Red");
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void StateAddOrderIrrelevanceTest(){
    trProgram p1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE).Program;
    trProgram p2 = (new TestProgramBuilder()).AddState("Blue", trBehaviorType.COLOR_BLUE).AddState("Red", trBehaviorType.COLOR_RED).Program;
    AssertEquivalent(p1, p2);
  }

  [Test]
  public void OmniMatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetOmni("Red");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetOmni("Red");
    AssertEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void OmniMismatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetOmni("Red");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetOmni("Blue");
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void BidirectionalTransitionTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    AssertEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void BidirectionalTransitionMismatchedStartTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Blue").AddTransition("Red", "Blue").AddTransition("Blue", "Red");
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void TriggerTypeMismatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1);
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void ParallelTransitionMatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    AssertEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void ParallelTransitionMismatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_3);
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void SameTransitionDifferentStatesTest(){
    TestProgramBuilder b1 = new TestProgramBuilder();
    b1.AddState("Red", trBehaviorType.COLOR_RED).AddState("Green", trBehaviorType.COLOR_GREEN).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Green", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    TestProgramBuilder b2 = new TestProgramBuilder();
    b2.AddState("Red", trBehaviorType.COLOR_RED).AddState("Green", trBehaviorType.COLOR_GREEN).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Green", trTriggerType.BUTTON_2).AddTransition("Red", "Blue", trTriggerType.BUTTON_1);
    AssertNotEquivalent(b1.Program, b2.Program);
  }

  [Test]
  public void TransitionCountMismatchTest(){
    TestProgramBuilder b1 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b1.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1).AddTransition("Red", "Blue", trTriggerType.BUTTON_2);
    TestProgramBuilder b2 = (new TestProgramBuilder()).AddState("Red", trBehaviorType.COLOR_RED).AddState("Blue", trBehaviorType.COLOR_BLUE);
    b2.SetStart("Red").AddTransition("Red", "Blue", trTriggerType.BUTTON_1);
    AssertNotEquivalent(b1.Program, b2.Program);
  }
}
