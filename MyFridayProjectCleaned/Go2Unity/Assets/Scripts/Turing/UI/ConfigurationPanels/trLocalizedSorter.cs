using UnityEngine;
using System;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Turing {
  public class trLocalizedSorter{

    private readonly CultureInfo _originalCulture;
    private readonly CultureInfo _sortCulture;
    private readonly Regex _numericRegex;
    private readonly Regex _alphaRegex;

    public trLocalizedSorter (){
      _originalCulture = Thread.CurrentThread.CurrentCulture;
      switch (trMultivariate.Instance.getOptionValue(trMultivariate.trAppOption.LANGUAGE)){
      case trMultivariate.trAppOptionValue.en_US:
        _sortCulture = new CultureInfo("en-US");
        break;
      case trMultivariate.trAppOptionValue.de_DE:
        _sortCulture = new CultureInfo("de-DE");
        break;
      case trMultivariate.trAppOptionValue.ko_KR:
        _sortCulture = new CultureInfo("ko-KR");
        break;
      case trMultivariate.trAppOptionValue.zh_CN:
        _sortCulture = new CultureInfo("zh-CN");
        break;
      default:
        _sortCulture = Thread.CurrentThread.CurrentCulture;
        break; 
      }
      _numericRegex = new Regex(@"^(\d+)(.*)");
      _alphaRegex = new Regex(@"^(\D+)(.*)");
    }

    public int compare(string x, string y) {
      if (x.CompareTo(y) == 0) {
        return 0;
      }

      // Changing the CurrentCulture value affects string sorting order according to language 
      Thread.CurrentThread.CurrentCulture = _sortCulture;

      // Pop alpha or numeric tokens from the heads of each strings and compare
      Match matchNumericX, matchAlphaX;
      Match matchNumericY, matchAlphaY;
      int result = 0;
      string sx = x;
      string sy = y;
      while ((sx.Length > 0) && (sy.Length > 0)){
        matchNumericX = _numericRegex.Match(sx);
        matchNumericY = _numericRegex.Match(sy);
        // Use default compare on strings if one starts with alpha and one with numeric
        if (matchNumericX.Success != matchNumericY.Success){
          result = sx.CompareTo(sy);
          break;
        }

        // If both strings start with numbers, sort on the int values
        if (matchNumericX.Success){
          int ix = Int32.Parse(matchNumericX.Groups[1].Value);
          int iy = Int32.Parse(matchNumericY.Groups[1].Value);
          if (ix != iy){
            result = (ix < iy) ? -1 : 1;
            break;
          } else{
            sx = matchNumericX.Groups[2].Value;
            sy = matchNumericY.Groups[2].Value;
          }
        } else{
          // both strings start with alpha, use default compare
          matchAlphaX = _alphaRegex.Match(sx);
          matchAlphaY = _alphaRegex.Match(sy);
          result = matchAlphaX.Groups[1].Value.CompareTo(matchAlphaY.Groups[1].Value);
          if (result != 0){
            break;
          }
          sx = matchAlphaX.Groups[2].Value;
          sy = matchAlphaY.Groups[2].Value;
        }
      }

      // if the alphanumeric comparator says the strings are equal
      // default to sorting by string length
      if (result == 0) {
        if (sx.Length != sy.Length) {
          result = (sx.Length < sy.Length) ? -1 : 1;
        } else if (x.Length != y.Length) {
          result = (x.Length < y.Length) ? -1 : 1;
        }
      }

      // Restore the original culture value to avoid side-effects (e.g. date strings)
      Thread.CurrentThread.CurrentCulture = _originalCulture;
      return result;
    }
  }
}

