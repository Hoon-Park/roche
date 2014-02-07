using UnityEngine;
using System.Collections;

public class MMCGUIAction_HelpScreen : MMCGUIAction {
	public Texture2D texture1;
	public Texture2D texture2;
	
	public Texture2D textureScreen1, textureScreen2;
	
	private Texture2D mTextureCurrentScreen;
	
	public GameObject helpscreen; 
	MMCGUITexture textureScriptHelpScreen;
	
	public MMCGUILabel detallesProjectoTitle;
	public MMCGUILabel detallesProjecto;

	// Use this for initialization
	void Start () {
		mTextureCurrent = texture1;
		textureScript = GetComponent<MMCGUITexture>();
		textureScript.texture = mTextureCurrent;
		textureScriptHelpScreen = helpscreen.GetComponent<MMCGUITexture>();
		mTextureCurrentScreen = textureScreen1;
		textureScriptHelpScreen.texture = mTextureCurrentScreen;
		
		detallesProjectoTitle.enabled = false;
		detallesProjecto.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		textureScript.texture = mTextureCurrent;
		textureScriptHelpScreen.texture = mTextureCurrentScreen;
	}
	
	public override void DoMouseOver()
	{
	}
	
	public override void DoMouseDown()
	{
		if(mTextureCurrent == texture1) 
		{
			SetTab(2);
		}
		else if (mTextureCurrent == texture2)
		{
			SetTab (1);
		}
	}
	
	public override void DoMouseOut()
	{
	}
	
	public void SetTab(int i) 
	{
		if (i == 1)
		{
			mTextureCurrent = texture1;
			mTextureCurrentScreen = textureScreen1;
			detallesProjecto.enabled = false;
			detallesProjectoTitle.enabled = false;
		}
		else if (i == 2)
		{
			mTextureCurrent = texture2;
			mTextureCurrentScreen = textureScreen2;
			detallesProjecto.enabled = true;
			detallesProjectoTitle.enabled = true;
		}
	}
}
