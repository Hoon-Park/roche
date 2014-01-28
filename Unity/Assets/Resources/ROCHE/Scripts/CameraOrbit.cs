using UnityEngine;
using System.Collections;

public class CameraOrbit : CameraRoche {
	public bool isActive = false;

    // Zoom
    public float speedZoom = 25.0f;
    public float minPinchSpeed = 1.0F;
    public float varianceInDistances = 1.0F;
    public float zoomInput = 0.0f;
    public float distance = 20.0f;
    public float distanceMin = 4.0f;
    public float distanceMax = 60.0f;
    public float wheelSpeed = 10000.0f;

    // Panning
    public float spanSpeed = 0.5f;
    //public float spanSpeed = 20.0f;
    public Vector3 target = Vector3.zero;

    // Rotation
    public float xSpeed = 500.0f;
	public float ySpeed = 500.0f;

    public float yMinLimit = 1.0f;
    public float yMaxLimit = 80f;

	public float smoothTime = 0.1f;
 
	private float xSmooth = 0.0f;
	private float ySmooth = 45.0f; 

	private float distSmooth = 0.0f;
	private float spanXSmooth = 0.0f;
	private float spanYSmooth = 0.0f;
	private float xVelocity = 0.0f;
	private float yVelocity = 45.0f;
	private float distVelocity = 80.0f;
	private float spanXVelocity = 0.0f;
	private float spanYVelocity = 0.0f;
	 
    public float x = 0.0f;
    public float y = 45.0f;

    public float spanX = 0;
    public float spanZ = 0;

    public float xRotationSpeed = 80.0f;
    public float yRotationSpeed = 80.0f;

	private Bounds buildingBounds;

    public float zoomKinect = 0.0f;

	private KinectManager kinectManager;

    private ROCHEScript.BuildType buildType;
	private Vector3 lastMousePos = Vector3.zero;

	void Start () {
        buildType = ROCHEScript.instance.buildType;

        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
		
		buildingBounds = new Bounds(Vector3.zero,Vector3.one);
		GameObject edificio = GameObject.Find("Edificio");
		if (edificio != null)
		{
			buildingBounds = new Bounds (edificio.transform.position, Vector3.zero);
		    Renderer[] renderers = edificio.GetComponentsInChildren<Renderer> ();
		    foreach (Renderer r in renderers)
		    {
		        buildingBounds.Encapsulate (r.bounds);
		    }
		}
		target = buildingBounds.center;
		kinectManager = ROCHEScript.instance.kinectManager;
	}

