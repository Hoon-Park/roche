using UnityEngine;
using System.Collections;

public class MMCGUIClient : MonoBehaviour {
	public GUISkin skin;
	public string style = "Regular";
	public float native_res_width = 1280;
	public float native_res_height = 1024;
	
	public Texture2D texture;
	public Vector2 position = Vector2.zero;
	public Vector2 textPosition = Vector2.zero;
	public Vector2 inset = Vector2.zero;
	public Vector2 size = new Vector2(1280,1024);
	public string text = "Lorem Ipsum..";
	private Rect r;
	private Matrix4x4 matrix;
	private MMCGUIAction action;
	
	public int isOver = 0;
	public bool isClicking = false;
	
	public int depth = -1;
	
	void Awake()
	{
		action = GetComponent<MMCGUIAction>();
	}
	
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (action == null) return;
		if (isOver == 1) 
		{
			action.DoMouseOver();
		}
		else if (isOver >= 2)
		{
			action.DoMouseOut();
			isOver = 0;
		}
		if (isClicking)
		{
			action.DoMouseDown();
			isClicking = false;
		}
	}
	
	void OnGUI()
	{
		GUI.depth = depth;
		GUI.skin = skin;
		
		//set up scaling
	    float rx = Screen.width / native_res_width;
	    float ry = Screen.height / native_res_height;
		
		matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (rx, ry, 1));
	    GUI.matrix = matrix;
		
		if (texture != null)
		{
			r = new Rect((position.x + inset.x), (position.y + inset.y), size.x, size.y);
			GUI.DrawTexture(r, texture);
		}
		else 
		{
			Rect rect = new Rect(textPosition.x , textPosition.y, size.x, size.y);
			GUI.Label(rect,text,style);
		}
		
		if ( action != null) 
		{
			Vector2 m = GUIUtility.ScreenToGUIPoint(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
			if (r.Contains(m))
			{
				isOver = 1;
				if (Input.GetMouseButtonDown(0)) 
					isClicking = true;
			}
			else 
			{
				if (isOver >= 1) isOver++;
				else isOver = 0;
			}
			
		}
		
	}
}
