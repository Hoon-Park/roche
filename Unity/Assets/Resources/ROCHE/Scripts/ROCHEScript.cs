using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class ROCHEScript : MonoSingleton<ROCHEScript>
{
    public GameObject tagPrefab;

    public bool showLoading = true;
    public bool visibleTags = false;
    private List<GameObject> tags;
    public Dictionary<string, string[]> fichas;
    public GameObject ficha;

    [HideInInspector]
    public GameObject KinectSystem;
    [HideInInspector]
    public KinectManager kinectManager;
    private bool lastHandClose = false;
    private bool lastLeftHandClose = false;

    private GameObject GUI_Desktop;
    private GameObject GUI_Kinect;

    [HideInInspector]
    public GameObject guiOpened = null;

    public enum BuildType
    {
        Desktop = 0,
        Kinect = 1,
        Monotouch = 2,
        Tablet = 3,
		OVR = 4
    }

    public BuildType buildType = BuildType.Desktop;

    public override void Init()
    {
        // Tag management
        CreateTags();

        // KinectSystem
        KinectSystem = GameObject.Find("KinectSystem") as GameObject;
        KinectSystem.SetActive(false);

        // GUI
        GUI_Desktop = GameObject.Find("GUI_Desktop") as GameObject;
        GUI_Kinect = GameObject.Find("GUI_Kinect") as GameObject;
        GameObject GUI_Kinect_Button = GameObject.Find("GUI_Kinect_Button") as GameObject;
        GameObject GUI_Help_Screen = GameObject.Find("GUI_Help_Screen") as GameObject;
        GameObject GUI_Ficha = GameObject.Find("GUI_Ficha") as GameObject;
        GameObject GUI_Launcher = GameObject.Find("GUI_Launcher") as GameObject;
        GameObject GUI_Monotouch = GameObject.Find("GUI_Monotouch") as GameObject;
		GameObject GUI_OVR = GameObject.Find("GUI_OVR") as GameObject;

        GUI_Desktop.SetActive(false);
        GUI_Kinect.SetActive(false);
        GUI_Kinect_Button.SetActive(false);
        GUI_Help_Screen.SetActive(false);
        GUI_Ficha.SetActive(false);
        GUI_Monotouch.SetActive(false);
		GUI_OVR.SetActive(false);

        if (showLoading == false) GUI_Launcher.SetActive(false);

        switch (buildType)
        {
            case BuildType.Desktop:
                GUI_Desktop.SetActive(true);
                break;
            case BuildType.Kinect:
                GUI_Desktop.SetActive(true);
                KinectSystem.SetActive(true);
                kinectManager = KinectSystem.GetComponent<KinectManager>();
                kinectManager.DisableKinect();
                GUI_Kinect_Button.SetActive(true);
                break;
            case BuildType.Monotouch:
                GUI_Monotouch.SetActive(true);
                break;
			case BuildType.OVR:
				GUI_OVR.SetActive(true);
				break;
        }
    }

    void CreateTags()
    {
        // Create all tags
        tags = new List<GameObject>();
        GameObject mobiliarioContainer = GameObject.Find("Mobiliario");
        if (mobiliarioContainer == null) Debug.LogError("Can't find 'Mobiliario' container");
        foreach (Transform t in mobiliarioContainer.transform)
        {
            Ficha ficha = t.GetComponent<Ficha>();
            if (ficha == null) continue;
            if (ficha.activar == false) continue;

            Vector3 tagPos = t.position;
            Collider c = t.collider;
            if (c == null) c = t.GetComponentInChildren<Collider>();
            tagPos.y = c.bounds.max.y + 0.25f;
            GameObject tag = GameObject.Instantiate(tagPrefab, tagPos, Quaternion.identity) as GameObject;
            tag.transform.GetChild(0).GetChild(0).GetComponent<TextMesh>().text = ficha.tagName;
            tag.tag = "TAG"; // TROLOLOLOLO...
            tag.layer = LayerMask.NameToLayer("ROCHE_MOBILIARIO");
            tag.transform.parent = t;
            tags.Add(tag);
            if (visibleTags) tag.SetActive(true);
            else tag.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (buildType == BuildType.Desktop || (buildType == BuildType.Kinect && kinectManager.kinectLaunched == false) || buildType == BuildType.OVR)
        {
            DoKeyboardInput();
            DoMouseInput();
        }
        else if (buildType == BuildType.Kinect && kinectManager.kinectLaunched) DoKinectInput();
        else if (buildType == BuildType.Monotouch)
        {
            DoMonotouchInput();
        }
        else if (buildType == BuildType.Tablet)
        {
            // DoTabletInput();
        }
    }

    public void SwitchTagsOnOff()
    {
        visibleTags = !visibleTags;

        foreach (GameObject g in tags)
        {
            if (visibleTags) g.SetActive(true);
            else g.SetActive(false);
        }
    }

    void DoKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.T)) SwitchTagsOnOff();
    }

    void DoMouseInput()
    {
        if (Input.GetMouseButtonDown(0) && visibleTags == true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO") && ficha.activeInHierarchy == false)
                {
                    Ficha f = obj.GetComponent<Ficha>();
                    if (f == null && obj.transform.parent != null)
                    {
                        obj = obj.transform.parent.gameObject;
                        if (obj != null)
                        {
                            f = obj.GetComponent<Ficha>();
                            if (f == null && obj.transform.parent != null)
                            {
                                obj = obj.transform.parent.gameObject;
                                if (obj != null)
                                    f = obj.GetComponent<Ficha>();
                            }
                        }
                    }
                    if (f != null && f.activar == true && guiOpened == null)
                    {
                        guiOpened = ficha.gameObject;
                        ficha.SetActive(true);
                        ficha.GetComponent<MMCGUI_Ficha>().ShowTag(obj);
                    }
                }
            }
        }
    }
	
    void DoMonotouchInput()
    {
        if (Input.GetMouseButtonDown(0) && visibleTags == true)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO") && ficha.activeInHierarchy == false)
                {
                    Ficha f = obj.GetComponent<Ficha>();
                    if (f == null && obj.transform.parent != null)
                    {
                        obj = obj.transform.parent.gameObject;
                        if (obj != null)
                        {
                            f = obj.GetComponent<Ficha>();
                            if (f == null && obj.transform.parent != null)
                            {
                                obj = obj.transform.parent.gameObject;
                                if (obj != null)
                                    f = obj.GetComponent<Ficha>();
                            }
                        }
                    }
                    if (f != null && f.activar == true && guiOpened == null)
                    {
                        guiOpened = ficha.gameObject;
                        ficha.SetActive(true);
                        ficha.GetComponent<MMCGUI_Ficha>().ShowTag(obj);
                    }
                }
            }
        }
	 	if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began && visibleTags == true)
        {
            Vector2 pos = Input.GetTouch(0).position;

            Ray ray = Camera.main.ScreenPointToRay(pos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                GameObject obj = hit.collider.gameObject;
                if (obj.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO") && ficha.activeInHierarchy == false)
                {
                    Ficha f = obj.GetComponent<Ficha>();
                    if (f == null && obj.transform.parent != null)
                    {
                        obj = obj.transform.parent.gameObject;
                        if (obj != null)
                        {
                            f = obj.GetComponent<Ficha>();
                            if (f == null && obj.transform.parent != null)
                            {
                                obj = obj.transform.parent.gameObject;
                                if (obj != null)
                                    f = obj.GetComponent<Ficha>();
                            }
                        }
                    }
                    if (f != null && f.activar == true && guiOpened == null)
                    {
                        guiOpened = ficha.gameObject;
                        ficha.SetActive(true);
                        ficha.GetComponent<MMCGUI_Ficha>().ShowTag(obj);
                    }
                }
            }
        }
    }

    void DoKinectInput()
    {
        if (kinectManager.leftHand.isHandClosed && lastLeftHandClose == false)
        {
            kinectManager.ToggleBlock();
            BlockKinect(kinectManager.blockKinect);
        }

        if (kinectManager.rightHand.isHandClosed && lastHandClose == false && visibleTags == true && kinectManager.blockKinect == false)
        {
            Vector2 handPos = new Vector2(kinectManager.cursorPosition.x, Screen.height - kinectManager.cursorPosition.y);
            HandleKinectRay(Camera.main.ScreenPointToRay(handPos));
        }

        lastHandClose = kinectManager.rightHand.isHandClosed;
        lastLeftHandClose = kinectManager.leftHand.isHandClosed;


    }

    bool HandleKinectRay(Ray ray)
    {
        bool collided = false;
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            GameObject obj = hit.collider.gameObject;
            if (obj.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO") && ficha.activeInHierarchy == false)
            {
                collided = true;
                Ficha f = obj.GetComponent<Ficha>();
                if (f == null && obj.transform.parent != null)
                {
                    obj = obj.transform.parent.gameObject;
                    if (obj != null)
                    {
                        f = obj.GetComponent<Ficha>();
                        if (f == null && obj.transform.parent != null)
                        {
                            obj = obj.transform.parent.gameObject;
                            if (obj != null)
                                f = obj.GetComponent<Ficha>();
                        }
                    }
                }
                if (f != null && f.activar == true && guiOpened == null)
                {
                    guiOpened = ficha.gameObject;
                    ficha.SetActive(true);
                    ficha.GetComponent<MMCGUI_Ficha>().ShowTag(obj);
                }
            }
        }
        return collided;
    }

    public void ActivateKinect(bool activate)
    {
        if (activate)
        {
            GUI_Desktop.SetActive(false);
            GUI_Kinect.SetActive(true);
            kinectManager.ActivateKinect();
        }
        else
        {
            GUI_Desktop.SetActive(true);
            GUI_Kinect.SetActive(false);
            kinectManager.DisableKinect();
        }
    }

    public void ToggleKinectWindow()
    {
        kinectManager.ToggleKWindow();
    }

    public void BlockKinect(bool block)
    {
        if (block)
        {
            kinectManager.Block();
            GUI_Kinect.SetActive(false);
        }
        else
        {
            kinectManager.Unblock();
            GUI_Kinect.SetActive(true);
        }
    }
}
