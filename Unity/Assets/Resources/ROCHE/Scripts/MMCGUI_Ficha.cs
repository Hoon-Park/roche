using UnityEngine;
using System.Collections;

public class MMCGUI_Ficha : MonoBehaviour {
	public MMCGUILabel nombre;
	public MMCGUILabel area;
	public MMCGUILabel descripcion;
	public MMCGUILabel propiedades;
	public MMCGUITexture imagen;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	public void ShowTag(GameObject g)
	{
		Ficha ficha = g.GetComponent<Ficha>();
		if (ficha.activar == false) return;
		nombre.text = ficha.nombre;
		area.text = ficha.area;
		descripcion.text = ficha.descripcion;
		propiedades.text = ficha.propiedades;
		imagen.texture = ficha.imagen;		
	}
}
