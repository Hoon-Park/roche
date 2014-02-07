using UnityEngine;
using System.Collections;

public class MMCGUITexture : MonoBehaviour {
	public float native_res_width = 1280;
	public float native_res_height = 1024;
	
	public Texture2D texture;
	public Vector2 position = Vector2.zero;
	public Vector2 inset = Vector2.zero;
	public Vector2 size = new Vector2(1280,1024);
	
	private Rect r;
	private Matrix4x4 matrix;
	private MMCGUIAction action;
	
	public int isOver = 0;
	public int isKinectOver = 0;
	
	public bool isClicking = false;
	public bool lastClicking = false;
	public bool isKinectClicking = false;
	
	public int depth = 0;
	private KinectManager kinectManager;
	private bool lastHandClose = false;
	private ROCHEScript rScript;
	
	void Awake()
	{
		action = GetComponent<MMCGUIAction>();
	}
	
	void Start () 
	{
		rScript = GameObject.Find ("_ROCHE").GetComponent<ROCHEScript>();
		kinectManager = rScript.kinectManager;
		 
	}
	
	// Update is called once per frame
	void Update () 
	{
		r = new Rect((position.x + inset.x), (position.y + inset.y), size.x, size.y);
		if (action == null) return;
		if (isOver == 1) 
		{
			action.DoMouseOver();
		}
		else if (isOver >= 2 )
		{
			action.DoMouseOut();
			isOver = 0;
		}
		if (isClicking)
		{
			action.DoMouseDown();
			isClicking = false;
		}
		if (kinectManager != null && kinectManager.kinectLaunched)
		{
			if (isKinectOver == 1)
			{
				action.DoMouseOver();
			}
			else if (isKinectOver == -1)
			{
				action.DoMouseOut();
				isKinectOver = 0;
			}
			if (isKinectClicking)
			{
				action.DoMouseDown();
				isKinectClicking = false;
			}
		}
		
	}
	
	void OnGUI()
	{
		GUI.depth = depth;
		
		//set up scaling
	    float rx = Screen.width / native_res_width;
	    float ry = Screen.height / native_res_height;
		
		matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (rx, ry, 1));
	    GUI.matrix = matrix;
		

		if (texture != null)
			GUI.DrawTexture(r, texture);
		
		if ( action != null) 
		{
			Vector2 m = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
			if (r.Contains(m) )
			{
				isOver = 1;
				if (Input.GetMouseButtonDown(0) && lastClicking == false) 
				{
					isClicking = true;
				}
                else if (Input.GetMouseButtonUp(0))
                {
                    action.DoMouseUp();
                }
			}
			else 
			{
				if (isOver >= 1) isOver++;
				else isOver = 0;
			}
			lastClicking = Input.GetMouseButtonDown(0);
			
			if (kinectManager != null && kinectManager.kinectLaunched && (kinectManager.blockKinect == false || GetComponent<MMCGUIAction_Kinect_Block>() != null))
			{
				Vector2 m2 = GUIUtility.ScreenToGUIPoint(new Vector2(kinectManager.cursorPosition.x, kinectManager.cursorPosition.y));
				if (r.Contains(m2))
				{
					isKinectOver = 1;
					if (kinectManager.rightHand.isHandClosed && lastHandClose == false) {
						isKinectClicking = true;
					}
				}
				else 
				{
					if (isKinectOver == 1) isKinectOver = -1;
				}
				lastHandClose = kinectManager.rightHand.isHandClosed;
			}
		}
	}
}
