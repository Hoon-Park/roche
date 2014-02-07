using System;
using System.Collections.Generic;
using System.Threading;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Linq;
using Random = System.Random;

/*
 * Main class for ROCHE Module Editor
 * Mobile Media Content - 2013
 * Grup Tecnologies Interactives, UPF - 2013
 */

public class ROCHE : EditorWindow
{
	/*
	 * GLOBAL VARIABLES
	 */
	
	// Textures
	private static Texture2D 	rocheIcon;
	private static Texture2D 	YLightIconOne;
	private static Texture2D 	YLightIconThree;
	private static Texture2D 	YLightIconFive;
	private static Texture2D 	BLightIconOne;
	private static Texture2D 	BLightIconThree;
	private static Texture2D 	BLightIconFive;
	
	// Misc variables
	private static GameObject	furnitureContainer;
	private static GameObject	lightContainer;
	private static GameObject	sceneBuilding; // Imported Building GameObject
	private static List<int> 	createdIDs = new List<int>(); // Created GameObject IDs.
	
	private Bounds				sceneBuildingBounds;

	private Texture2D auxFloorTexture, auxFloorCadTexture, auxWallTexture, auxCeilingTexture;

	// UI variables
	enum TOOLBAR_POS
	{
		//PROJECT = 0,
		//SCENE = 1,
		//LIGHTING = 2,
		//EXPORT = 3,
		DESIGN = 0,
		APP = 1,
		INFO = 2
	};
	
	enum PLATFORM_SELECTED
	{
		WINDOWS = 0,
		MAC_OS_X = 1
	};
	
	private TOOLBAR_POS	toolbarPos;
	private Vector2 	scrollPos = Vector2.zero;
	private string[] 	toolbarStrings = new string[] { "DISEÑO", "APP", "INFO" };
	
	// Collision Detection variables
	private bool 		viewCollisions = false;
	private GameObject 	lastSelectedObject;
	
	// Light Generation
	public float 		lightDistance = 1.0f;
	
	/*
	 * MAIN FUNCTIONS 
	 */
	
	// New Project Menu Item
	[MenuItem("ROCHE/Nuevo Proyecto")]
	public static void CreateNewProject ()
	{
		// Create new scene (it asks to save the current one)
		EditorApplication.NewScene();
		
		// Clean the scene
		foreach(GameObject gameObj in GameObject.FindObjectsOfType(typeof(GameObject)))
			GameObject.DestroyImmediate(gameObj);
		
		// Instantiate the ROCHE main GameObject
		//GameObject g_roche = GameObject.Instantiate((GameObject) Resources.Load("ROCHE/_ROCHE",typeof(GameObject))) as GameObject;
		//g_roche.layer = LayerMask.NameToLayer("ROCHE");
		//g_roche.name = "_ROCHE";
		//g_roche.transform.position = Vector3.zero;
		
		// Set Skybox
		RenderSettings.skybox = (Material) Resources.Load ("ROCHE/Assets/Skybox",typeof(Material)) as Material;
		
		// Set Render Settings
		RenderSettings.ambientLight = new Color(80.0f/255.0f,80.0f/255.0f,80.0f/255.0f,1.0f);
		
		// Open the ROCHE Inspector
		EditorWindow.GetWindow (typeof(ROCHE));
	}
	
	// ROCHE Inspector Menu Item
	[MenuItem("ROCHE/Abrir Inspector")]
	public static void ShowWindow ()
	{
		// Open the ROCHE Inspector
		EditorWindow.GetWindow (typeof(ROCHE));
	}
	
	// Called when class is initialized for the first time
	static ROCHE()
	{	
		// Load all textures
		rocheIcon = (Texture2D)Resources.Load ("ROCHE/Assets/icon", typeof(Texture2D));
		YLightIconOne = (Texture2D)Resources.Load ("ROCHE/Assets/YLT", typeof(Texture2D));
		YLightIconThree = (Texture2D)Resources.Load ("ROCHE/Assets/3YLT", typeof(Texture2D));
		YLightIconFive = (Texture2D)Resources.Load ("ROCHE/Assets/5YLT", typeof(Texture2D));
		BLightIconOne = (Texture2D)Resources.Load ("ROCHE/Assets/BLT", typeof(Texture2D));
		BLightIconThree = (Texture2D)Resources.Load ("ROCHE/Assets/3BLT", typeof(Texture2D));
		BLightIconFive = (Texture2D)Resources.Load ("ROCHE/Assets/5BLT", typeof(Texture2D));
		
		// Set Max Quality Settings
		QualitySettings.SetQualityLevel(QualitySettings.names.Length-3,true);
		
		PlayerSettings.productName = "ROCHE";
	}
	
	// Called each frame we repaint the ROCHE inspector
	void OnGUI ()
	{
		if (Application.isPlaying)
		{
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.LabelField ("La aplicación está en modo Ejecución.", EditorStyles.boldLabel);
			EditorGUILayout.EndVertical();
			return;
		}
		
		if (Lightmapping.isRunning)
		{
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.LabelField ("Lightmapping trabajando. Espere por favor...", EditorStyles.boldLabel);
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Cancelar",GUILayout.MaxWidth(80))) Lightmapping.Cancel();
			GUILayout.FlexibleSpace();
			
			GUILayout.EndHorizontal();
			
			EditorGUILayout.EndVertical();
			return;
		}
		
		// If we have no building GameObject on scene, just show the "ESCENA" tab.
		if (sceneBuilding == null) sceneBuilding = GameObject.Find("Edificio") as GameObject;
		if(sceneBuilding == null) 
		{
			TabStart();
			return;
		}

		toolbarPos = (TOOLBAR_POS) GUILayout.SelectionGrid ((int) toolbarPos, toolbarStrings,3);
		switch (toolbarPos) 
		{
		case TOOLBAR_POS.DESIGN:
			TabDesign();
			break;
		case TOOLBAR_POS.APP:
			TabApp();
			break;
		case TOOLBAR_POS.INFO:
			TabInfo();
			break;
		}
	}
	
	// Update function called each frame on the Unity Editor
	private void Update ()
	{
		// Repaint the ROCHE Inspector window
		Repaint();

		// Instantiate them if we can't locate them
		furnitureContainer = GameObject.Find("Mobiliario");
		if (!furnitureContainer) 
		{
			// Instantiate Containers
			furnitureContainer = new GameObject("Mobiliario");
			furnitureContainer.layer = LayerMask.NameToLayer("ROCHE");
			furnitureContainer.transform.position = Vector3.zero;
		}

		lightContainer = GameObject.Find("Luces");
		if (!lightContainer)
		{
			lightContainer = new GameObject("Luces");
			lightContainer.layer = LayerMask.NameToLayer("ROCHE");
			lightContainer.transform.position = Vector3.zero;
		}

		// Locate the Building GameObject so the Inspector window can modify the shown contents
		sceneBuilding = GameObject.Find("Edificio");
		// Reposition
		if (sceneBuilding) 
		{
			if (sceneBuilding.transform.position != Vector3.zero)
				sceneBuilding.transform.position = Vector3.zero;
		}

		// Manage new GameObjects
		GameObject currentGameObject = Selection.activeGameObject;
		if (currentGameObject != null && currentGameObject.activeInHierarchy)
		{
			// Check instance
			int currentGOID = currentGameObject.GetInstanceID();
			// Discard special GameObjects or any object not new
			if (createdIDs.Contains(currentGOID) == false && currentGameObject.transform.parent == null && currentGameObject != sceneBuilding && currentGameObject.layer != LayerMask.NameToLayer("ROCHE") && currentGameObject.layer != LayerMask.NameToLayer("ROCHE_EDIFICIO") )
			{
				if (currentGameObject.layer == LayerMask.NameToLayer("ROCHE_LUZ"))
				{
					currentGameObject.transform.parent = lightContainer.transform;
					createdIDs.Add(currentGOID);
					SetStaticRecursively(currentGameObject);
					CenterCameraOnGameObject(currentGameObject);
				}
				else
				{
					SetLayerRecursively(currentGameObject, "ROCHE_MOBILIARIO");
					SetStaticRecursively(currentGameObject);
					currentGameObject.transform.parent = furnitureContainer.transform;
//					PlaceOnFloor(currentGameObject.transform, false);
					Vector3 tempPos = currentGameObject.transform.position;
					GameObject pref = PrefabUtility.GetPrefabParent(currentGameObject) as GameObject;
					if (pref != null) 
						tempPos.y = pref.transform.position.y;
					currentGameObject.transform.position = tempPos;
					SetCollisionsRecursively(currentGameObject);
					createdIDs.Add(currentGOID);
					CenterCameraOnGameObject(currentGameObject);
				}
			}
		}

		// Collision Detection
		ViewObjectCollisions();
	}


