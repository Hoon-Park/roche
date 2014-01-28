using UnityEngine;
using System.Collections;

public class MMCGUIAction_WalkOrbit : MMCGUIAction {
	public Texture2D textureWalk;
	public Texture2D textureOrbit;
	public GameObject RocheCamera;
	
	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOrbit;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
            if (CameraRocheSystem.instance.selectedCamera == CameraRocheSystem.CAMERA_TYPE.CAMERA_ORBIT) return;
			else 
			{
                CameraRocheSystem.instance.SwitchCamera();
				mTextureCurrent = textureOrbit;
			}
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
            if (CameraRocheSystem.instance.selectedCamera == CameraRocheSystem.CAMERA_TYPE.CAMERA_FPS) return;
			else 
			{
                CameraRocheSystem.instance.SwitchCamera();
				mTextureCurrent = textureWalk;
			}
		}
		textureScript.texture = mTextureCurrent;
	}
	
	public override void DoMouseOver()
	{
	}
	
	public override void DoMouseDown()
	{
        if (CameraRocheSystem.instance.selectedCamera == CameraRocheSystem.CAMERA_TYPE.CAMERA_ORBIT)
		{
            CameraRocheSystem.instance.SwitchCamera();
			mTextureCurrent = textureWalk;
		}
		else
		{
            CameraRocheSystem.instance.SwitchCamera();
			mTextureCurrent = textureOrbit;
		}
		
		textureScript.texture = mTextureCurrent;
	}
	
	public override void DoMouseOut()
	{		
	}
}
