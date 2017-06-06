using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class _PhotonNetworkManager : MonoBehaviour {

	const string VERSION = "v1";
	public string roomName = "Apple";
	public string prefabName = "ViveModel";

	void Start () {
		PhotonNetwork.ConnectUsingSettings (VERSION);
	}

	void OnConnectedToMaster() {
		RoomOptions roomOptions = new RoomOptions();
		roomOptions.IsVisible = false;
		roomOptions.MaxPlayers = 2;
		PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);

		Debug.Log ("Master Joined Successfully");
	}

	void OnJoinedLobby() {
		Debug.Log ("Lobby Joined Successfully");
	}

	void OnJoinedRoom() {
		PhotonNetwork.Instantiate (prefabName, Vector3.zero, Quaternion.identity, 0);

		Debug.Log ("Room Joined Successfully");
	}
}
