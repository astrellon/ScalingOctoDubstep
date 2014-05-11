using UnityEngine;
using System.Collections;

public class SpaceScene : MonoBehaviour {

	// Use this for initialization
	void Start () {
		GameObject prefab = Resources.LoadAssetAtPath<GameObject>("Assets/Meshes/Ship1/hull.obj");
		GameObject obj = GameObject.Instantiate (prefab) as GameObject;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
