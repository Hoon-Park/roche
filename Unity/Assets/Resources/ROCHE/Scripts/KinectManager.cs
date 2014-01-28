using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class KinectManager : MonoSingleton<KinectManager> {
	
	[DllImport("user32.dll")]
	static extern bool AllowSetForegroundWindow(int dwProcessId);
	
	[DllImport("user32.dll", SetLastError=true)]
	static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
	
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetForegroundWindow(IntPtr hWnd);
	[DllImport("user32.dll", EntryPoint = "SetWindowText")]
    public static extern bool SetWindowText(System.IntPtr hwnd, System.String lpString);
	
    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    public static extern System.IntPtr FindWindow(System.String className, System.String windowName);
	
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
	
	[DllImport("user32.dll")]
	static extern IntPtr SetFocus(IntPtr hWnd);
	
	[DllImport("user32.dll")]
 	static extern bool SetActiveWindow(IntPtr hWnd);
	
	[DllImport("user32.dll")]
	static extern bool SetWindowPos (IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
	
	// Find window by Caption only. Note you must pass IntPtr.Zero as the first parameter.
	
	[DllImport("user32.dll", EntryPoint="FindWindow", SetLastError = true)]
	static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);
	
	[DllImport("user32.dll", SetLastError = true)]
 	[return: MarshalAs(UnmanagedType.Bool)]
 	static extern bool GetWindowRect(IntPtr hWnd, ref RECT lpRect);
	[StructLayout(LayoutKind.Sequential)]
	
	 private struct RECT
	 {
	     public int Left;
	     public int Top;
	     public int Right;
	     public int Bottom;
	 }
	
	static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
	

	// Hand Management
	
	[System.Serializable]
	public class Hand
	{
		public bool isHandClosed;		
		public bool isHandPushing;		
		public int swipeGesture;
		
		public Hand(bool closed, bool pushing, int gesture)
		{
			isHandClosed = closed;
			isHandPushing = pushing;
			swipeGesture = gesture;
		}

		public void CopyHand(Hand other)
		{
			isHandClosed = other.isHandClosed;
			isHandPushing = other.isHandPushing;
			swipeGesture = other.swipeGesture;
		}

		public void Reset()
		{
			isHandClosed = false;
			isHandPushing = false;
			swipeGesture = 0;
		}
	};

	public Hand leftHand, oldLeftHand;
	public Hand rightHand, oldRightHand;
	public Hand currentHand;
	
	// System
	private Vector2 originalRes = new Vector2(640.0f,480.0f);
	
	public bool kinectLaunched = false;
	private Process procKinect = null; 
	private Thread udpThread;
    private UdpClient udpc;
    private IPEndPoint ep = null;
	private string path = "";
	
	public string currentPacket = "null";
	public string currentHandUsed = "n";
	
	public bool blockKinect = false;
	
	// UI
	private Vector2 screenRatio = Vector2.zero;
	public Texture2D cursorTexture;
	public Texture2D cursorTexture2;
	public Texture2D cursorTexture3;
	
	private Vector2 objCursorPosition = Vector2.zero;
	public Vector2 cursorPosition = Vector2.zero;
	public Vector2 prevCursorPosition = Vector2.zero;

	private float goingLeft = 0;
	private float goingRight = 0;
	private float goingUp = 0;
	private float goingDown = 0;
	public bool isLeft = false;
	public bool isRight = false;
	public bool isUp = false;
	public bool isDown = false;
	
	public bool isKinectHidden = false;

	private float zonesTime = 0.5f;
	public bool rightOutOfBounds = true;
	public bool leftOutOfBounds = true;
	
	// Methods
	
	public override void Init()
	{
		
		// Map hand coords to real screen
		screenRatio.x = Screen.width / originalRes.x;
		screenRatio.y = Screen.height / originalRes.y;
		
		// Init params
		leftHand = new Hand(false, false, 0);
		rightHand = new Hand(false, false, 0);
		currentHand = rightHand;
	}
	
	void Update () {
		oldLeftHand.CopyHand(leftHand);
		oldRightHand.CopyHand(rightHand);
		
		prevCursorPosition = cursorPosition;

		cursorPosition = Vector2.Lerp (cursorPosition, objCursorPosition, 0.25f);
		
		// Hand limits
		Vector3 tempPos = cursorPosition;
		
		if (tempPos.x < 0) 
			tempPos.x = 0;
		else if (tempPos.x > Screen.width) tempPos.x = Screen.width;
		if (tempPos.y < 0) tempPos.y = 0;
		else if (tempPos.y > Screen.height) tempPos.y = Screen.height;
		cursorPosition = tempPos;
		
		// Is hand inside a "direction zone"?
		CalculateDirections();
		
		if (currentHandUsed == "r") currentHand = rightHand;
		else if (currentHandUsed == "l") currentHand = leftHand;
		
	}
	
	public void ToggleKWindow()
	{
		// retrieve Notepad main window handle
	    //IntPtr hWnd = FindWindow("Notepad", "Untitled - Notepad");
		IntPtr hWnd = FindWindow(null, "GTI Kinect Controller");
		
	    if (!hWnd.Equals(IntPtr.Zero))
	    {
			if (isKinectHidden == true)
			{
				IntPtr HWND_TOPMOST = new IntPtr(-1);
//				IntPtr HWND_BOTTOM = new IntPtr(1);
    			const int SWP_NOACTIVATE = 0x0010;
   			 	const int SWP_NOMOVE = 0x0002;

    			SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 320, 240, SWP_NOMOVE | SWP_NOACTIVATE);
				
				isKinectHidden = false;
			}
			else {
//				IntPtr HWND_TOPMOST = new IntPtr(-1);
				IntPtr HWND_BOTTOM = new IntPtr(1);
    			const int SWP_NOACTIVATE = 0x0010;
   			 	const int SWP_NOMOVE = 0x0002;

    			SetWindowPos(hWnd, HWND_BOTTOM, 0, 0, 320, 240, SWP_NOMOVE | SWP_NOACTIVATE);
				isKinectHidden = true;
			}
	    }
	}
	
	public void ReceiveUDP()
	{
		try
    	{   
			//Open the Kinect Process
			procKinect = new Process();
			path = path.Replace("/Assets","");
			UnityEngine.Debug.Log(path);
			
			ProcessStartInfo info = new ProcessStartInfo(path);
			
			procKinect = Process.Start(info);

			Thread.Sleep(1000);
			SetFocusOnROCHE();
			ToggleKWindow();
			
			// Kinect launched. Do upd call.
			kinectLaunched = true;
			udpc = new UdpClient("127.0.0.1", 2055);
			Thread.Sleep(1000);
			
			// while bucle
			while (Thread.CurrentThread.IsAlive)
			{
                //we can send a 'request' string if we want
                string name = "RH";
                byte[] sdata = Encoding.ASCII.GetBytes(name);
                udpc.Send(sdata, sdata.Length);
				
				// Receive data
                byte[] rdata = udpc.Receive(ref ep);
                currentPacket = Encoding.ASCII.GetString(rdata);
				
				// There's no packet, sleep for a bit
				if (currentPacket == null || currentPacket == "null") 
				{
					Thread.Sleep(20);
				}
				else
				{
					string[] coords = currentPacket.Split('-');
					currentHandUsed = Convert.ToString(coords[0]);

					if (currentHandUsed == "r") 
					{
						GetRightHandInput(coords);
						leftHand.Reset();
					}
					else if (currentHandUsed == "l") 
					{
						GetLeftHandInput(coords);
						rightHand.Reset();
					}
				}
				Thread.Sleep(5);
			}
                
        } catch (Exception ex)
		{
			ReleaseEverything();
			UnityEngine.Debug.Log (ex.Message);
		}
	}

	void GetRightHandInput(string[] coords)
	{
		float posX = Convert.ToSingle(coords[1]);
		float posY = Convert.ToSingle(coords[2]);	
		string handClosed = Convert.ToString(coords[3]);
		string pushGesture = Convert.ToString(coords[4]);
		string swipeGesture = Convert.ToString(coords[5]);
		if (handClosed == "y") rightHand.isHandClosed = true;
		else rightHand.isHandClosed = false;
		if (pushGesture == "y") rightHand.isHandPushing = true;
		else rightHand.isHandPushing = false;
		if (swipeGesture == "sl") rightHand.swipeGesture = 1;
		else if (swipeGesture == "sr") rightHand.swipeGesture = 2;
		else rightHand.swipeGesture = 0;

		if (posX != 0 && posY != 0)
		{
			objCursorPosition.x = posX * screenRatio.x;
			objCursorPosition.y = posY * screenRatio.y;
			rightOutOfBounds = false;
		}
		else
		{
			rightOutOfBounds = true;
		}
	}

	void GetLeftHandInput(string[] coords)
	{
		float posX = Convert.ToSingle(coords[1]);
		float posY = Convert.ToSingle(coords[2]);	
		string handClosed = Convert.ToString(coords[3]);
		string pushGesture = Convert.ToString(coords[4]);
		string swipeGesture = Convert.ToString(coords[5]);
		if (handClosed == "y") leftHand.isHandClosed = true;
		else leftHand.isHandClosed = false;
		if (pushGesture == "y") leftHand.isHandPushing = true;
		else leftHand.isHandPushing = false;
		if (swipeGesture == "sl") leftHand.swipeGesture = 1;
		else if (swipeGesture == "sr") leftHand.swipeGesture = 2;
		else leftHand.swipeGesture = 0;
		
		if (posX != 0 && posY != 0)
		{
			objCursorPosition.x = posX * screenRatio.x;
			objCursorPosition.y = posY * screenRatio.y;
			leftOutOfBounds = false;
		}
		else
		{
			leftOutOfBounds = true;
		}
		
	}

	void ResetInputs()
	{
		leftHand.Reset();
		rightHand.Reset();

		prevCursorPosition = Vector3.zero;
		cursorPosition = Vector3.zero;
		objCursorPosition = Vector2.zero;
	}

	public void ToggleKinect()
	{
		if (!kinectLaunched) ActivateKinect();
		else DisableKinect();
	}
	
	public void ReleaseEverything()
	{
		if (udpThread != null) udpThread.Abort();
		if (udpc != null) udpc.Close();
		if (procKinect != null) if (procKinect.HasExited == false) procKinect.CloseMainWindow();

		ResetInputs();

		kinectLaunched = false;	
		currentPacket = "null";
	}
	
	void OnApplicationQuit () {
		ReleaseEverything();
	}
	
	void OnDisable () {
		ReleaseEverything();
	}
	
	void OnGUI () {
		if (kinectLaunched == false) return;
		GUI.depth = -10;
		if (leftOutOfBounds && rightOutOfBounds) 
		{
			GUI.DrawTexture(new Rect(cursorPosition.x - 64, cursorPosition.y - 64, 128, 128),cursorTexture3);
		}
		else
		{
			if (rightHand.isHandClosed == false)
				GUI.DrawTexture(new Rect(cursorPosition.x - 64, cursorPosition.y - 64, 128, 128),cursorTexture);
			else GUI.DrawTexture(new Rect(cursorPosition.x - 64, cursorPosition.y - 64, 128, 128),cursorTexture2);
		}
	}
	
	public void ActivateKinect()
	{
		
		if (kinectLaunched == true) return;
		UnityEngine.Debug.Log("Open Kinect");
		path = Application.dataPath + "/Kinect/GTIKinectControls.exe";
		
		udpThread = new Thread(new ThreadStart(ReceiveUDP));
		udpThread.IsBackground = true;
		udpThread.Start();
	}
	
	public void DisableKinect()
	{
		if (kinectLaunched == false) return;
		UnityEngine.Debug.Log("Close Kinect");
		ReleaseEverything();
	}

	public void ToggleBlock()
	{
		blockKinect = !blockKinect;
	}

	public void Block()
	{
		blockKinect = true;
	}

	public void Unblock()
	{
		blockKinect = false;
	}

	private void CalculateDirections()
	{
		if (blockKinect || (rightOutOfBounds&&rightHand.isHandClosed == false))
		{
			isRight = false;
			isDown = false;
			isUp = false;
			isLeft = false;
			return;
		}
		
		// Active zone - middle height
		if (cursorPosition.y > Screen.height*0.30f && cursorPosition.y < Screen.height*0.70f)
		{
			if(cursorPosition.x > Screen.width*0.80f && rightHand.isHandClosed == false) 
			{
				goingRight += Time.deltaTime;
			}
			else goingRight = 0;
	
			if(cursorPosition.x < Screen.width*0.20f && rightHand.isHandClosed == false) 
			{
				goingLeft+= Time.deltaTime;
			}
			else goingLeft = 0;
		}
		else
		{
			goingLeft = 0;
			goingRight = 0;
		}
		
		// Active zone - middle horizontal
		if (cursorPosition.x > Screen.width * 0.30f && cursorPosition.x < Screen.width * 0.70f)
		{
			if(cursorPosition.y < Screen.height*0.20f && rightHand.isHandClosed == false) 
			{
				goingDown += Time.deltaTime;
			}
			else goingDown = 0;
	
			if(cursorPosition.y > Screen.height*0.80f && rightHand.isHandClosed == false) 
			{
				goingUp+= Time.deltaTime;
			}
			else goingUp = 0;
		}

		if (goingRight > zonesTime) isRight = true;
		else isRight = false;

		if (goingLeft > zonesTime) isLeft = true;
		else isLeft = false;

		if (goingUp > zonesTime) isUp = true;
		else isUp = false;

		if (goingDown > zonesTime) isDown = true;
		else isDown = false;

	}
	
	public bool IsInCenterArea()
	{
		if (cursorPosition.y < Screen.height*0.30f) return false;
		if (cursorPosition.y > Screen.height*0.70f) return false;
		if (cursorPosition.x < Screen.width * 0.30f) return false;
		if (cursorPosition.x > Screen.width * 0.70f) return false;
		return true;
	}
	
	public void SetFocusOnROCHE()
	{
		IntPtr windowPtr = FindWindow(null, "ROCHE");
		//find main app's rectangle
		RECT rct = new RECT();
		GetWindowRect(windowPtr, ref rct);
		
		//Reset the main apps position and bring to front
		SetWindowPos(	windowPtr, 
						HWND_TOPMOST, 
						rct.Left, 
						rct.Top, 
						rct.Right-rct.Left, 
						rct.Bottom-rct.Top,
						0x0040 //SHOWWINDOW
					);
		
		//Double check to make sure
		SetForegroundWindow(windowPtr);
	}
	
}
