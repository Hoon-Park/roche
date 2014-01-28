using UnityEngine;
using System.Collections;

public class MMCGUIAction_Kinect_Walk : MMCGUIAction {
	public GameObject guiZoom;
	
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
		guiZoom.SetActive(false);
        CameraRocheSystem.instance.GetComponent<CameraRocheSystem>().ActivateCamera(CameraRocheSystem.CAMERA_TYPE.CAMERA_FPS, false);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