    void GetInputDesktop()
    {
        spanX += Input.GetAxis("Horizontal") * spanSpeed * Time.deltaTime;
        spanZ += Input.GetAxis("Vertical") * spanSpeed * Time.deltaTime;
        if (Input.GetMouseButton(1))
        {
            // Camera rotation around target
            x += Input.GetAxis("Mouse X") * xSpeed * Time.deltaTime;
            y -= Input.GetAxis("Mouse Y") * ySpeed * Time.deltaTime;
        }
        zoomInput = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.PageUp)) zoomInput += Time.deltaTime * 2.5f;
        else if (Input.GetKey(KeyCode.PageDown)) zoomInput -= Time.deltaTime * 2.5f;
    }

	void GetKinectSpan()
	{
		Vector3 diff = kinectManager.cursorPosition - kinectManager.prevCursorPosition;
		spanZ -= diff.y / 50;
		spanX += diff.x / 50;
	}

	void GetKinectRotate()
	{
		if (kinectManager.cursorPosition != Vector2.zero)
		{
			if (kinectManager.isRight)
			{
				x += 25 * Time.deltaTime;
			}
			else if (kinectManager.isLeft)
			{
				x -= 25 * Time.deltaTime;
			}
			if (kinectManager.isUp)
			{
				y += 25 * Time.deltaTime;
			}
			else if (kinectManager.isDown)
			{
				y -= 25 * Time.deltaTime;
			}
		}
	}

    void GetInputKinect()
    {
        if (kinectManager == null || kinectManager.kinectLaunched == false)
        {
            GetInputDesktop();
            return;
        }

		if (kinectManager.blockKinect == true) return;
		zoomInput += zoomKinect;
		if (zoomKinect != 0) return;
		if (kinectManager.rightHand.isHandClosed) GetKinectSpan();
		else GetKinectRotate();
    }

    void GetInputMonotouch()
    {
		if (Input.GetMouseButtonDown(0))
		{
			lastMousePos = Input.mousePosition;
		}
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonUp(0))
        {
			Vector3 dif = Input.mousePosition - lastMousePos;
			dif.Normalize();
			Debug.Log (dif);
            spanZ += dif.y * spanSpeed * distance * Time.deltaTime;
            spanX += dif.x * spanSpeed * distance * Time.deltaTime;
			lastMousePos = Input.mousePosition;
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPos = Input.GetTouch(0).deltaPosition;
            spanX -= touchDeltaPos.x * spanSpeed * distance * Time.deltaTime;
            spanZ -= touchDeltaPos.y * spanSpeed * distance * Time.deltaTime;
        }
    }

    void GetInputTablet()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Vector2 touchDeltaPos = Input.GetTouch(0).deltaPosition;
            spanX -= touchDeltaPos.x * spanSpeed * distance * Time.deltaTime;
            spanZ -= touchDeltaPos.y * spanSpeed * distance * Time.deltaTime;
        }
        else if (Input.touchCount >= 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);
            Vector2 curDist = t1.position - t2.position;
            Vector2 t1Delta = t1.position - t1.deltaPosition;
            Vector2 t2Delta = t2.position - t2.deltaPosition;
            Vector2 prevDist = t1Delta - t2Delta;
            float touchDelta = curDist.magnitude - prevDist.magnitude;

            //			if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Stationary)
            //			{
            //				// Rotation
            //				Touch tUpper = t1;
            //				int sign = 1;
            ////				if (t2.position.y > t1.position.y) sign = -1;
            //            	x += t1.deltaPosition.x * xRotationSpeed * sign * Time.deltaTime;
            //			}
            //			else if (t2.phase == TouchPhase.Moved && t1.phase == TouchPhase.Stationary)
            //			{
            //				// Rotation
            //				Touch tUpper = t2;
            //				int sign = 1;
            ////				if (t1.position.y > t2.position.y) sign = -1;
            //            	x += t2.deltaPosition.x * xRotationSpeed * sign * Time.deltaTime;
            //			}
            if (t1.phase == TouchPhase.Moved && t2.phase == TouchPhase.Moved)
            {
                // Rotation
                //				Touch tUpper = t1;
                //				Vector3 d = t1.deltaPosition;
                //				if (t2.position.y > t1.position.y) d = t2.deltaPosition;
                //            	x += d.x * xRotationSpeed * Time.deltaTime;
                //x += touchDeltaPos2.x * xRotationSpeed * Time.deltaTime;
                //y += touchDeltaPos1.y * yRotationSpeed * Time.deltaTime;
                //y -= touchDeltaPos2.y * yRotationSpeed * Time.deltaTime;

                // Zoom


                float speedTouch0 = t1.deltaPosition.magnitude / t1.deltaTime;
                float speedTouch1 = t2.deltaPosition.magnitude / t2.deltaTime;


                if ((touchDelta + varianceInDistances <= 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                {
                    zoomInput = -speedZoom;
                    //Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView + (1 * speedZoom),15,90);
                }

                if ((touchDelta + varianceInDistances > 1) && (speedTouch0 > minPinchSpeed) && (speedTouch1 > minPinchSpeed))
                {
                    zoomInput = speedZoom;
                    //Camera.main.fieldOfView = Mathf.Clamp(Camera.main.fieldOfView - (1 * speedZoom),15,90);
                }
            }
        }
    }

    void Update () {
		if (isActive == false) return;
        if (ROCHEScript.instance.guiOpened != null) return;
		
        // Restart variables
		spanX = 0;
		spanZ = 0;

        // Input type
        switch (buildType)
        {
            case ROCHEScript.BuildType.Desktop:
                GetInputDesktop();
                break;
            case ROCHEScript.BuildType.Kinect:
                GetInputKinect();
                break;
            case ROCHEScript.BuildType.Monotouch:
                GetInputMonotouch();
                break;
            case ROCHEScript.BuildType.Tablet:
                GetInputTablet();
                break;
        }

        spanXSmooth = Mathf.SmoothDamp(spanXSmooth, spanX, ref spanXVelocity, smoothTime / 2);
        spanYSmooth = Mathf.SmoothDamp(spanYSmooth, spanZ, ref spanYVelocity, smoothTime / 2);
		
 		target += spanXSmooth * transform.right;
		target += spanYSmooth * Vector3.Cross(transform.right ,new Vector3(0,1,0));
		target.y = 0;
		
		if (target.x < buildingBounds.min.x) target.x = buildingBounds.min.x;
		else if (target.x > buildingBounds.max.x) target.x = buildingBounds.max.x;
			
		if (target.z < buildingBounds.min.z) target.z = buildingBounds.min.z;
		else if (target.z > buildingBounds.max.z) target.z = buildingBounds.max.z;
		
		xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);
		ySmooth = Mathf.SmoothDamp(ySmooth, y, ref yVelocity, smoothTime);
			
		if (ySmooth < yMinLimit) y = yMinLimit;
		else if (ySmooth > yMaxLimit) y = yMaxLimit;
        ySmooth = ClampAngle(ySmooth, yMinLimit, yMaxLimit);
	        
 		Quaternion rotation = Quaternion.Euler(ySmooth, xSmooth, 0);
		// Distance to target
        distance = Mathf.Clamp(distance - zoomInput*wheelSpeed*Time.deltaTime, distanceMin, distanceMax);
        distSmooth = Mathf.SmoothDamp(distSmooth, distance, ref distVelocity, smoothTime);
		Vector3 negDistance = new Vector3(0.0f, 0.0f, -distSmooth);
		
		// Final position
        Vector3 position = rotation * negDistance + target;
		
		// Set new rotation and position
        transform.rotation = rotation;
        transform.position = position;

        zoomInput = 0;
        zoomKinect = 0;
	}
 
    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
	
	public void Activate()
	{
		if (isActive) return;
		
		// Set default angle
		isActive = true;
		
		// Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
		
	}
	
	public void Disactivate()
	{
		if (!isActive) return;
		
		isActive = false;
		if (rigidbody)
        	rigidbody.freezeRotation = false;
	}

    public override void ZoomOut()
    {
        zoomInput -= speedZoom/2000;
    }

    public override void ZoomIn()
    {
        zoomInput += speedZoom/2000;
    }

    public override void Left()
    {
        x -= xSpeed / 6 * Time.deltaTime;
    }

    public override void Right()
    {
        x += xSpeed / 6 * Time.deltaTime;
    }

    public override void Up()
    {
        y += ySpeed / 8 * Time.deltaTime;
    }

    public override void Down()
    {
        y -= ySpeed / 8 * Time.deltaTime;
    }

}
