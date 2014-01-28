using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class MMCGUIAction_Kinect_Block : MMCGUIAction {
	bool isBlocked = false;
	public GameObject guiCommon;
	
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
		if (isBlocked == false) mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
//		isBlocked = !isBlocked;
		GameObject.Find ("_ROCHE").GetComponent<ROCHEScript>().ToggleKinectWindow();
		
//		if (isBlocked)
//		{
//			mTextureCurrent = textureOn;
//			guiCommon.SetActive(false);
//		}
//		else 
//		{
//			guiCommon.SetActive(true);
//			mTextureCurrent = textureOff;
//			
//		}
	}
	
	public override void DoMouseOut()
	{
		if (isBlocked == false) mTextureCurrent = textureOff;
	}
}