	#region Start Tab

	/// <summary>
	/// Introduction Tab to import the building.
	/// </summary>
	private void TabStart()
	{
		// Vertical Scrollbar
		scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
		
		// ROCHE Header Icon
		DrawRocheIconBox();		
		
		// "EDIFICIO" Box
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField ("INICIO", EditorStyles.boldLabel);

		GUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		GUILayout.TextArea ("1.1 - Importación de FBX",EditorStyles.boldLabel);
		GUILayout.Space (10);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (40);
		EditorGUILayout.LabelField ("Arrastra tu fichero FBX del edificio a la carpeta \"/ROCHE/Inicio/Modelos/\".", EditorStyles.wordWrappedLabel);
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		GUILayout.TextArea ("1.2 - Importar a escena",EditorStyles.boldLabel);
		GUILayout.Space (10);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (40);
		EditorGUILayout.LabelField ("Arrastra el modelo al siguiente recuadro:", EditorStyles.wordWrappedLabel);
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
		GUILayout.Space (10);
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space (40);			
		GameObject pObj = null;
		pObj = EditorGUILayout.ObjectField(pObj,typeof(GameObject),false,GUILayout.MinWidth(50),GUILayout.Height(50)) as GameObject;
		if (pObj != null && (PrefabUtility.GetPrefabType(pObj) == PrefabType.ModelPrefab || PrefabUtility.GetPrefabType(pObj) == PrefabType.Prefab))
		{
			if (sceneBuilding != null) GameObject.DestroyImmediate(sceneBuilding); 
			// Instantiate it, add layers+colliders and reposition it at the center
			sceneBuilding = GameObject.Instantiate(pObj) as GameObject;
			sceneBuilding.name = "Edificio";
			
			SetLayerRecursively(sceneBuilding,"ROCHE_EDIFICIO");
			sceneBuilding.layer = LayerMask.NameToLayer("ROCHE");
			SetCollisionsRecursively(sceneBuilding);
			SetStaticRecursively(sceneBuilding);
			sceneBuilding.transform.position = new Vector3 (0, 0, 0);
			
			// Get building real bounds using children renderers
			CalculateBuildingBounds();
			CenterCameraOnGameObject(sceneBuilding);
			//toolbarPos = TOOLBAR_POS.SCENE;
			pObj = null;
		}
		else pObj = null;
		
		//			GUILayout.Box("",GUILayout.Width(2000),GUILayout.MinWidth(40),GUILayout.Height(40));
		GUILayout.Space (20);GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		GUILayout.Space (5);
		GUILayout.EndVertical ();
		GUILayout.EndScrollView ();
		return;
	}

	#endregion

	#region Design Tab

	bool showFloor = false;
	bool showWalls = false;
	bool showCeiling = false;
	bool showLights1 = false;
	bool showLights2 = false;

