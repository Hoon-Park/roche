using UnityEngine;
using UnityEditor;
using System.Collections;

public class ExportRocheBase : MonoBehaviour {

	// Add a menu item named "Do Something" to MyMenu in the menu bar.
	[MenuItem ("ROCHE/Export RDK Base")]
	static void DoSomething () {
		AssetDatabase.ExportPackage("Assets", "RDK_Base.unitypackage", ExportPackageOptions.Interactive | ExportPackageOptions.Recurse | ExportPackageOptions.IncludeLibraryAssets|ExportPackageOptions.IncludeDependencies );
	}
}
