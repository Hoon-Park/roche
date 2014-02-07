using UnityEngine;
using System.Collections;

public class MMCGUIAction_HelpScreenClose : MMCGUIAction {

	
	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		
		textureScript.texture = mTextureCurrent;
		
		if (Input.GetKeyDown(KeyCode.Escape))
			DoMouseDown();
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
		transform.parent.gameObject.SetActive(false);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
