using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class MMCGUIAction_Orbit_ZoomMinus : MMCGUIAction {
	bool touched = false;
	
	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}

	// Update is called once per frame
	void Update () {
		textureScript.texture = mTextureCurrent;

        if (buttonPressed) CameraRocheSystem.instance.currentCamera.ZoomOut();
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
		if (touched == false) return;
		if (KinectManager.instance.kinectLaunched && KinectManager.instance.rightHand.isHandClosed)
            CameraRocheSystem.instance.currentCamera.ZoomOut();
	}
	
	public override void DoMouseDown()
	{
        buttonPressed = true;
		touched = true;
	}
	
	public override void DoMouseOut()
	{
		touched = false;
		mTextureCurrent = textureOff;
	}

    public override void DoMouseUp()
    {
        buttonPressed = false;
    }
}
