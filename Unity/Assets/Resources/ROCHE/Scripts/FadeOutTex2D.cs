using UnityEngine;
using System.Collections;

public class FadeOutTex2D : MonoBehaviour {
	public float aliveTime = 3.0f;
	
	private float alpha =1.0f;
	private GUITexture myGUITexture;
	private float totalTimer = 0;
	
	// Use this for initialization
	void Start () {
		myGUITexture = this.gameObject.GetComponent("GUITexture") as GUITexture;
	}
	
	// Update is called once per frame
	void Update () {
		totalTimer += Time.deltaTime;
		if (totalTimer > aliveTime) 
		{
			alpha = Mathf.Lerp(alpha,0,Time.deltaTime*10.0f);
		}
		Color tempColor = myGUITexture.color;
		tempColor.a = alpha;
		myGUITexture.color = tempColor;
	}
}
