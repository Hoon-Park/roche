using UnityEngine;
using System.Collections;

public class DestroyTimer : MonoBehaviour {
	public float aliveTime = 5.0f;
	private float totalTimer = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		totalTimer += Time.deltaTime;
		if (totalTimer > aliveTime) 
		{
 			GameObject.Destroy(this.gameObject);
		}
	}
}
