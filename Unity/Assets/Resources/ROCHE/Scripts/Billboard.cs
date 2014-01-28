using UnityEngine;
using System.Collections;

public class Billboard : MonoBehaviour {
	Transform camTransform;
	Vector3 originalSize;
	
	private Vector3 walkSize;
	
	// Use this for initialization
	void Start () {
		originalSize = transform.localScale;
		camTransform = Camera.main.transform;
		walkSize = new Vector3(0.65f,0.65f,0.65f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.LookAt(camTransform.position,Vector3.up);
		transform.Rotate(0,-180,0);
		float dist = Vector3.Distance(camTransform.position,transform.position);

        if (CameraRocheSystem.instance.selectedCamera == CameraRocheSystem.CAMERA_TYPE.CAMERA_FPS)
		{
			transform.localScale = walkSize;
		}
		else transform.localScale = originalSize * dist/7.5f;
	}
}
