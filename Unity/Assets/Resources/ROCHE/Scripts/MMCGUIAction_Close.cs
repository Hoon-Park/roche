using UnityEngine;
using System.Collections;

public class MMCGUIAction_Close : MMCGUIAction {
	public GameObject closeDialog;

	// Use this for initialization
	void Start () {
		mTextureCurrent = textureOff;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		
		textureScript.texture = mTextureCurrent;
	}
	
	public override void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public override void DoMouseDown()
	{
		closeDialog.SetActive(true);
	}
	
	public override void DoMouseOut()
	{
		mTextureCurrent = textureOff;
	}
}
