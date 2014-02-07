using UnityEngine;
using System.Collections;

public class CameraRocheSystem : MonoSingleton<CameraRocheSystem>
{
	
	public enum CAMERA_TYPE
	{
		CAMERA_ORBIT = 0,
		CAMERA_FPS = 1,
		CAMERA_OVR = 2
	}
	
	public CAMERA_TYPE selectedCamera = CAMERA_TYPE.CAMERA_ORBIT;
    public CameraRoche currentCamera;
	public GameObject OVR_Controller;
	private CameraOrbit camOrbit;
    private CameraFPS camFPS;
	public GameObject DefaultCamera;
	
	public override void Init()
	{
		gameObject.GetComponent<CharacterController>().enabled = true;
		camOrbit = gameObject.GetComponent<CameraOrbit>() as CameraOrbit;
		camFPS = gameObject.GetComponent<CameraFPS>();
		OVR_Controller.SetActive(false);
		DefaultCamera.SetActive(true);
		
		DisableCamera(CAMERA_TYPE.CAMERA_ORBIT);
		DisableCamera(CAMERA_TYPE.CAMERA_FPS);
        currentCamera = camOrbit;
        ActivateCamera(selectedCamera, true);

		if (ROCHEScript.instance.buildType == ROCHEScript.BuildType.OVR) 
		{
			ActivateCamera(CAMERA_TYPE.CAMERA_OVR,true);
		}

	}
	
	void Update () 
	{

	}
	
	public void ActivateCamera (CAMERA_TYPE camType, bool force)
	{
		if (!force && selectedCamera == camType) return; // Already active
		DisableCamera(selectedCamera);
		switch(camType)
		{
		case CAMERA_TYPE.CAMERA_ORBIT:
			selectedCamera = CAMERA_TYPE.CAMERA_ORBIT;
			camOrbit.enabled = true;
			camOrbit.Activate();
            currentCamera = camOrbit;
			break;
		case CAMERA_TYPE.CAMERA_FPS:
			selectedCamera = CAMERA_TYPE.CAMERA_FPS;
			camFPS.enabled = true;
			camFPS.Activate();
            currentCamera = camFPS;
			break;
		case CAMERA_TYPE.CAMERA_OVR:
			DefaultCamera.SetActive(false);
			//camFPS.enabled = true;
			//camFPS.Activate();
			selectedCamera = CAMERA_TYPE.CAMERA_OVR;
			OVR_Controller.SetActive(true);
			//currentCamera = camFPS;
			break;
		}
	}
	
	public void DisableCamera (CAMERA_TYPE camType)
	{
		switch(camType)
		{
		case CAMERA_TYPE.CAMERA_ORBIT:
			camOrbit.Disactivate();
			camOrbit.enabled = false;
			break;
		case CAMERA_TYPE.CAMERA_FPS:
			camFPS.Disactivate();
			camFPS.enabled = false;
			break;
		case CAMERA_TYPE.CAMERA_OVR:
			OVR_Controller.SetActive(false);
			DefaultCamera.SetActive(true);
			//camFPS.Disactivate();
			break;
		}
	}
	
	public void SwitchCamera()
	{
		if (selectedCamera == CAMERA_TYPE.CAMERA_ORBIT)
		{
			ActivateCamera(CAMERA_TYPE.CAMERA_FPS, false);
		}
		else 
		{
			ActivateCamera(CAMERA_TYPE.CAMERA_ORBIT, false);
		}
	}

}
