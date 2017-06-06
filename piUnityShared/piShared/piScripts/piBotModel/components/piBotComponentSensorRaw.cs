using PI;

public class piBotComponentSensorRaw : piBotComponentBase2 {

  private const int sensorPacketNumBytes = 20;

  private byte[] data;

  public byte[] Data {
    get {
      return data;
    }
  }

  public override void handleState(WW.SimpleJSON.JSONClass jsComponent) {
    WW.SimpleJSON.JSONArray jsaData = jsComponent[piJSONTokens.WW_SENSOR_VALUE_DATA].AsArray;
    if (jsaData == null) {
      WWLog.logError("improper json in raw sensor: " + jsComponent.ToString());
      return;
    }
    if (jsaData.Count != sensorPacketNumBytes) {
      WWLog.logError("unexpected number of bytes in raw sensor: " + jsaData.Count);
    }
    if (jsaData.Count == 0) {
      return;
    }

    data = new byte[jsaData.Count];
    for (int n = 0; n < jsaData.Count; ++n) {
      data[n] = (byte)(jsaData[n].AsInt);
    }
  }

  public override string ToString() {
    if (Data == null) {
      return "<null>";
    }
    else {
      return piStringUtil.byteArrayToString(Data);
    }
  }
}