	private void TabDesign()
	{
		// Vertical Scrollbar
		scrollPos = EditorGUILayout.BeginScrollView (scrollPos);

		// ROCHE Header Icon
		DrawRocheIconBox();		
		GUILayout.BeginVertical();

		EditorGUILayout.BeginVertical ("Box");
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Eliminar edificio actual", GUILayout.MaxWidth (160))) {
			Undo.RecordObject(sceneBuilding,sceneBuilding.name);
			GameObject.DestroyImmediate(sceneBuilding);			
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		EditorGUILayout.EndVertical();

		#region textures
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField("Texturas", EditorStyles.boldLabel);

		showFloor = EditorGUILayout.Foldout(showFloor, "Suelo");
		if (showFloor)
		{
			GameObject suelo = GameObject.Find("Suelo");
			if (suelo == null)
			{
				EditorGUILayout.LabelField ("Objeto 'Suelo' no encontrado.", EditorStyles.label);
			}
			else
			{
				EditorGUILayout.BeginVertical();

				EditorGUILayout.LabelField("Plano CAD");
				suelo.renderer.sharedMaterial.shader = Shader.Find("ROCHE/Detail");
				auxFloorCadTexture = EditorGUILayout.ObjectField(auxFloorCadTexture,
			                                              typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
				if(GUILayout.Button("Aplicar", GUILayout.MaxWidth(80)))
				{
					suelo.renderer.sharedMaterial.SetTexture("_Detail", auxFloorCadTexture);	
					suelo.renderer.sharedMaterial.SetFloat("_DetailActive",1);
				}
				EditorGUILayout.EndVertical();
				EditorGUILayout.BeginVertical();
				EditorGUILayout.LabelField("Textura Suelo");
				Texture tex = suelo.renderer.sharedMaterial.mainTexture;
				if (auxFloorTexture == null && tex != null) auxFloorTexture = (Texture2D) tex;
				auxFloorTexture = EditorGUILayout.ObjectField(auxFloorTexture,
				                                              typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
				if(GUILayout.Button("Aplicar", GUILayout.MaxWidth(80)))
				{
					suelo.renderer.sharedMaterial.mainTexture = auxFloorTexture;	
					suelo.renderer.sharedMaterial.SetFloat("_DetailActive",0);
				}
				EditorGUILayout.EndVertical();
			}
		}

		showWalls = EditorGUILayout.Foldout(showWalls, "Paredes");
		if (showWalls)
		{
			GameObject paredes = GameObject.Find("Paredes");
			if (paredes == null) paredes = GameObject.Find("Pared");
			if (paredes == null)
			{
				EditorGUILayout.LabelField ("Objeto 'Paredes' no encontrado.", EditorStyles.label);
			}
			else
			{
				EditorGUILayout.BeginVertical();
				
				EditorGUILayout.LabelField("Textura Paredes");
				//Texture tex = paredes.renderer.sharedMaterial.mainTexture;
				//paredes.renderer.sharedMaterial.shader = Shader.Find("ROCHE/Detail");
				auxWallTexture = EditorGUILayout.ObjectField(auxWallTexture,
				                                                 typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
				if(GUILayout.Button("Aplicar", GUILayout.MaxWidth(80)))
				{
					paredes.renderer.sharedMaterial.mainTextureScale = new Vector2 (1, 1);
					paredes.renderer.sharedMaterial.mainTextureOffset = new Vector2 (0, 0);
					paredes.renderer.sharedMaterial.mainTexture = auxWallTexture;	
				}
				EditorGUILayout.EndVertical();
			}
		}

		showCeiling = EditorGUILayout.Foldout(showCeiling, "Techo");
		if (showCeiling)
		{
			GameObject techo = GameObject.Find("Techo");
			if (techo == null)
			{
				EditorGUILayout.LabelField ("Objeto 'Techo' no encontrado.", EditorStyles.label);
			}
			else
			{
				EditorGUILayout.BeginVertical();
				
				EditorGUILayout.LabelField("Textura Techo");
				//Texture tex = paredes.renderer.sharedMaterial.mainTexture;
				//paredes.renderer.sharedMaterial.shader = Shader.Find("ROCHE/Detail");
				auxCeilingTexture = EditorGUILayout.ObjectField(auxCeilingTexture,
				                                             typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
				if(GUILayout.Button("Aplicar", GUILayout.MaxWidth(80)))
				{
					techo.renderer.sharedMaterial.mainTextureScale = new Vector2 (1, 1);
					techo.renderer.sharedMaterial.mainTextureOffset = new Vector2 (0, 0);
					techo.renderer.sharedMaterial.mainTexture = auxCeilingTexture;	
				}
				EditorGUILayout.EndVertical();
			}
		}

		EditorGUILayout.EndVertical();
		#endregion textures

		EditorGUILayout.BeginVertical ("Box");

		EditorGUILayout.LabelField("Mobiliario", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Arrastra los objetos de mobiliario directamente a la escena.",MessageType.Info);

		EditorGUILayout.EndVertical();
		EditorGUILayout.BeginVertical ("Box");

		EditorGUILayout.LabelField("Posicionamiento", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Utiliza el editor para posicionar, rotar y escalar los objetos en la escena, o utiliza las siguientes herramientas seleccionando uno o más objetos.",MessageType.Info);


		// Center GameObject at the center of the Building GameObject
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Centrar en edificio", GUILayout.MaxWidth (160))) {
			//Undo.RecordObjects(.RegisterSceneUndo ("Move to Center");
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms) PlaceOnFloor(t, true);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Colocar sobre el suelo", GUILayout.MaxWidth (160))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms) PlaceOnFloor(t, false);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Apoyar contra la pared", GUILayout.MaxWidth (160))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			int layerMask = (1 << 9);
			foreach (Transform tr in Selection.transforms)
			{
				Transform t = GetParentFurnitureTransform(tr);
				Undo.RecordObject (t,t.name);
				
				RaycastHit hit;
				Bounds b = t.renderer.bounds;
				Vector3 pos = Vector3.zero;
				float best_dist = Mathf.Infinity;

				if (Physics.Raycast(new Ray(b.center,Vector3.left),out hit,Mathf.Infinity,layerMask))
				{
					if (hit.distance < best_dist && hit.distance > 1.0f)
					{
						best_dist = hit.distance;
						pos = new Vector3(hit.point.x + b.size.x/2, t.position.y, t.position.z);
					}
				}
				if (Physics.Raycast(new Ray(b.center,Vector3.right),out hit,Mathf.Infinity,layerMask))
				{
					if (hit.distance < best_dist && hit.distance > 1.0f)
					{
						best_dist = hit.distance;
						pos = new Vector3(hit.point.x - b.size.x/2, t.position.y, t.position.z);
					}
				}
				if (Physics.Raycast(new Ray(b.center,Vector3.forward),out hit,Mathf.Infinity,layerMask))
				{
					if (hit.distance < best_dist && hit.distance > 1.0f)
					{
						best_dist = hit.distance;
						pos = new Vector3(t.position.x, t.position.y, hit.point.z - b.size.z/2);
					}
				}
				if (Physics.Raycast(new Ray(b.center,Vector3.back),out hit,Mathf.Infinity,layerMask))
				{
					if (hit.distance < best_dist && hit.distance > 1.0f)
					{
						best_dist = hit.distance;
						pos = new Vector3(t.position.x, t.position.y, hit.point.z + b.size.z/2);
					}
				}
				if (best_dist != Mathf.Infinity)
				{
					t.position = pos;
				}

			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField("Rotación", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Selecciona uno o más objetos.",MessageType.Info);

		// Rotation Buttons
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Rotar X", GUILayout.MaxWidth (80))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms)
			{
				Undo.RecordObject(t,t.name);
				t.RotateAround(Vector3.right, (float)Math.PI/2.0f);
			}
		}

		if (GUILayout.Button ("Rotar Y", GUILayout.MaxWidth (80))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms) 
			{
				Undo.RecordObject(t,t.name);
				t.RotateAround(Vector3.up, (float)Math.PI/2.0f);
			}
		}

		if (GUILayout.Button ("Rotar Z", GUILayout.MaxWidth (80))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms) 
			{
				Undo.RecordObject(t,t.name);
				t.RotateAround(Vector3.forward,(float)Math.PI/2.0f);
			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Box");

		EditorGUILayout.LabelField("Alineamiento", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Selecciona dos o más objetos.",MessageType.Info);

		// Axis Alignment Buttons. They require to select a group of 2 or more objects
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Alinear X", GUILayout.MaxWidth (80))) {
			AlignGameObjects (0);
		}
		if (GUILayout.Button ("Alinear Y", GUILayout.MaxWidth (80))) {
			AlignGameObjects (1);
		}
		if (GUILayout.Button ("Alinear Z", GUILayout.MaxWidth (80))) {
			AlignGameObjects (2);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");

		EditorGUILayout.LabelField("Iluminación", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Creación de puntos de luz.",MessageType.Info);
		// Light types (prefabs)
		showLights1 = EditorGUILayout.Foldout(showLights1, "Manual");
		if (showLights1)
		{
			EditorGUILayout.HelpBox("Utiliza los botones para crear un tipo de luz en la escena.",MessageType.Info);
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button (YLightIconOne, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				Undo.RegisterSceneUndo ("Light1");
				CreateLight(new Vector3(0,sceneBuildingBounds.center.y,0));
			}
			if (GUILayout.Button (YLightIconThree, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				Undo.RegisterSceneUndo ("Light2");
				CreateLight(new Vector3(-4,sceneBuildingBounds.center.y,0));
				CreateLight(new Vector3(0,sceneBuildingBounds.center.y,0));
				CreateLight(new Vector3(4,sceneBuildingBounds.center.y,0));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal ();
			 
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button (BLightIconOne, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				Undo.RegisterSceneUndo ("Light4");
				CreateLightBlue(new Vector3(0,sceneBuildingBounds.center.y,0));
			}
			if (GUILayout.Button (BLightIconThree, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				Undo.RegisterSceneUndo ("Light5");
				CreateLightBlue(new Vector3(-4,sceneBuildingBounds.center.y,0));
				CreateLightBlue(new Vector3(0,sceneBuildingBounds.center.y,0));
				CreateLightBlue(new Vector3(4,sceneBuildingBounds.center.y,0));
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal ();
		}
		
		showLights2 = EditorGUILayout.Foldout(showLights2, "Automático");
		if (showLights2)
		{
			EditorGUILayout.HelpBox("Define distancia entre puntos de luz y selecciona el tipo de luz deseado.",MessageType.Info);
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			GUILayout.Space(20);
			lightDistance = EditorGUILayout.Slider(lightDistance, 3.0f, 20.0f, GUILayout.MaxWidth (1000));
			GUILayout.Space(20);
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal ();
			
			GUILayout.Space (5);
			
			// Buttons for Automatic Light Generation
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button (YLightIconFive, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				GenerateLights("ROCHE/Prefabs/YellowLight");
			}
			
			if (GUILayout.Button (BLightIconFive, GUILayout.MaxWidth (35), GUILayout.MaxHeight (35))) 
			{
				GenerateLights("ROCHE/Prefabs/BlueLight");
			}
			
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal ();
		}
		EditorGUILayout.HelpBox("Puedes eliminar luces manualmente o utilizar el siguiente botón para eliminar todas las luces de la escena.",MessageType.Warning);

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Eliminar luces", GUILayout.MaxWidth (160))) {
			foreach (GameObject g in GameObject.FindObjectsOfType(typeof(GameObject))) if (g.layer == LayerMask.NameToLayer("ROCHE_LUZ"))
				DestroyImmediate (g);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.HelpBox("Elige un nivel de calidad para la generacion de mapas de luz.",MessageType.Info);
		// Buttons for Lightmap Generation
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Bajo", GUILayout.MaxWidth (80))) {
			GameObject[] gs = GameObject.FindSceneObjectsOfType (typeof(GameObject)) as GameObject[];
			foreach (GameObject g in gs)
			{
				if (g.layer != LayerMask.NameToLayer("ROCHE")) g.isStatic = true;
			}
			LightmapEditorSettings.bounces = 1;
			LightmapEditorSettings.quality = LightmapBakeQuality.Low;
			LightmapEditorSettings.finalGatherRays = 400;
			Lightmapping.BakeAsync ();
		}
		if (GUILayout.Button ("Medio", GUILayout.MaxWidth (80))) {
			GameObject[] gs = GameObject.FindSceneObjectsOfType (typeof(GameObject)) as GameObject[];
			foreach (GameObject g in gs)
			{
				if (g.layer != LayerMask.NameToLayer("ROCHE")) g.isStatic = true;
			}
			LightmapEditorSettings.bounceBoost = 1;
			LightmapEditorSettings.bounceIntensity = 1;
			LightmapEditorSettings.aoAmount = 0;
			LightmapEditorSettings.finalGatherInterpolationPoints = 15;
			LightmapEditorSettings.bounces = 1;
			LightmapEditorSettings.finalGatherRays = 550;
			LightmapEditorSettings.quality = LightmapBakeQuality.High;
			Lightmapping.BakeAsync ();
		}
		if (GUILayout.Button ("Alto", GUILayout.MaxWidth (80))) {
			GameObject[] gs = GameObject.FindSceneObjectsOfType (typeof(GameObject)) as GameObject[];
			foreach (GameObject g in gs)
			{
				if (g.layer != LayerMask.NameToLayer("ROCHE")) g.isStatic = true;
			}
			
			LightmapEditorSettings.bounces = 2;
			
			LightmapEditorSettings.finalGatherRays = 1000;
			LightmapEditorSettings.quality = LightmapBakeQuality.High;
			Lightmapping.BakeAsync ();
		}
		GUILayout.FlexibleSpace();
		
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");

		EditorGUILayout.LabelField("Exportar diseño", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Permite exportar la escena en un paquete .unitypackage para su posterior uso. El archivo resultante se ubicará en la carpeta /ROCHE/Proyectos.",MessageType.Info);
		if(GameObject.Find("_ROCHE") != null)
			EditorGUILayout.HelpBox("El proyecto se encuentra en fase 'App', si exporta el diseño de escena se perderá la informacion de aplicación.",MessageType.Warning);
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Exportar", GUILayout.MaxWidth (160))) {
			GameObject groche = GameObject.Find("_ROCHE") as GameObject;
			if (groche != null) GameObject.DestroyImmediate(groche);

			bool success = EditorApplication.SaveScene();
			if (success == false) return;
			string pathScene = EditorApplication.currentScene;
			//String pathToSave = "Assets/ROCHE/Proyectos/" + EditorApplication.currentScene;
			Debug.Log (pathScene);
			AssetDatabase.ExportPackage(pathScene, pathScene + ".unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets|ExportPackageOptions.IncludeDependencies );
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		EditorGUILayout.EndVertical();

		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView ();
	}

	#endregion

	#region App Tab

	private String clientName = "";
	private void TabApp()
	{
		// Vertical Scrollbar
		scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
		
		// ROCHE Header Icon
		DrawRocheIconBox();		
		GUILayout.BeginVertical();

		if (GameObject.Find("_ROCHE") == null)
		{
			EditorGUILayout.BeginVertical ("Box");
			EditorGUILayout.HelpBox("El proyecto se encuentra en fase de Diseño. ¿Desea pasar a fase 'App'? Si vuelve a exportar el diseño de la escena se perderá la información de aplicación.",MessageType.Info);
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button ("Activar fase 'App'", GUILayout.MaxWidth (120))) 
			{
				GameObject _ROCHE = GameObject.Instantiate(Resources.Load("ROCHE/_ROCHE") as GameObject) as GameObject;
				_ROCHE.name = "_ROCHE";
				_ROCHE.transform.position = Vector3.zero;	
				_ROCHE.transform.position = new Vector3(sceneBuildingBounds.center.x, _ROCHE.transform.position.y, sceneBuildingBounds.center.z);
				Vector3 rPos = _ROCHE.transform.position;
				rPos.y = 1.5f;
				_ROCHE.transform.position = rPos;	
				SetLayerRecursively(_ROCHE,"ROCHE");
				_ROCHE.GetComponent<ROCHEScript>().buildType = ROCHEScript.BuildType.Desktop;
				CenterCameraOnGameObject(_ROCHE);
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndScrollView();
			return;
		}

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField("Colocación de cámara", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Utiliza el 'gizmo' y las herramientas del editor para posicionar el objeto '_ROCHE' en la posición deseada.",MessageType.Info);
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");
		// View collisions on editor scene
		EditorGUILayout.LabelField("Sistema de colisiones", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox("Muestra las colisiones para volver transparente el objeto seleccionado si este colisiona con otro objeto.",MessageType.Info);
		GUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		viewCollisions = EditorGUILayout.Toggle("Mostrar colisiones",viewCollisions,EditorStyles.toggle);
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal ();

		// View collisions on editor scene
		GUILayout.BeginHorizontal ();
		GUILayout.Space (20);
		EditorGUILayout.LabelField ("Añadir o eliminar colisiones a los objetos seleccionados.", EditorStyles.wordWrappedLabel);
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal ();
		GUILayout.Space (5);
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Añadir", GUILayout.MaxWidth (80))) 
		{
			Undo.RegisterSceneUndo ("AddCollisions");
			
			foreach (GameObject g in Selection.gameObjects) 
			{
				SetCollisionsRecursively(g);
			}
		}
		if (GUILayout.Button ("Eliminar", GUILayout.MaxWidth (80))) 
		{
			Undo.RegisterSceneUndo ("DeleteCollisions");
			foreach (GameObject g in Selection.gameObjects) 
			{
				RemoveCollisionsRecursively(g);
			}
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField ("Ficha del objeto", EditorStyles.boldLabel);
		if (Selection.activeGameObject == null || Selection.activeGameObject.layer != LayerMask.NameToLayer("ROCHE_MOBILIARIO"))
		{
			EditorGUILayout.HelpBox("Selecciona un objeto de mobiliario para editar su ficha.", MessageType.Info);
		}
		else 
		{
			// Get the Ficha Component
			GameObject obj = Selection.activeGameObject;
			if (obj.transform.childCount == 0)
			{
				GameObject parent = obj.transform.parent.gameObject;
				if (parent.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO")) obj = parent;
			}
			Ficha ficha = obj.GetComponent<Ficha>();
			if (ficha == null) ficha = obj.AddComponent<Ficha>();
			
			EditorGUILayout.HelpBox("Rellena los siguientes campos para personalizar la Ficha del objeto.",MessageType.Info);

			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			ficha.activar = EditorGUILayout.Toggle("Mostrar Ficha", ficha.activar, EditorStyles.toggle );
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Tag:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.tagName = GUILayout.TextField(ficha.tagName, 10);
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Nombre:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.nombre = GUILayout.TextField(ficha.nombre,60);
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Área:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.area = GUILayout.TextField(ficha.area, 60);
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField("Descripción:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.descripcion = EditorGUILayout.TextArea(ficha.descripcion, GUILayout.MaxWidth(250));
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Propiedades:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.propiedades = EditorGUILayout.TextArea(ficha.propiedades, GUILayout.MaxWidth(250));
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Imagen:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			ficha.imagen = EditorGUILayout.ObjectField(ficha.imagen,
                    typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			
			GUILayout.Space(5);
			
			EditorUtility.SetDirty(ficha);
		}
		GUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField("Sobreescribir Objeto", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox ("Puedes aplicar las propiedades e información de esta instancia de objeto al objeto original (almacenar info de ficha, colisiones, etc...).", MessageType.Info);

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Sobreescribir", GUILayout.MaxWidth (100))) {
			if (Selection.transforms.Length == 0) 
			{
				EditorUtility.DisplayDialog ("Error", "No se ha seleccionado ningún objeto.", "Aceptar");
				return;
			}
			foreach (Transform t in Selection.transforms)
			{
				Undo.RecordObject (t, t.name);
				PrefabUtility.ReplacePrefab(t.gameObject, PrefabUtility.GetPrefabParent(t.gameObject), ReplacePrefabOptions.ConnectToPrefab);
			}	
		}

		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		GUILayout.EndVertical ();
		
		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField("Propiedades de Proyecto", EditorStyles.boldLabel);
		GameObject GUIClient = GameObject.Find("GUI_Client");
		if (GUIClient != null) 
		{
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Nombre del Cliente:");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			MMCGUIClient gClient = GUIClient.GetComponent<MMCGUIClient>();
			gClient.text = GUILayout.TextArea(gClient.text, 32, GUILayout.MinHeight(20), GUILayout.MinWidth(60));
			GameObject GUIClientLauncher = GameObject.Find("GUI_Client_Launcher");
			if (GUIClientLauncher != null) 
			{
				MMCGUILabel gClientLauncher = GameObject.Find("GUI_Client_Launcher").GetComponent<MMCGUILabel>();
				gClientLauncher.text = gClient.text;
				EditorUtility.SetDirty(gClientLauncher);
			}
			PlayerPrefs.SetString("client",gClient.text);
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Logo del Cliente (128x64px):");
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(40);
			
			gClient.texture = EditorGUILayout.ObjectField(gClient.texture,
	                typeof(Texture2D), false, GUILayout.Width(80), GUILayout.Height(80)) as Texture2D;
			GUILayout.Space (20);
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(5);
			EditorUtility.SetDirty(gClient);
		}
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Texto de Versión:");
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(40);
		MMCGUILabel versionTextLabel = GameObject.Find("GUI_Version").GetComponent<MMCGUILabel>();
		versionTextLabel.text = GUILayout.TextField(versionTextLabel.text, 13, EditorStyles.textField, GUILayout.MinHeight(20), GUILayout.MinWidth(60));
		MMCGUILabel versionLauncherTextLabel = GameObject.Find("GUI_Version_Launcher").GetComponent<MMCGUILabel>();
		versionLauncherTextLabel.text = versionTextLabel.text;
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
		EditorUtility.SetDirty(versionTextLabel);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Detalles del Projecto:");
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(40);
		MMCGUIAction_HelpScreen projDetails = GameObject.Find("GUI_Help_h").GetComponent<MMCGUIAction_HelpScreen>();
		projDetails.detallesProjecto.text = EditorGUILayout.TextArea(projDetails.detallesProjecto.text, EditorStyles.textField, GUILayout.MinHeight(20), GUILayout.MinWidth(60));
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
		EditorUtility.SetDirty(projDetails.detallesProjecto);
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Mostrar Pantalla de Carga");
		ROCHEScript rScript = GameObject.Find("_ROCHE").GetComponent<ROCHEScript>();
		rScript.showLoading = EditorGUILayout.Toggle(rScript.showLoading, EditorStyles.toggle, GUILayout.MinHeight(20), GUILayout.MinWidth(60));
		GUILayout.Space (20);
		EditorGUILayout.EndHorizontal();
		EditorUtility.SetDirty(rScript);
		GUILayout.EndVertical ();

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.LabelField ("Exportar aplicación", EditorStyles.boldLabel);
		EditorGUILayout.HelpBox ("Elige el tipo de exportado.", MessageType.Info);
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Escritorio", GUILayout.Width(80)))
		{
			SetDefaultExportParams();
			GameObject r = GameObject.Find("_ROCHE") as GameObject;
			rScript.buildType = ROCHEScript.BuildType.Desktop;
			EditorUtility.SetDirty(rScript);

			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1]; 
			System.Array.Copy(scenes, newSettings, 0);
			EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(EditorApplication.currentScene, true); 
			newSettings[0] = sceneToAdd;
			EditorBuildSettings.scenes = newSettings;
			
			string[] scenesToAdd = new string[1];
			scenesToAdd[0] = EditorApplication.currentScene;
			
			string exportPath = EditorUtility.SaveFolderPanel("Escoge la carpeta de destino", "", "");
			
			string finalPath = exportPath + "/ROCHE.exe";

			Debug.Log("Exporting to: " + finalPath);
			BuildPipeline.BuildPlayer(scenesToAdd,finalPath ,BuildTarget.StandaloneWindows,BuildOptions.ShowBuiltPlayer);
		}
		//GUILayout.FlexibleSpace();
		//EditorGUILayout.EndHorizontal ();

		//GUILayout.BeginHorizontal ();
		//GUILayout.FlexibleSpace();
		if (GUILayout.Button("Showroom", GUILayout.Width(80)))
		{
			SetDefaultExportParams();
			GameObject r = GameObject.Find("_ROCHE") as GameObject;
			rScript.buildType = ROCHEScript.BuildType.Kinect;
			EditorUtility.SetDirty(rScript);

			string kinectPath = Application.dataPath + "Kinect/";
			kinectPath = kinectPath.Replace("Assets",""); 
			Debug.Log("Kinect Path = " + kinectPath);
			string exportPath = EditorUtility.SaveFolderPanel("Escoge la carpeta de destino", "", "");
			
			string finalPath = exportPath + "/ROCHE_Showroom.exe";
			
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1]; 
			System.Array.Copy(scenes, newSettings, 0);
			EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(EditorApplication.currentScene, true); 
			newSettings[0] = sceneToAdd;
			EditorBuildSettings.scenes = newSettings;
			
			string[] scenesToAdd = new string[1];
			scenesToAdd[0] = EditorApplication.currentScene;

			Debug.Log("Exporting to: " + finalPath);
			BuildPipeline.BuildPlayer(scenesToAdd,finalPath,BuildTarget.StandaloneWindows,BuildOptions.ShowBuiltPlayer);

			FileUtil.CopyFileOrDirectory(kinectPath, exportPath + "/ROCHE_Showroom" + "_Data/Kinect");
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
        if (GUILayout.Button("Monotouch", GUILayout.Width(80)))
        {
			SetDefaultExportParams();
            GameObject r = GameObject.Find("_ROCHE") as GameObject;
            rScript.buildType = ROCHEScript.BuildType.Monotouch;
            EditorUtility.SetDirty(rScript);

            EditorApplication.SaveCurrentSceneIfUserWantsTo();

            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1];
            System.Array.Copy(scenes, newSettings, 0);
            EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(EditorApplication.currentScene, true);
            newSettings[0] = sceneToAdd;
            EditorBuildSettings.scenes = newSettings;

            string[] scenesToAdd = new string[1];
            scenesToAdd[0] = EditorApplication.currentScene;

            string exportPath = EditorUtility.SaveFolderPanel("Escoge la carpeta de destino", "", "");

            string finalPath = exportPath + "/ROCHE_Monotouch.exe";

            Debug.Log("Exporting to: " + finalPath);
            BuildPipeline.BuildPlayer(scenesToAdd, finalPath, BuildTarget.StandaloneWindows, BuildOptions.ShowBuiltPlayer);
        }
        //GUILayout.FlexibleSpace();
        //EditorGUILayout.EndHorizontal();

		//GUILayout.BeginHorizontal ();
		//GUILayout.FlexibleSpace();
		if (GUILayout.Button("Oculus Rift", GUILayout.Width(80)))
		{
			SetDefaultExportParams();
			PlayerSettings.defaultIsFullScreen = true;
			//PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;
			PlayerSettings.defaultScreenWidth = 1280;
			PlayerSettings.defaultScreenHeight = 800;

			GameObject r = GameObject.Find("_ROCHE") as GameObject;
			rScript.buildType = ROCHEScript.BuildType.OVR;
			EditorUtility.SetDirty(rScript);
			
			EditorApplication.SaveCurrentSceneIfUserWantsTo();
			
			EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
			EditorBuildSettingsScene[] newSettings = new EditorBuildSettingsScene[1]; 
			System.Array.Copy(scenes, newSettings, 0);
			EditorBuildSettingsScene sceneToAdd = new EditorBuildSettingsScene(EditorApplication.currentScene, true); 
			newSettings[0] = sceneToAdd;
			EditorBuildSettings.scenes = newSettings;
			
			string[] scenesToAdd = new string[1];
			scenesToAdd[0] = EditorApplication.currentScene;
			
			string exportPath = EditorUtility.SaveFolderPanel("Escoge la carpeta de destino", "", "");
			
			string finalPath = exportPath + "/ROCHE_OVR.exe";
			
			Debug.Log("Exporting to: " + finalPath);
			BuildPipeline.BuildPlayer(scenesToAdd,finalPath ,BuildTarget.StandaloneWindows,BuildOptions.ShowBuiltPlayer);
		}
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.EndVertical();


		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}

	#endregion

	#region Info Tab

	private void TabInfo()
	{
		// Vertical Scrollbar
		scrollPos = EditorGUILayout.BeginScrollView (scrollPos);
		
		// ROCHE Header Icon
		DrawRocheIconBox();		

		GUILayout.BeginVertical();

		// Nº objetos
		GUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Información de escena.", EditorStyles.boldLabel);
		#pragma warning disable 0219
		int numEdif = sceneBuilding.transform.childCount;
		int numMobi = furnitureContainer.transform.childCount;
		int numLuces = 0;
		GetNumGameObjects(lightContainer,ref numLuces);

		int numSelected = 0;
		foreach (Transform g in Selection.transforms)
		{
			if (g.gameObject.layer == LayerMask.NameToLayer("ROCHE")) continue;
			numSelected++;
		}
		int numTotal = numEdif + numMobi + numLuces;		
		#pragma warning restore 0219
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Nº total de objetos:");
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField (numTotal.ToString(), EditorStyles.label, GUILayout.MinWidth(30));
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Nº de objetos de Edificio:");
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField (numEdif.ToString(), EditorStyles.label, GUILayout.MinWidth(30));
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Nº de objetos de Mobiliario:");
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField (numMobi.ToString(), EditorStyles.label, GUILayout.MinWidth(30));
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Nº de luces:");
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField (numLuces.ToString(), EditorStyles.label, GUILayout.MinWidth(30));
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		EditorGUILayout.BeginHorizontal();
		GUILayout.Space(20);
		EditorGUILayout.LabelField ("Nº de objetos seleccionados:");
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField (numSelected.ToString(), EditorStyles.label, GUILayout.MinWidth(30));
		GUILayout.Space(20);
		EditorGUILayout.EndHorizontal ();
		
		GUILayout.Space(5);
				
		GUILayout.EndVertical();

		GUILayout.BeginVertical("Box");
		EditorGUILayout.LabelField("Parámetros de Interfaz y Control", EditorStyles.boldLabel);
		if (GameObject.Find ("_ROCHE") == null)
		{
			EditorGUILayout.HelpBox ("Para poder editar los parámetros debe activar la fase 'App'.", MessageType.Warning);
		}
		else
		{
			GameObject camS = GameObject.Find("CameraSystem") as GameObject;
			CameraOrbit orbit = camS.GetComponent<CameraOrbit>();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor vuelo Orbit:");
			GUILayout.FlexibleSpace();
			orbit.spanSpeed = Int32.Parse( EditorGUILayout.TextField (orbit.spanSpeed.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor giro eje X Orbit:");
			GUILayout.FlexibleSpace();
			orbit.xSpeed = Int32.Parse(EditorGUILayout.TextField (orbit.xSpeed.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor giro eje Y Orbit:");
			GUILayout.FlexibleSpace();
			orbit.ySpeed = Int32.Parse(EditorGUILayout.TextField (orbit.ySpeed.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor zoom Orbit:");
			GUILayout.FlexibleSpace();
			orbit.speedZoom = Int32.Parse(EditorGUILayout.TextField (orbit.speedZoom.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorUtility.SetDirty(orbit);
			
			MouseLook mLook = camS.GetComponent<MouseLook>();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor Giro X Walk:");
			GUILayout.FlexibleSpace();
			mLook.sensitivityX = Int32.Parse(EditorGUILayout.TextField (mLook.sensitivityX.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(20);
			EditorGUILayout.LabelField ("Factor Giro Y Walk:");
			GUILayout.FlexibleSpace();
			mLook.sensitivityY = Int32.Parse(EditorGUILayout.TextField (mLook.sensitivityY.ToString(), EditorStyles.numberField, GUILayout.MinWidth(30)));
			GUILayout.Space(20);
			EditorGUILayout.EndHorizontal ();
			EditorUtility.SetDirty(mLook);
			
			GUILayout.Space(5);
					

		}
		GUILayout.EndVertical();

		EditorGUILayout.BeginVertical ("Box");
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("Para cualquier incidencia técnica enviar un e-mail a:", EditorStyles.wordWrappedLabel);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		EditorGUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField("info@mobilemediacontent.com", EditorStyles.boldLabel);
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();

		GUILayout.EndVertical();

		GUILayout.EndVertical();
		EditorGUILayout.EndScrollView();
	}

	#endregion
	
	/*
	 * HELPER METHODS
	 */
		
	// Create a single light
	private void CreateLight(Vector3 p)
	{
		GameObject light = GameObject.Instantiate((GameObject)Resources.Load ("ROCHE/Prefabs/YellowLight"),p,Quaternion.identity) as GameObject;
		light.name = "Light";
		SetLayerRecursively(light,"ROCHE_LUZ");
		light.transform.parent = lightContainer.transform;
	}
	
	// Create a single blue light
	private void CreateLightBlue(Vector3 p)
	{
		GameObject light = GameObject.Instantiate((GameObject)Resources.Load ("ROCHE/Prefabs/BlueLight"),p,Quaternion.identity) as GameObject;
		light.name = "Light";
		SetLayerRecursively(light,"ROCHE_LUZ");
		light.transform.parent = lightContainer.transform;
	}
	
	// Get num of gameObjects without children inside this gameObject
	private void GetNumGameObjects(GameObject g, ref int x)
	{
		if (g.transform.childCount == 0) x++;
		else foreach (Transform child in g.transform) GetNumGameObjects(child.gameObject, ref x);
	}
	
	// Set Static tag recursively on all children
	private void SetStaticRecursively(GameObject iGameObject)
	{
		iGameObject.isStatic = true;
		foreach (Transform childT in iGameObject.transform)
		{
			SetStaticRecursively(childT.gameObject);
		}
	}
	
	// Helper function to draw ROCHE header icon on a ROCHE Inspector tab
	private void DrawRocheIconBox()
	{
		EditorGUILayout.BeginVertical ("Box");
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace();
		GUILayout.Label (rocheIcon, GUILayout.MaxWidth (224), GUILayout.MaxHeight(92));
		GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
	}
	
	// Helper function to set the layer of a GameObject and all its children GameObjects
	private void SetLayerRecursively (GameObject iGameObject, string layer)
	{
		iGameObject.layer = LayerMask.NameToLayer(layer);
		foreach (Transform childT in iGameObject.transform)
		{
			SetLayerRecursively(childT.gameObject,layer);
		}
	}
	
	// Helper function to set the tag of a GameObject and all its children GameObjects
	private void SetTagRecursively (GameObject iGameObject, string tag)
	{
		iGameObject.tag = tag;
		foreach (Transform childT in iGameObject.transform)
		{
			SetTagRecursively(childT.gameObject,tag);
		}
	}
	
	// Helper function to set add a MeshCollider to a GameObject and all its children GameObjects
	private void SetCollisionsRecursively (GameObject iGameObject)
	{
		if (iGameObject.renderer && !iGameObject.collider) iGameObject.AddComponent<MeshCollider>();
		foreach (Transform childT in iGameObject.transform)
		{
			SetCollisionsRecursively(childT.gameObject);
		}
	}
				
	private void RemoveCollisionsRecursively (GameObject iGameObject)
	{
		if (iGameObject.collider) GameObject.DestroyImmediate(iGameObject.collider);
		foreach (Transform childT in iGameObject.transform)
		{
			RemoveCollisionsRecursively(childT.gameObject);
		}
	}
	
	private Transform GetParentBuildingTransform(Transform t)
	{
		if (t.parent != null && t.parent.gameObject.layer == LayerMask.NameToLayer("ROCHE")) return t;
		if (t.parent != null && t.parent.gameObject.layer == LayerMask.NameToLayer("ROCHE_EDIFICIO")) return GetParentBuildingTransform(t.parent);
		else return t;
	}
	
	private Transform GetParentFurnitureTransform(Transform t)
	{
		if (t.parent != null && t.parent.gameObject.layer == LayerMask.NameToLayer("ROCHE")) return t;
		if (t.parent != null && t.parent.gameObject.layer == LayerMask.NameToLayer("ROCHE_MOBILIARIO")) return GetParentBuildingTransform(t.parent);
		else return t;
	}
		
	// Helper function to align a Group of GameObjects on 1 axis.
	// Aligns them taking the first selected object as reference for position
	private void AlignGameObjects (int axis)
	{
		if (Selection.transforms.Length < 2) {
			EditorUtility.DisplayDialog ("Error", "Se requieren al menos dos objetos para realizar una alineación.", "Aceptar");
			return;
		}

		Vector3 active = Selection.activeTransform.position;
		switch (axis) {
		case 0:
			foreach (Transform t in Selection.transforms) {
				Undo.RecordObject(t,t.name);
				t.position = new Vector3 (active.x, t.position.y, t.position.z);
			}
			break;
		case 1:
			foreach (Transform t in Selection.transforms) {
				Undo.RecordObject(t,t.name);
				t.position = new Vector3 (t.position.x, active.y, t.position.z);
			}
			break;
		case 2:
			foreach (Transform t in Selection.transforms) {
				Undo.RecordObject(t,t.name);
				t.position = new Vector3 (t.position.x, t.position.y, active.z);
			}
			break;
		}
	}
	
	// Helper function to view object collisions with other objects
	private void ViewObjectCollisions()
	{
		// Disable view collisions if we are not at the Scene Inspector Tab
		if (toolbarPos != TOOLBAR_POS.APP) viewCollisions = false;
		
		// Draw collisions for current GameObject
		// Get active object
		GameObject currentActiveObject = Selection.activeGameObject;
		
		// Restore renderer status for last object and clean
		if (currentActiveObject != lastSelectedObject || !viewCollisions)
		{
			if (lastSelectedObject != null)
			{
			    Renderer[] childRenderers = lastSelectedObject.GetComponentsInChildren<Renderer> ();
			    foreach (Renderer r in childRenderers)
			    {
			        r.enabled = true;
			    }
				lastSelectedObject = null;
			}
		}
		
		// If we don't want to view collisions just return, as everything has already been cleaned
		if (!viewCollisions) return;
		// No valid object? return
		if (currentActiveObject == null) return;
		
		// Check if we have a new valid active object
		if (currentActiveObject != lastSelectedObject) 
		{
			// If selected object is a ROCHE object then return
			if (currentActiveObject.layer == LayerMask.NameToLayer("ROCHE")) return;
			// If selected object is a ROCHE_EDIFICIO object then return
			if (currentActiveObject.layer == LayerMask.NameToLayer("ROCHE_EDIFICIO")) return;
			
			// Object valid, set new lastObject & calculate bounds
			lastSelectedObject = currentActiveObject;
		}
				
		// Check against the other objects
		bool isColliding = false;
		foreach (GameObject g in GameObject.FindObjectsOfType(typeof(GameObject))) 
		{
			if (g == lastSelectedObject) continue;
			if (g.layer == LayerMask.NameToLayer("ROCHE")) continue;
			if (g.layer == LayerMask.NameToLayer("ROCHE_EDIFICIO")) continue;
			if (g.collider == null) continue;
			if (g.renderer != null) 
			{
				bool isValid = true;
				// Check childs
				foreach (Transform child in lastSelectedObject.transform)
				{
					if (child == g.transform) 
					{
						isValid = false;
						break;
					}
				}
				if (!isValid) continue;
				// Check parents
				foreach (Transform child in g.transform)
				{
					if (child == lastSelectedObject.transform) 
					{
						isValid = false;
						break;
					}
				}
				if (!isValid) continue;
				
				// Object valid, check intersection
			    Renderer[] currentChildRenderers = lastSelectedObject.GetComponentsInChildren<Renderer> ();
			    foreach (Renderer r in currentChildRenderers)
			    {
			        if (r.bounds.Intersects(g.renderer.bounds))
					{
						isColliding = true;
						break;
					}
			    }
				if (isColliding) break;
			}			
		}
		
		if (!isColliding)
		{
			Bounds b = currentActiveObject.renderer.bounds;
			if (Physics.Raycast(new Ray(b.center,Vector3.down),b.size.y/2 - 0.01f,1<<9))
				isColliding = true;
			else if (Physics.Raycast(new Ray(b.center,Vector3.up),b.size.y/2 - 0.01f,1<<9))
				isColliding = true;
			else if (Physics.Raycast(new Ray(b.center,Vector3.left),b.size.x/2 - 0.01f,1<<9))
				isColliding = true;
			else if (Physics.Raycast(new Ray(b.center,Vector3.right),b.size.x/2 - 0.01f,1<<9))
				isColliding = true;
			else if (Physics.Raycast(new Ray(b.center,Vector3.forward),b.size.z/2 - 0.01f,1<<9))
				isColliding = true;
			else if (Physics.Raycast(new Ray(b.center,Vector3.back),b.size.z/2 - 0.01f,1<<9))
				isColliding = true;
		}
		
		// Disable renderers
		if (isColliding) 
		{
			Renderer[] currentChildRenderers = lastSelectedObject.GetComponentsInChildren<Renderer> ();
		    foreach (Renderer r in currentChildRenderers) r.enabled = false;
		}
		// Enable renderers
		else 
		{
			Renderer[] currentChildRenderers = lastSelectedObject.GetComponentsInChildren<Renderer> ();
		    foreach (Renderer r in currentChildRenderers) if (r.gameObject.collider != null) r.enabled = true;
		}
		
	}
	
	// Helper function to calculate the main Building bounds
	private void CalculateBuildingBounds()
	{
	    sceneBuildingBounds = new Bounds (sceneBuilding.transform.position, Vector3.zero);
	    Renderer[] renderers = sceneBuilding.GetComponentsInChildren<Renderer> ();
	    foreach (Renderer renderer in renderers)
	    {
	        sceneBuildingBounds.Encapsulate (renderer.bounds);
	    }
	}
	
	// Place a transform on the Building floor
	private void PlaceOnFloor(Transform t, bool centerOnBuilding)
	{
		Undo.RecordObject(t,t.name);
		// No building, just place on 0 0 0
		if (sceneBuilding == null)
		{
			t.position = new Vector3(0,0,0);
			return;
		}

		// Get new object real bounds using children renderers
		Bounds newObjectBounds = new Bounds(t.position, Vector3.zero);
		Renderer[] renderers2 = t.GetComponentsInChildren<Renderer> ();
	    foreach (Renderer renderer in renderers2)
	    {
	        newObjectBounds.Encapsulate (renderer.bounds);
	    }
		
		// Put object at building center but without colliding with floor (use object height)
		if (centerOnBuilding) t.position = new Vector3(sceneBuildingBounds.center.x, t.position.y - (newObjectBounds.min.y - sceneBuildingBounds.min.y), sceneBuildingBounds.center.z);
		else t.position = new Vector3(t.position.x, t.position.y - (newObjectBounds.min.y - sceneBuildingBounds.min.y), t.position.z);
	}
	
	// Generate lights automatically
	private void GenerateLights(string lightPrefabPath)
	{
		// Destroy current lights
		foreach (GameObject g in GameObject.FindObjectsOfType(typeof(GameObject))) if (g.layer == LayerMask.NameToLayer("ROCHE_LUZ"))
				DestroyImmediate (g);
		
		CalculateBuildingBounds();
		
		// Get desired light prefab
		GameObject lightPrefab = (GameObject)Resources.Load (lightPrefabPath);
		
		// Minimum distance to "walls"
		float distToWall = 1.0f;
		
		// Light Height
		float lightHeight = sceneBuildingBounds.center.y;
		
		// We make 2 bucles to position lights. We use raycasting to check the size of each room and that way
		// we can know how many lights should we put and where should we put them.
		// It still doesn't work well for the Z axis, but can be improved with some time and effort.
		float posZ = sceneBuildingBounds.min.z + distToWall;
		while (posZ <= sceneBuildingBounds.max.z - distToWall)
		{
			float posX = sceneBuildingBounds.min.x + distToWall;
			while (posX <= sceneBuildingBounds.max.x - distToWall)
			{			
				RaycastHit hit;
				bool isHitLeft = Physics.Raycast(new Vector3(posX,lightHeight,posZ),Vector3.left,out hit);
				if (isHitLeft == false) 
				{
					
					posX += lightDistance;
					continue;
				}
				float leftPos = hit.point.x;
				
				bool isHitRight = Physics.Raycast(new Vector3(posX,lightHeight,posZ),Vector3.right,out hit);
				if (isHitRight == false) 
				{
					posX += lightDistance;
					continue;
				}
				float rightPos = hit.point.x;
				
				bool isHitBack = Physics.Raycast(new Vector3(posX,lightHeight,posZ),Vector3.back,out hit);
				if (isHitBack == false) 
				{
					posX += lightDistance;
					continue;
				}
				float backPos = hit.point.z;
				
				bool isHitFront = Physics.Raycast(new Vector3(posX,lightHeight,posZ),Vector3.forward,out hit);
				if (isHitFront == false) 
				{
					posX += lightDistance;
					continue;
				}
				float frontPos = hit.point.z;
				
				float roomWidth = rightPos - leftPos;
				float roomCenterWidth = leftPos + roomWidth/2;
				
				//float roomDepth = frontPos-backPos;
				//float roomCenterDepth = backPos + roomDepth/2;
				float currentPosZ = posZ;
				if (currentPosZ - backPos < distToWall) currentPosZ = backPos+distToWall;
				if (frontPos - currentPosZ < distToWall) currentPosZ = frontPos-distToWall;
				
				float numLights = (roomWidth - distToWall*2.0f)/lightDistance;
				int numLightsFloored = (int) numLights;
				if (numLights > 0.0f && numLights < 1.0f) 
				{
					GameObject light = GameObject.Instantiate(lightPrefab) as GameObject;
					light.transform.position = new Vector3 (roomCenterWidth, sceneBuildingBounds.center.y, currentPosZ);
					light.name = "YellowLight";
					SetLayerRecursively(light,"ROCHE_LUZ");
					light.transform.parent = lightContainer.transform;
				}
				else if (numLightsFloored%2 == 0)
				{
					for (float pX = roomCenterWidth - lightDistance/2; pX >= leftPos + distToWall; pX -= lightDistance)
					{
						GameObject light = GameObject.Instantiate(lightPrefab) as GameObject;
						light.transform.position = new Vector3 (pX, sceneBuildingBounds.center.y, currentPosZ);
						light.name = "YellowLight";
						SetLayerRecursively(light,"ROCHE_LUZ");
						light.transform.parent = lightContainer.transform;
					}
					for (float pX = roomCenterWidth + lightDistance/2; pX <= rightPos - distToWall; pX += lightDistance)
					{
						GameObject light = GameObject.Instantiate(lightPrefab) as GameObject;
						light.transform.position = new Vector3 (pX, sceneBuildingBounds.center.y, currentPosZ);
						light.name = "YellowLight";
						SetLayerRecursively(light,"ROCHE_LUZ");
						light.transform.parent = lightContainer.transform;
					}
				}
				else if (numLightsFloored%2 != 0)
				{
					GameObject lightCenter = GameObject.Instantiate(lightPrefab) as GameObject;
					lightCenter.transform.position = new Vector3 (roomCenterWidth, sceneBuildingBounds.center.y, currentPosZ);
					lightCenter.name = "YellowLight";
					SetLayerRecursively(lightCenter,"ROCHE_LUZ");
					lightCenter.transform.parent = lightContainer.transform;
					
					for (float pX = roomCenterWidth - lightDistance; pX >= leftPos + distToWall; pX -= lightDistance)
					{
						GameObject light = GameObject.Instantiate(lightPrefab) as GameObject;
						light.transform.position = new Vector3 (pX, sceneBuildingBounds.center.y, currentPosZ);
						light.name = "YellowLight";
						SetLayerRecursively(light,"ROCHE_LUZ");
						light.transform.parent = lightContainer.transform;
					}
					for (float pX = roomCenterWidth + lightDistance; pX <= rightPos - distToWall; pX += lightDistance)
					{
						GameObject light = GameObject.Instantiate(lightPrefab) as GameObject;
						light.transform.position = new Vector3 (pX, sceneBuildingBounds.center.y, currentPosZ);
						light.name = "YellowLight";
						SetLayerRecursively(light,"ROCHE_LUZ");
						light.transform.parent = lightContainer.transform;
					}
				}
				
				posX = rightPos + distToWall;
			}
			posZ += lightDistance;
		}
	
	}
	
	private void CenterCameraOnGameObject(GameObject g)
	{
		if (g == null) return;
		Selection.activeGameObject = g;
		if (SceneView.lastActiveSceneView != null)
			SceneView.lastActiveSceneView.FrameSelected();
	}

	private void SetDefaultExportParams()
	{
		PlayerSettings.productName = "ROCHE";
		PlayerSettings.defaultIsFullScreen = false;
		PlayerSettings.runInBackground = true;
		PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
		QualitySettings.SetQualityLevel(5);

	}
}