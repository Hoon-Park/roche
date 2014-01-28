using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {

	public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
	RotationAxes axes = RotationAxes.MouseXAndY;
	public float sensitivityX = 3;
	public float sensitivityY = 3;
	
	#pragma warning disable 0414
	float minimumX = -360F;
	float maximumX = 360F;

	float minimumY = -90.0f;
	float maximumY = 90.0f;
	
	public float rotationX = 0.0f;
	public float rotationY = 0.0f;
	
	public float velX = 0.0f;
	public float velY = 0.0f;
	private float lastVelX = 0.0f;
	private float lastVelY = 0.0f;
	#pragma warning restore 0219
	
	public float inputY = 0;
    public float inputX = 0;
	
	private KinectManager kinectManager;
	private Vector3 lastMousePos = Vector3.zero;
	
	void Start ()
	{
		// Make the rigid body not change rotation
		if (rigidbody)
			rigidbody.freezeRotation = true;
		
		rotationX = transform.localEulerAngles.y; 
		rotationY = transform.localEulerAngles.x;
		kinectManager = KinectManager.instance;
	}
	
	void Update ()
	{
		if (axes == RotationAxes.MouseXAndY)
        {
            inputX = 0; inputY = 0;

            switch (ROCHEScript.instance.buildType)
            {
                case ROCHEScript.BuildType.Desktop:
                    inputY = Input.GetAxis("Mouse Y");
                    inputX = Input.GetAxis("Mouse X");
                    break;
                case ROCHEScript.BuildType.Kinect:
                    if (kinectManager == null || kinectManager.kinectLaunched == false)
                    {
                        inputY = Input.GetAxis("Mouse Y");
                        inputX = Input.GetAxis("Mouse X");
                    }
                    else if (kinectManager.rightHand.isHandClosed && kinectManager.blockKinect == false)
                    {
                        Vector3 diff = kinectManager.cursorPosition - kinectManager.prevCursorPosition;
                        inputY -= diff.y / 10;
                        inputX += diff.x / 10;
                    }
                    break;
                case ROCHEScript.BuildType.Monotouch:
					if (Input.GetMouseButtonDown(0))
					{
						lastMousePos = Input.mousePosition;
					}
                    if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
                    {
						Vector3 dif = Input.mousePosition - lastMousePos;
						dif.Normalize();
                        inputY = dif.y;
                        inputX = dif.x;
						lastMousePos = Input.mousePosition;
                    }
                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        Vector2 touchDeltaPos = Input.GetTouch(0).deltaPosition;
                        inputX -= touchDeltaPos.x * 12 * Time.deltaTime;
                        inputY -= touchDeltaPos.y * 12 * Time.deltaTime;
                    }
                    break;
                case ROCHEScript.BuildType.Tablet:
                    if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
                    {
                        Vector2 touchDeltaPos = Input.GetTouch(0).deltaPosition;
                        inputX -= touchDeltaPos.x * 12 * Time.deltaTime;
                        inputY -= touchDeltaPos.y * 12 * Time.deltaTime;
                    }
                    break;

            }

            if (Input.GetKey(KeyCode.Z)) inputX -= 1;
            else if (Input.GetKey(KeyCode.X)) inputX += 1;

            float diffX = Mathf.Abs(inputX - velX);
            if (velX < inputX) velX += diffX * sensitivityX * Time.deltaTime;
            else if (velX > inputX) velX -= diffX * sensitivityX * Time.deltaTime;
            else velX = 0;

            rotationX = transform.localEulerAngles.y + velX * sensitivityX;
            if (rotationX < 0) rotationX += 360;
            if (rotationX > 0) rotationX %= 360;

            float diffY = Mathf.Abs(inputY - velY);
            if (velY < inputY) velY += diffY * sensitivityY * Time.deltaTime;
            else if (velY > inputY) velY -= diffY * sensitivityY * Time.deltaTime;
            else velY = 0;

            rotationY = transform.localEulerAngles.x - velY * sensitivityY;
            if (rotationY < 0) rotationY += 360;
            if (rotationY > 0) rotationY %= 360;

            if (rotationY > 45 && rotationY < 90) rotationY = 45;
            if (rotationY < 320 && rotationY > 120) rotationY = 320;

            transform.localEulerAngles = new Vector3(rotationY, rotationX, 0);

            lastVelX = velX;
            lastVelY = velY;
        }
		else if (axes == RotationAxes.MouseX)
		{
			transform.Rotate(0, Input.GetAxis("Mouse X") * sensitivityX, 0);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
			
			transform.localEulerAngles = new Vector3(-rotationY, transform.localEulerAngles.y, 0);
		}
	}
	
}
