using UnityEngine;
using System.Collections;

public class LaunchAnimation : MonoBehaviour {
	public Texture2D[] frames;
	public float FPS = 15.0f;
	private MMCGUITexture myGUITexture;
	private int currentFrame = 0;
	float timer = 0;
//	float alpha = 255.0f;
	
	// Use this for initialization
	void Start () {
		myGUITexture = gameObject.GetComponent<MMCGUITexture>();
	}
	
	// Update is called once per frame
	void Update () {
		myGUITexture.texture = frames[currentFrame%12];
		timer += Time.deltaTime;
		if (timer > 1.0f/FPS) 
		{
			currentFrame++;
			timer = 0;
		}

	}
}
