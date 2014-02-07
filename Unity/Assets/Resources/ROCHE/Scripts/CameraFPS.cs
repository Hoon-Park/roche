using UnityEngine;
using System.Collections;

public class CameraFPS : CameraRoche {
	public CharacterController controller;
	public MouseLook mLook;
	public CharacterMotor cMotor;
	public FPSInputController fpsInputController;
	
	public Vector3 spawnPoint = Vector3.zero;
    public Quaternion spawnRotation = Quaternion.identity;
	private KinectManager kinectManager;

    private enum DIRECTION
    {
        NONE = 0,
        LEFT = 1,
        RIGHT = 2,
        FORWARD = 3,
        BACK = 4
    };

    private DIRECTION myDirection;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		mLook = GetComponent<MouseLook>();
		cMotor = GetComponent<CharacterMotor>();
		fpsInputController = GetComponent<FPSInputController>();
	    spawnPoint = transform.position;
	    spawnRotation = transform.rotation;
        myDirection = DIRECTION.NONE;
	}

    private ROCHEScript.BuildType buildType;

    void Start()
    {
        buildType = ROCHEScript.instance.buildType;
		kinectManager = ROCHEScript.instance.kinectManager;
	}
	
	void Update () 
	{
		if (ROCHEScript.instance.guiOpened != null) return;

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
		case ROCHEScript.BuildType.OVR:
			GetInputDesktop();
			break;
        }

	}

    void GetInputDesktop()
    {
        fpsInputController.directionVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    }

    void GetInputKinect()
    {
        if (kinectManager == null || kinectManager.kinectLaunched == false)
        {
            GetInputDesktop();
            return;
        }
        if (kinectManager.rightHand.isHandClosed == true) return;
        if (kinectManager.blockKinect == true) return;
        if (kinectManager.cursorPosition != Vector2.zero)
        {
            if (kinectManager.isRight)
            {
                fpsInputController.directionVector.x += 1000 * Time.deltaTime;
            }
            else if (kinectManager.isLeft)
            {
     			fpsInputController.directionVector.x -= 1000 * Time.deltaTime;
            }
			else fpsInputController.directionVector.x = 0;

            if (kinectManager.isDown)
            {
                fpsInputController.directionVector.z += 1000 * Time.deltaTime;
            }
            else if (kinectManager.isUp)
            {
                fpsInputController.directionVector.z -= 1000 * Time.deltaTime;
            }
			else fpsInputController.directionVector.z = 0;
        }
		else 
		{
			fpsInputController.directionVector.x = 0;
			fpsInputController.directionVector.z = 0;
		}
    }

    void GetInputMonotouch()
    {
        switch (myDirection)
        {
            case DIRECTION.NONE:
                fpsInputController.directionVector = Vector3.zero;
                break;
            case DIRECTION.LEFT:
                fpsInputController.directionVector.x -= 1000 * Time.deltaTime;
                break;
            case DIRECTION.RIGHT:
                fpsInputController.directionVector.x += 1000 * Time.deltaTime;
                break;
            case DIRECTION.FORWARD:
                fpsInputController.directionVector.z += 1000 * Time.deltaTime;
                break;
            case DIRECTION.BACK:
                fpsInputController.directionVector.z -= 1000 * Time.deltaTime;
                break;
        }

        myDirection = DIRECTION.NONE;
    }

    void GetInputTablet()
    {
        fpsInputController.directionVector.x = 0;
        fpsInputController.directionVector.z = 0;
    }

	public void Activate()
	{
		controller.enabled = true;
		mLook.enabled = true;
		cMotor.enabled = true;
		fpsInputController.enabled = true;
		
		if (rigidbody)
        	rigidbody.freezeRotation = false;
		
		transform.position = spawnPoint;
		transform.rotation = spawnRotation;
	}
	
	public void Disactivate()
	{
	    spawnPoint = transform.position;
        spawnRotation = transform.rotation;

		controller.enabled = false;
		mLook.enabled = false;
		cMotor.enabled = false;
		fpsInputController.enabled = false;	
	}

    public override void Left()
    {
        myDirection = DIRECTION.LEFT;
    }

    public override void Right()
    {
        myDirection = DIRECTION.RIGHT;
    }

    public override void Up()
    {
        myDirection = DIRECTION.FORWARD;
    }

    public override void Down()
    {
        myDirection = DIRECTION.BACK;
    }

    public override void RotateLeft()
    {        
    }

    public override void RotateRight()
    {
    }

    public override void RotateDown()
    {
    }

    public override void RotateUp()
    {
    }
}
