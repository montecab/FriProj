using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

public class wwPOEntry{
  public List<string> translations;
  public List<string> comments;
  public wwPOEntry(){
    translations = new List<string>();
    comments = new List<string>();
  }
  public wwPOEntry(wwPOEntry entry){
    translations = new List<string>();
    comments = new List<string>();
    foreach (string translation in entry.translations){
      translations.Add(translation);
    }
    foreach (string comment in entry.comments){
      comments.Add(comment);
    }
  }
  public bool isEquivalent(wwPOEntry otherEntry) {
    if (translations.Count != otherEntry.translations.Count) {
      return false;
    }
    for (int n = 0; n < translations.Count; ++n) {
      if (translations[n] != otherEntry.translations[n]) {
        return false;
      }
    }

    return true;
  }
}

public enum PluralFormulaType{
  // from https://www.gnu.org/software/gettext/manual/html_node/Plural-forms.html
  ONLY_ONE_FORM,                              //nplurals=1; plural=0;
  TWO_FORMS_SINGULAR_USED_FOR_ONE_ONLY,       //nplurals=2; plural=n != 1;
  TWO_FORMS_SINGULAR_USED_FOR_ZERO_AND_ONE,   //nplurals=2; plural=n==0 || n==1 ? 0: 1;
  THREE_FORMS_SPECIAL_CASE_FOR_ZERO,          //nplurals=3; plural=n==0 ? 0 : n==1 ? 1 : 2;
}

public static class wwPOStringExtensions{
  public static string Unescape(this string s){
    return s
      .Replace("\\\""  , "\"")    //   \"     -> "
      .Replace("\\n"   , "\n")    //   \n     -> ⏎
      .Replace("\\t"   , "\t")    //   \t     -> ↹
      .Replace("\\\\"  , "\\")    //   \\     -> \  // must be done after other backslash replacements
      .Replace("&quot;", "\"")    //   &quot; -> "
      ;
  }
}

public static class PluralFormulaExtensions{
  private static Dictionary<PluralFormulaType, string> lookup = new Dictionary<PluralFormulaType, string>(){
    {PluralFormulaType.ONLY_ONE_FORM,                             "nplurals=1; plural=0;"},
    {PluralFormulaType.TWO_FORMS_SINGULAR_USED_FOR_ONE_ONLY,      "nplurals=2; plural=n != 1;"},
    {PluralFormulaType.TWO_FORMS_SINGULAR_USED_FOR_ZERO_AND_ONE,  "nplurals=2; plural=n==0 || n==1 ? 0: 1;"},
    {PluralFormulaType.THREE_FORMS_SPECIAL_CASE_FOR_ZERO,         "nplurals=3; plural=n==0 ? 0 : n==1 ? 1 : 2;"},
  };

  public static PluralFormulaType ToFormulaType(this string s){
    PluralFormulaType type = PluralFormulaType.ONLY_ONE_FORM;
    foreach (PluralFormulaType key in lookup.Keys){
      if (lookup[key] == s){
        type = key;
      }
    }
    return type;
  }

  public static string ToFormulaString(this PluralFormulaType type){
    string result = "";
    if (lookup.ContainsKey(type)){
      result = lookup[type];
    }
    return result;
  }

  public static int GetPluralIndex(this PluralFormulaType type, int pluralVal){
    int index = -1;
    if (type == PluralFormulaType.ONLY_ONE_FORM){
      index = 0;
    } else if (type == PluralFormulaType.TWO_FORMS_SINGULAR_USED_FOR_ONE_ONLY){
      index = (pluralVal!=1)?1:0;
    } else if(type == PluralFormulaType.TWO_FORMS_SINGULAR_USED_FOR_ZERO_AND_ONE){
      index = (pluralVal==0||pluralVal==1)?0:1;
    } else if(type == PluralFormulaType.THREE_FORMS_SPECIAL_CASE_FOR_ZERO){
      index = (pluralVal==0)?0:(pluralVal==1)?1:2;
    }
    return index;
  }
}

public class wwPO{

  private Dictionary<string, wwPOEntry> _dictionary;
  private string _lang;
  private int _version;
  private PluralFormulaType _formula;

  public Dictionary<string, wwPOEntry> dictionary{ get { return _dictionary;}}
  public string lang{get{return _lang;}}
  public int version{get{return _version;}}
  public PluralFormulaType formula{get{return _formula;}}

  public wwPO(){
    _dictionary = new Dictionary<string, wwPOEntry>();
    _lang = "en";
    _version = 0;
    _formula = PluralFormulaType.TWO_FORMS_SINGULAR_USED_FOR_ONE_ONLY;
  }

  public void PrintAllKeys(){
    string output = "*** Print all keys ***\n";
    foreach (string key in _dictionary.Keys)
    {
      output += key+"\n";
    }
    WWLog.logInfo(output);
  }

