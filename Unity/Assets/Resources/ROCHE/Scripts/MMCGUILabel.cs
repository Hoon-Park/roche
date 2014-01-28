using UnityEngine;
using System.Collections;

public class MMCGUILabel : MonoBehaviour {
	public GUISkin skin;
	public string style = "Regular";
	
	public float native_res_width = 1280;
	public float native_res_height = 1024;
	
	public string text = "Lorem Ipsum..";
	public Vector2 position = Vector2.zero;
	public Vector2 size = new Vector2(1280,1024);
	
	public int depth = -1;
	
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI()
	{
		GUI.skin = skin;
		GUI.depth = depth;
		
		//set up scaling
	    float rx = Screen.width / native_res_width;
	    float ry = Screen.height / native_res_height;
	    GUI.matrix = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (rx, ry, 1)); 
		
	    //example
		Rect rect = new Rect(position.x, position.y, size.x, size.y);
	
		GUI.Label(rect,text,style);
	}
	
}
