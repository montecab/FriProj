using UnityEngine;
using System.Collections;

public class piBotConstants_Internal {

	public const string SHELLCMD_AD_GET_MOT_PAN   = "AD GET MOT_PAN";
	public const string SHELLCMD_FETCH_PP2        = "CFG GET PP2";
	public const string SHELLCMD_FETCH_PP3        = "CFG GET PP3";
	public const string SHELLCMD_SET_PP1          = "CFG SET PP1";
	public const string SHELLCMD_SET_PP2          = "CFG SET PP2";
	public const string SHELLCMD_SET_PP3          = "CFG SET PP3";
	public const string SHELLCMD_SET_PP4          = "CFG SET PP4";
	public const string SHELLCMD_CFG_RELOAD_NVT2  = "CFG_RELOAD NVT2";
	public const string SHELLCMD_ERASE_PP3        = "DEL NRF PP3.EP1";
	
	public const string SHELLRESPONSE_NG          = "NG";
	public const string SHELLRESPONSE_OK          = "OK";
	
	public const int POT_MEASUREMENT_VARIANCE     = 150;
	public const int POT_MEASUREMENT_KEY0         = (1900 + 2200) / 2;
	public const int POT_MEASUREMENT_KEY4         = (1431 + 1732) / 2;
	public const int POT_MEASUREMENT_KEY7         = (1079 + 1380) / 2;
	
}
