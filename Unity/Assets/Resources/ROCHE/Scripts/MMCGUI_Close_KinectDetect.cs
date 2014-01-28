using UnityEngine;
using System.Collections;

public class MMCGUI_Close_KinectDetect : MonoBehaviour {
	public GameObject normalGO;
	public GameObject kinectGO;
	
	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
				if (KinectManager.instance.kinectLaunched)
		{
			normalGO.SetActive(false);
			kinectGO.SetActive(true);
		}
		else	
		{
			normalGO.SetActive(true);
			kinectGO.SetActive(false);
		}
	}
}
