#pragma strict

@script ExecuteInEditMode()

//PUBLIC VARIABLES
var saveAsset : boolean = false;
var useName : String = "";

//PRIVATE VARIABLES
private var mesh : Mesh = new Mesh();



function Start () {

}


function LateUpdate () {
#if UNITY_EDITOR
	if (saveAsset && useName != ""){
		saveAsset = false;
		SaveAsset();
	}
#endif
}


function SaveAsset () {
#if UNITY_EDITOR
	mesh = new Mesh();
	mesh = GetComponent(MeshFilter).sharedMesh;
	mesh.name = useName;
	mesh.RecalculateNormals();
	mesh.Optimize();

	if (mesh != null){ 
		AssetDatabase.CreateAsset(mesh, "Assets/SUIMONO - WATER SYSTEM 2/MESH/"+useName+".asset");
		Debug.Log("Asset Created at: "+AssetDatabase.GetAssetPath(mesh)+"!");
	}

#endif
}