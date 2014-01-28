using UnityEngine;
using System.Collections;

public class MMCGUIAction_OrbitD : MMCGUIAction {

	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		
		textureScript.texture = mTextureCurrent;

        if (buttonPressed) CameraRocheSystem.instance.currentCamera.Down();
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
        mTextureCurrent = textureOn;
        buttonPressed = true;
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
        buttonPressed = false;
	}
}
