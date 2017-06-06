using UnityEngine;
using System.Collections;
using NUnit.Framework;
using Turing;

[TestFixture]
public class trLocalizedSorterTest{

  private trLocalizedSorter _localizedSorter;

  [SetUp]
  public void SetUp() {
    _localizedSorter = new trLocalizedSorter();
  }

  [TestCase("1", "2", -1)]
  [TestCase("2", "1", 1)]
  [TestCase("10", "2", 1)]
  [TestCase("2", "10", -1)]
  [TestCase("a", "b", -1)]
  [TestCase("b", "a", 1)]
  [TestCase("Custom Sound 1", "Custom Sound 2", -1)]
  [TestCase("Custom Sound 2", "Custom Sound 1", 1)]
  [TestCase("Custom Sound 10", "Custom Sound 9", 1)]
  [TestCase("Custom Sound 9", "Custom Sound 10", -1)]
  [TestCase("Custom Sound 1", "Custom Sound 10", -1)]
  [TestCase("Custom Sound 10", "Custom Sound 1", 1)]
  [TestCase("8th Sound", "11th Sound", -1)]
  [TestCase("11th Sound", "8th Sound", 1)]
  [TestCase("a150b", "a150", 1)]
  [TestCase("a150", "a150b", -1)]
  [TestCase("a15b", "a150b", -1)]
  [TestCase("a150b", "a15b", 1)]
  [TestCase("Charge #2", "Lets go! #1", -1)]
  [TestCase("Lets go! #1", "Charge #2", 1)]
  public void SortingTest(string x, string y, int expectedValue) {
    int result = _localizedSorter.compare(x, y);
    Assert.AreEqual(expectedValue, result);
  }
  
}
