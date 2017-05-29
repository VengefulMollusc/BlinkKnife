#pragma strict


//PUBLIC VARIABLES
var useShader : Shader;

//PRIVATE VARIABLES
private var useMat : Material;
private var CamInfo : Camera;



function Start () {

	//setup material
	useMat = new Material (useShader);

	//setup camera
	CamInfo = this.GetComponent(Camera);
}



function OnRenderImage (source : RenderTexture, destination : RenderTexture){
	Graphics.Blit(source,destination,useMat);
}