  public override string ToString(){
    StringBuilder sb = new StringBuilder();
    //Header
    sb.AppendLine("#. this is the header");
    sb.AppendLine("#. language=" + _lang);
    sb.AppendLine("#. strings_version=" + (_version + 1).ToString());
    sb.AppendLine("# smartling.placeholder_format_custom = \\{[0-9]+\\}");
    sb.AppendLine("msgid \"\"");
    sb.AppendLine("msgstr \"\"");
    sb.AppendLine("\"Content-Type: text/plain; charset=UTF-8\\n\"");
    sb.AppendLine("\"Content-Transfer-Encoding: 8bit\\n\"");
    sb.AppendLine("\"Plural-Forms: "+_formula.ToFormulaString()+"\\n\"");
    sb.AppendLine("");
    //Entry
    foreach (string key in _dictionary.Keys){
      wwPOEntry entry = _dictionary[key];
      //Comments
      foreach (string comment in entry.comments){
        sb.AppendLine("#: "+comment);
      }
      //Translations
      if (entry.translations.Count > 1){
        sb.AppendLine("msgid_plural \"" + key + "\"");
        for (int i = 0; i < entry.translations.Count; i++){
          sb.AppendLine("msgstr[" + i + "] \"" + entry.translations[i] + "\"");
        }
      } 
      else if (entry.translations.Count == 1){
        sb.AppendLine("msgid \"" + key + "\"");
        sb.AppendLine("msgstr \"" + entry.translations[0] + "\"");
      } 
      else{
        WWLog.logError("key:"+key+" doesn't have translations");
      }
      sb.AppendLine("");
    }
    return sb.ToString();
  }

  private static string everythingInsideTheOutermostQuotes(string s) {
    int q1 = s.IndexOf    ('"');
    int q2 = s.LastIndexOf('"');
    if ((q2 < 0) || (q1 == q2)) {
      WWLog.logError("bad quotes in string: " + s);
      return null;
    }
    return s.Substring(q1 + 1, q2 - q1 - 1);
  }

  private static void test_everythingInsideTheOutermostQuotes() {
    List<string> cases = new List<string>() {
      "hello \"there\" world!!"       , "there",
      "hello \"the \"real\"\" world!!", "the \"real\"",
      "hello \" world!!"              , null,
      "hello world!!"                 , null,
      "\"hello\" world"               , "hello",
      "hello \"world\""               , "world",
      "\"world\""                     , "world",
      "\"\""                          , "",
    };

    for (int n = 0; n < cases.Count; n += 2) {
      string expected = cases[n+1];
      string actual   = everythingInsideTheOutermostQuotes(cases[n]);
      if (actual != expected) {
        WWLog.logError("error: everythingInsideTheOutermostQuotes. expected \"" + expected + "\" got: \"" + actual + "\"");
      }
    }
  }

  private static void test_parse() {
    string poData = @"# smartling.placeholder_format_custom = (\{+[0-9]+\}+|ROBOT|COLOR)
msgid """"
msgstr """"
""Content-Type: text/plain; charset=UTF-8\n""
""Content-Transfer-Encoding: 8bit\n""
""Plural-Forms: nplurals=2; plural=n != 1;\n""

#: it came from beyond
# plain comment
msgid ""singular single-line key""
msgstr ""singular single-line value""

msgid ""singular multi-""
""line ""
""key""
msgstr ""singular multi-""
""line ""
""value""

msgid ""plural single-line key""
msgid_plural ""plural single-line key""
msgstr[0] ""plural single-line value 0""
msgstr[1] ""plural single-line value 1""

msgid ""plural single-line key multi-line value""
msgid_plural ""plural single-line key multi-line value""
msgstr[0] ""plural single-line key multi-""
""line value 0""
msgstr[1] ""plural single-line key single-line value 1""

msgid ""plural multi-""
""line key""
msgid_plural ""plural multi-""
""line key""
msgstr[0] ""plural multi-""
""line value 0""
msgstr[1] ""plural single-line value 1""
";

    Dictionary<string, wwPOEntry> dict = new wwPO().Parse(poData);

    string key;
    string val0;
    string val1;

    key  = "singular single-line key";
    val0 = "singular single-line value";
    if (piTest.assertTrue(dict.ContainsKey(key), "PO parse failed: " + key)) {
      piTest.assertTrue(dict[key].translations[0] == val0, "PO parse failed: " + val0);
    }

    key  = "singular multi-line key";
    val0 = "singular multi-line value";
    if (piTest.assertTrue(dict.ContainsKey(key), "PO parse failed: " + key)) {
      piTest.assertTrue(dict[key].translations[0] == val0, "PO parse failed: " + val0);
    }

    key  = "plural single-line key";
    val0 = "plural single-line value 0";
    val1 = "plural single-line value 1";
    if (piTest.assertTrue(dict.ContainsKey(key), "PO parse failed: " + key)) {
      piTest.assertTrue(dict[key].translations[0] == val0, "PO parse failed: " + val0);
      piTest.assertTrue(dict[key].translations[1] == val1, "PO parse failed: " + val1);
    }


    key = "plural single-line key multi-line value";
    val0 = "plural single-line key multi-line value 0";
    val1 = "plural single-line key single-line value 1";
    if (piTest.assertTrue(dict.ContainsKey(key), "PO parse failed: " + key)) {
      piTest.assertTrue(dict[key].translations[0] == val0, "PO parse failed: " + val0);
      piTest.assertTrue(dict[key].translations[1] == val1, "PO parse failed: " + val1);
    }

    key  = "plural multi-line key";
    val0 = "plural multi-line value 0";
    val1 = "plural single-line value 1";
    if (piTest.assertTrue(dict.ContainsKey(key), "PO parse failed: " + key)) {
      piTest.assertTrue(dict[key].translations[0] == val0, "PO parse failed: " + val0);
      piTest.assertTrue(dict[key].translations[1] == val1, "PO parse failed: " + val1);
    }
  }


