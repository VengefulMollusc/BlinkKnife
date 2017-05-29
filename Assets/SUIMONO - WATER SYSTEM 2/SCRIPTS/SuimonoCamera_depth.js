@script ExecuteInEditMode()
#pragma strict


//PUBLIC VARIABLES
@HideInInspector var _sceneDepth : float = 20.0;
@HideInInspector var _shoreDepth : float = 45.0;

//PRIVATE VARIABLES
private var useMat : Material;

function Start () {
	
	//setup material
	useMat = new Material (Shader.Find("Suimono2/SuimonoDepth"));
}




function LateUpdate () {
	
	//clamp values
	_sceneDepth = Mathf.Clamp(_sceneDepth,0.0,100.0);
	_shoreDepth = Mathf.Clamp(_shoreDepth,0.0,100.0);

	//set material properties
	useMat.SetFloat("_sceneDepth", _sceneDepth);
	useMat.SetFloat("_shoreDepth", _shoreDepth);

}
	

function OnRenderImage (source : RenderTexture, destination : RenderTexture){
	Graphics.Blit(source,destination,useMat);
}


