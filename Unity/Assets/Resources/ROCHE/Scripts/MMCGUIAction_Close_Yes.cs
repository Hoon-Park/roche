using UnityEngine;
using System.Collections;

public class MMCGUIAction_Close_Yes : MMCGUIAction {

		
	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		textureScript.texture = mTextureCurrent;
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
		Application.Quit();
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
