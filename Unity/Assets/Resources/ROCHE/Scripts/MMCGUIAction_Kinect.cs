using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class MMCGUIAction_Kinect : MMCGUIAction {
	

	
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
		GameObject.Find ("_ROCHE").GetComponent<ROCHEScript>().ActivateKinect(true);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
