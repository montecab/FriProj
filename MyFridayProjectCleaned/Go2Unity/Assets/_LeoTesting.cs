using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using WW;
using PI;
using Turing;

public class _LeoTesting : MonoBehaviour {

	public Button addButton;
	public Button clearButton;
	public Button startButton;
	public VerticalLayoutGroup listVerticalLayoutGroup;
	public GameObject pointPrefab;

	private List<GameObject> instantiatedPoints = new List<GameObject>();

	void Start () {

		addButton.onClick.AddListener (delegate {
			GameObject newPoint = Instantiate(pointPrefab) as GameObject;
			addGameObjectToTransform(newPoint, listVerticalLayoutGroup.transform);
			instantiatedPoints.Add(newPoint);
		});

		clearButton.onClick.AddListener (delegate {
			foreach(GameObject obj in instantiatedPoints) {
				Destroy(obj);
			}
			instantiatedPoints.Clear();
		});

		startButton.onClick.AddListener (delegate {
			StopAllCoroutines();
			StartCoroutine(Run());
		});
	}

	Vector2 currentVector = Vector2.right;
	Vector2 nextVector = Vector2.zero;

	List<Command> CalculateCommands() {
		
		List<Command> commands = new List<Command>();

		for (int x = 0; x < instantiatedPoints.Count-1; x++) {
			Vector2 point1 = instantiatedPoints [x].GetComponent<_LeoPoint>().vector;
			Vector2 point2 = instantiatedPoints [x + 1].GetComponent<_LeoPoint>().vector;

			nextVector = point2 - point1;
			// Get Angle
			float deltaAngle = Vector2.Angle(currentVector, nextVector);
			// Get Distance
			float deltaDistance = Vector2.Distance(point1, point2);
			commands.Add (new Command(deltaAngle, Command.CommandType.Turn));
			commands.Add (new Command(deltaDistance, Command.CommandType.Move));
			currentVector = nextVector;

			Debug.Log ("angle: " + deltaAngle + ", distance: " + deltaDistance);
		}

		return commands;
	}

	IEnumerator Run() {
		List<Command> commands = CalculateCommands ();

		foreach (Command command in commands) {
			bool done = false;
			command.onFinish = () => {
				done = true;
			};
			StartCoroutine (command.RunCommand());
			while (!done) {
				yield return null;
			}
			yield return new WaitForSeconds (1f);
		}	
	}
		
	void addGameObjectToTransform(GameObject obj, Transform parentTransform){
		obj.transform.SetParent(parentTransform);
		obj.GetComponent<RectTransform>().SetDefaultScale();
		RectTransform rectTransform = obj.GetComponent<RectTransform>();
		Vector3 position = rectTransform.localPosition;
		position.z = 0;
		rectTransform.localPosition = position;
	}

	public class Command {

		public enum CommandType {Move, Turn}
		public delegate void OnFinishHandler ();
		public OnFinishHandler onFinish;

		public float xFactor;
		public CommandType type;

		private const float TIMEPERUNIT = .5f;
		private const float SECONDSPERDEGREE = 0.0225f;
		//private const float SECONDSPERDEGREE = 0.0195f;

		public Command(float x, CommandType t) {
			xFactor = x;
			type = t;
		}

		public IEnumerator RunCommand() {
			
			piBotBo dash = piConnectionManager.Instance.AnyConnectedBo;
			float counter = 0;

			switch (type) {
			case CommandType.Move:
				float totalMoveTime = xFactor * TIMEPERUNIT;
				dash.cmd_bodyMotion (50, 0);
				while (counter < totalMoveTime) {
					counter += Time.deltaTime;
					yield return null;
				}
				break;
			case CommandType.Turn:
				float totalTurnTime = xFactor * SECONDSPERDEGREE;
				//float totalTurnTime = (xFactor + 360) * SECONDSPERDEGREE;
				dash.cmd_bodyMotion (0, 1);
				while (counter < totalTurnTime) {
					counter += Time.deltaTime;
					yield return null;
				}
				break;
			}
				
			dash.cmd_bodyMotionStop ();
			onFinish.Invoke ();
		}
	}
}