  public static void test() {
    test_everythingInsideTheOutermostQuotes();
    test_parse();
  }

  public Dictionary<string, wwPOEntry> Parse(string s){    
    // todo @jason - should this be new()ing _dictionary ?
    //               seems weird. a second call to Parse() will wipe that out, won't it ?
    _dictionary = new Dictionary<string, wwPOEntry>();
    string[] lines = s.Split(new []{ '\n' });
    int index = -1;
    string line = "";
    string msgId = null;
    string msgStr = null;
    wwPOEntry entry = new wwPOEntry();

    // we create a new entry after concluding a msgstr segment

    while (++index < lines.Length && (line = lines[index])!=null){

      line = wwPOStringExtensions.Unescape(line);

      if (line.StartsWith("#. language=")){
        _lang = line.Substring(12, line.Length - 12);
      }
      else if (line.StartsWith("#. strings_version=")){
        _version = int.Parse(line.Substring(19, line.Length - 19));
      }
      else if (line.StartsWith("\"Plural-Forms: ")){
        _formula = line.Substring(15, line.Length - 18).ToFormulaType();
      }
      else if (line.StartsWith("msgid")){ // note matches msgid and msgid_plural
        if (msgStr != null) {
          // this can begin a new entry. close-out old entry
          entry.translations.Add(msgStr);
          AddEntry(msgId, entry);
          entry = new wwPOEntry();
        }
        msgId = everythingInsideTheOutermostQuotes(line);
        msgStr = null;
      }
      else if (line.StartsWith("#")){
        if (msgStr != null) {
          // this can begin a new entry. close-out old entry
          entry.translations.Add(msgStr);
          AddEntry(msgId, entry);
          entry = new wwPOEntry();
        }
        entry.comments.Add(line);
        msgId = null;
        msgStr = null;
      }
      else if (line.StartsWith("msgstr \"")) {
        msgStr = everythingInsideTheOutermostQuotes(line);
      }
      else if (line.StartsWith("msgstr[")) {
        if (msgStr != null) {
          entry.translations.Add(msgStr);
        }
        msgStr = everythingInsideTheOutermostQuotes(line);
      }
      else if (line.StartsWith("\"")) {
        // continue msgid or msgstr
        if (msgStr != null) {
          msgStr = msgStr + everythingInsideTheOutermostQuotes(line);
        }
        else if (msgId != null) {
          msgId = msgId + everythingInsideTheOutermostQuotes(line);
        }
        else {
          WWLog.logError("malformed PO file on line: " + line);
        }
      }
    }

    if (msgStr != null) {
      // close-out old entry
      entry.translations.Add(msgStr);
      AddEntry(msgId, entry);
    }

    return _dictionary;
  }

  // todo: handle plurals
  public void AddEntry(string key, string translation, List<string> comments = null){
    wwPOEntry entry = new wwPOEntry();
    entry.translations.Add(translation);
    if (comments!=null && comments.Count>0){
      foreach (string comment in comments){
        entry.comments.Add(comment);
      }
    }

    AddEntry(key, entry);
  }

  private void AddEntry(string key, wwPOEntry entry) {
    if (_dictionary.ContainsKey(key)) {
      if (!_dictionary[key].isEquivalent(entry)) {
        WWLog.logError("conflicting entries for string: \"" + key + "\"");
        return;
      }
    }
    else if ((entry.translations.Count > 0) && (!string.IsNullOrEmpty(key))) {
      _dictionary.Add(key, entry);
    }
  }

  public void Merge(wwPO po){
    if (_dictionary.Count == 0){  //if wwPo is empty
      _lang = po.lang;
      _formula = po.formula;
    }
    if (_lang == po.lang && _formula == po.formula){
      foreach (string key in po.dictionary.Keys){
        if (!_dictionary.ContainsKey(key)){
          _dictionary.Add(key, new wwPOEntry(po.dictionary[key]));
        } else{
          if (!_dictionary[key].isEquivalent(po.dictionary[key])) {
            WWLog.logError("Duplicate PO keys with different values. lang: " + _lang + " key: " + key);
          }
          else {
            // don't log this. it's not a problem.
            // WWLog.logWarn("Found duplicated key in PO dictionary with matching values. lang: " + _lang + " key:"+key);
          }
        }
      }
    } else{
      WWLog.logError("Can't merge two different languages or formulas");
    }
  }
}
