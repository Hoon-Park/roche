using UnityEngine;
using System.Collections;

public class MMCGUIAction : MonoBehaviour {
	public Texture2D textureOn;
	public Texture2D textureOff;
	protected Texture2D mTextureCurrent;
	protected MMCGUITexture textureScript;
    protected bool buttonPressed = false;
	
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
	
	public virtual void DoMouseOver()
	{
		mTextureCurrent = textureOn;
	}
	
	public virtual void DoMouseDown()
	{
        buttonPressed = true;
	}
	
	public virtual void DoMouseOut()
	{		
		mTextureCurrent = textureOff;
	}

    public virtual void DoMouseUp()
    {
        buttonPressed = false;
    }
}
