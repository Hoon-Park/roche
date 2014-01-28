using UnityEngine;
using System.Collections;

public class MMCGUIAction_Help : MMCGUIAction {

	public GameObject helpScreen;
	
	
	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		
		textureScript.texture = mTextureCurrent;
		
		if (Input.GetKeyDown(KeyCode.A)) DoMouseDown();			
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
		mTextureCurrent = textureOn;
		// Launch help
		helpScreen.SetActive(true);
		helpScreen.GetComponentInChildren<MMCGUIAction_HelpScreen>().SetTab(1);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
