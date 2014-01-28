using UnityEngine;
using System.Collections;
using System.Diagnostics;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;


public class KinectTest : MonoBehaviour {
	
	private bool kinectLaunched = false;
	private Process procKinect = null; 
	private String strReceived = "null";
    UdpClient udpc = new UdpClient("127.0.0.1", 2055);
    IPEndPoint ep = null;
	
	private GameObject cube;

	
	// Use this for initialization
	void Start () {
		cube = GameObject.Find("Cube");
	}
	
	// Update is called once per frame
	void Update () {
		if (kinectLaunched)
			StartCoroutine("ReceiveUDP");
	}
	
	IEnumerator ReceiveUDP()
	{
		try
            {   
                //we can send a 'request' string if we want
                string name = "RH";
                byte[] sdata = Encoding.ASCII.GetBytes(name);
                udpc.Send(sdata, sdata.Length);
                byte[] rdata = udpc.Receive(ref ep);
                strReceived = Encoding.ASCII.GetString(rdata);

                //UnityEngine.Debug.Log (strReceived);
				if (strReceived != null && strReceived != "null")
				{
					string[] coords = strReceived.Split('-');
					//UnityEngine.Debug.Log (coords[0] + " " + coords[1]);
					Vector3 newPos = new Vector3( Convert.ToSingle(coords[0]), -400.0f-Convert.ToSingle(coords[1]),cube.transform.position.z);
					
//					float distance = Vector3.Distance(cube.transform.position, newPos);
//					float fracJourney = distance / 0.3f;
					
					cube.transform.position =  Vector3.Lerp(cube.transform.position, newPos, 0.1f);
				}
                
            } catch (Exception ex){UnityEngine.Debug.Log (ex.Message);}
		yield return new WaitForSeconds(0.3f);
	}
	
	void ToggleKinect()
	{
		
		if (!kinectLaunched)
		{	
			UnityEngine.Debug.Log("Open Kinect");
			
			//Open the Kinect Process
			procKinect = new Process();
			procKinect.StartInfo.FileName = Application.dataPath + "/GTI-Right-Hand-UDP2055.exe";
			procKinect.Start();
			kinectLaunched = true;
			
		}
		else
		{
			UnityEngine.Debug.Log("Close Kinect");
			
			//close kinect 
			procKinect.CloseMainWindow();
			kinectLaunched = false;
		}
	}
	
	void OnApplicationQuit () {
		//clean up if launched
		if (kinectLaunched) {
			UnityEngine.Debug.Log("Close Kinect Finally");
			procKinect.CloseMainWindow();
			kinectLaunched = false;	
		}
	}
	
	void OnGUI () {
		// Make a background box
		GUI.Box(new Rect(10,10,200,90), "GTI-Roche-Kinect-Sample");

		// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
		if(GUI.Button(new Rect(20,40,180,20), "Enable/Disable Kinect")) {
			
			ToggleKinect();
		}
	}
}

/*
public class Win32
{
    public const int GWL_EXSTYLE = (-20);
    public const int GWL_STYLE = (-16);
    public const int WS_VISIBLE = 0x10000000;
    public const int WS_CHILD = 0x40000000;
    public const int WS_BORDER = 0x00800000;
    public const int WM_SETTEXT = 0x000C;
    public const int LWA_ALPHA = 0x00000002;
    public const int WS_CAPTION = 0X00C0000;
 
    public const int WS_EX_WINDOWEDGE = 0x00000100;
    public const int WS_EX_DLGMODALFRAME = 0x00000001;
    public const int WS_EX_TRANSPARENT = 0x20;
    public const int WS_EX_LAYERED = 0x80000;
 
    [DllImport("user32.dll", EntryPoint = "GetParent")]
    public static extern IntPtr GetParent(IntPtr hWnd);
 
    [DllImport("User32.dll", EntryPoint = "SendMessage")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
 
    [DllImport("user32.dll", EntryPoint = "SetParent")]
    public static extern IntPtr SetParent(IntPtr child, IntPtr parent);
 
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
    public extern static int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
 
    [DllImport("user32.dll", EntryPoint = "MoveWindow")]
    public static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);
 
    [DllImport("User32.dll", EntryPoint = "GetWindowLong")]
    public static extern int GetWindowLong(IntPtr handle, int style);
 
    [DllImport("user32.dll", EntryPoint = "ShowWindow", SetLastError = true)]
    public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
 
    [System.Runtime.InteropServices.DllImport("User32.dll")]
    public static extern int SetLayeredWindowAttributes(IntPtr Handle, int crKey, byte bAlpha, int dwFlags);
 
    [DllImport("user32.dll", EntryPoint = "SetFocus")]
    public static extern IntPtr SetFocus(IntPtr hWnd);
 
}
 */