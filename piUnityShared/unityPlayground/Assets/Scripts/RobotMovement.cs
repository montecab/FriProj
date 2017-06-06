using UnityEngine;
using System.Collections;

public class RobotMovement : MonoBehaviour {
	public GameObject avatarBody;
	public GameObject avatarHead;

	void Update () {
		piBotBo bot = piConnectionManager.Instance.AnyConnectedBo;
		
		if (bot != null) {
			float terrainHeight = getTerrainHeight();
			Vector3    pos = new Vector3(-bot.BodyPoseSensor.y,terrainHeight, bot.BodyPoseSensor.x);
			pos *= 1.0f;
			Quaternion rot = Quaternion.Euler(0, -bot.BodyPoseSensor.radians * Mathf.Rad2Deg,0);
			avatarBody.transform.position = pos;
			avatarBody.transform.localRotation = rot;

			//avoid shaking
			int tilt_degree = (int)(bot.HeadTiltSensor.angle.valTarget*Mathf.Rad2Deg);
			int pan_degree = (int)(-bot.HeadPanSensor.angle.valTarget * Mathf.Rad2Deg);
			rot = Quaternion.Euler(tilt_degree,  pan_degree, 0);
			avatarHead.transform.localRotation = rot;
		}
		
	}

	float getTerrainHeight(){
		RaycastHit hit;
		if (Physics.Raycast(avatarBody.transform.position, -Vector3.up, out hit))
			return  hit.point.y;
		return 0;
	}
}
