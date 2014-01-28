using UnityEngine;
using System.Collections;

public class MMCGUIAction_Kinect_Orbit : MMCGUIAction {

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
		guiZoom.SetActive(true);
        CameraRocheSystem.instance.GetComponent<CameraRocheSystem>().ActivateCamera(CameraRocheSystem.CAMERA_TYPE.CAMERA_ORBIT, false);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
