#pragma strict
@script AddComponentMenu ("Image Effects/Suimono/UnderwaterFX")


var showScreenMask : boolean = false;
var doTransition : boolean = false;
var cancelTransition : boolean = false;
var useUnderSurfaceView : boolean = false;

var distanceFog : boolean = true;
var useRadialDistance : boolean = true;
var heightFog : boolean = false;
var height : float = 1.0f;


var heightDensity : float = 2.0f;
var startDistance : float = 0.0f;
var fogStart : float = 0.0f;
var fogEnd : float = 20.0f;
var refractAmt : float = 0.005f;
var refractSpd : float = 1.5f;
var refractScale : float = 0.5f;
var lightFactor : float = 1.0;
var underwaterColor : Color;

var dropsTime : float = 2.0;
var wipeTime : float = 1.0;

var iterations : int = 2;
var blurSpread : float = 1.0;
var darkRange : float = 40.0;
var heightDepth : float = 1.0;
var hFac : float = 0.0;

//var causticTestTex : Texture2D;
//var causticCoordinates : Vector4 = Vector4(1,1,0,0);

var distortTex : Texture;
var mask1Tex : Texture;
var mask2Tex : Texture;
var fogShader : Shader = null;
var fogMaterial : Material = null;

private var moduleObject : SuimonoModule;
private var moduleLibrary : SuimonoModuleLib;
private var trans1Time : float = 1.1;
private var trans2Time : float = 1.1;
private var dropRand : Random;
private var dropOff : Vector2;

private var iteration : int = 1;
private var cam : Camera;
private var camtr : Transform;

private var pass : int;
private var rtW : int;
private var rtH : int;
private var buffer : RenderTexture;
private var i : int = 0;
private var buffer2 : RenderTexture;

private var camPos : Vector3;
private var FdotC : float;
private var paramK : float;
private var sceneStart : float;
private var sceneEnd : float;
private var sceneParams : Vector4;
private var diff : float;
private var invDiff : float;

private var frustumCorners : Matrix4x4;
private var fovWHalf : float;
private var toRight : Vector3;
private var toTop : Vector3;
private var topLeft : Vector3;
private var camScale : float;
private var topRight : Vector3;
private var bottomRight : Vector3;
private var bottomLeft : Vector3;

private var offc : float;
private var off : float;




function Start () {

	cam = gameObject.GetComponent(Camera);
	camtr = cam.transform;

	if (GameObject.Find("SUIMONO_Module") != null){
		moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule);
	    moduleLibrary = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModuleLib);
	}
	
    if (moduleLibrary != null){
    	distortTex = moduleLibrary.texNormalC;
    	mask1Tex = moduleLibrary.texHeightC;
    	mask2Tex = moduleLibrary.texDrops;
	}

	dropRand = new Random();

    fogShader = Shader.Find("Hidden/SuimonoUnderwaterFog");
    fogMaterial = new Material(fogShader);
}






function LateUpdate () {

	if (cancelTransition){
		doTransition = false;
		cancelTransition = false;
		trans1Time = 1.1;
		trans2Time = 1.1;
	}

	if (doTransition){
		doTransition = false;
		trans1Time = 0.0;
		trans2Time = 0.0;
		dropOff.x = dropRand.Range(0.0,1.0);
		dropOff.y = dropRand.Range(0.0,1.0);
	}

	trans1Time += (Time.deltaTime*0.7*wipeTime);
	trans2Time += (Time.deltaTime*0.1*dropsTime);



	//Caustics
    //if (causticTestTex != null &&  fogMaterial != null){
        //fogMaterial.SetTexture("_CausticTex", causticTestTex);
        //fogMaterial.SetVector("_fCoord",causticTilt);
        //if (useLight != null){
            //lC = useLight.transform.forward;
            //lC.y = lC.y * -1.0f;
            //lC.x = lC.x * -1.0f;
            //_material.SetVector("_lCoord", lC); 
            //_material.SetVector("_lColor",useLight.color * useLight.intensity);
        //}
        //if (cam != null && fogMaterial != null){
        //	var cMatrix = gameObject.GetComponent(Camera).cameraToWorldMatrix;
        //	fogMaterial.SetMatrix("_InverseMatrix", cMatrix);
		//	fogMaterial.SetVector("_CausCoord", causticCoordinates);
        //}
    //}
    

}



@ImageEffectOpaque
function OnRenderImage (source : RenderTexture, destination : RenderTexture){

	Graphics.Blit (source, destination);



	frustumCorners = Matrix4x4.identity;
	fovWHalf = cam.fieldOfView * 0.5f;
	toRight = camtr.right * cam.nearClipPlane * Mathf.Tan (fovWHalf * Mathf.Deg2Rad) * cam.aspect;
	toTop = camtr.up * cam.nearClipPlane * Mathf.Tan (fovWHalf * Mathf.Deg2Rad);
	topLeft = (camtr.forward * cam.nearClipPlane - toRight + toTop);
	camScale = topLeft.magnitude * cam.farClipPlane/cam.nearClipPlane;

	topLeft.Normalize();
	topLeft *= camScale;

	topRight = (camtr.forward * cam.nearClipPlane + toRight + toTop);
	topRight.Normalize();
	topRight *= camScale;

	bottomRight = (camtr.forward * cam.nearClipPlane + toRight - toTop);
	bottomRight.Normalize();
	bottomRight *= camScale;

	bottomLeft = (camtr.forward * cam.nearClipPlane - toRight - toTop);
	bottomLeft.Normalize();
	bottomLeft *= camScale;

	frustumCorners.SetRow (0, topLeft);
	frustumCorners.SetRow (1, topRight);
	frustumCorners.SetRow (2, bottomRight);
	frustumCorners.SetRow (3, bottomLeft);


	//set default values based on water surface height
	if (heightFog && this.transform.parent != null){
		height = this.transform.parent.transform.position.y + 1.0f;
		heightDensity = 2.0f;
	}

	camPos = camtr.position;
	FdotC = camPos.y-height;
	paramK = (FdotC <= 0.0f ? 1.0f : 0.0f);
	sceneStart = fogStart;
	sceneEnd = fogEnd;

	diff = sceneEnd - sceneStart;
	invDiff = Mathf.Abs(diff) > 0.0001f ? 1.0f / diff : 0.0f;
	sceneParams.x = 0.0f;
	sceneParams.y = 0.0f;
	sceneParams.z = -invDiff;
	sceneParams.w = sceneEnd * invDiff;


	if (fogMaterial != null){
		fogMaterial.SetMatrix ("_FrustumCornersWS", frustumCorners);
		fogMaterial.SetVector ("_CameraWS", camPos);
		fogMaterial.SetVector ("_HeightParams", new Vector4 (height, FdotC, paramK, heightDensity*0.5f));
		fogMaterial.SetVector ("_DistanceParams", new Vector4 (-Mathf.Max(startDistance,0.0f), 0, 0, 0));

		fogMaterial.SetVector ("_SceneFogParams", sceneParams);
		fogMaterial.SetVector ("_SceneFogMode", new Vector4(1, useRadialDistance ? 1 : 0, 0, 0));
		fogMaterial.SetColor ("_underwaterColor", underwaterColor);

		if (distortTex != null){
	    	fogMaterial.SetTexture("_underwaterDistort",distortTex);
	    	fogMaterial.SetFloat("_distortAmt",refractAmt);
	    	fogMaterial.SetFloat("_distortSpeed",refractSpd);
	    	fogMaterial.SetFloat("_distortScale",refractScale);
			fogMaterial.SetFloat("_lightFactor",lightFactor);
		}
		if (mask1Tex != null){
			fogMaterial.SetTexture("_distort1Mask",mask1Tex);
		}
		if (mask2Tex != null){
			fogMaterial.SetTexture("_distort2Mask",mask2Tex);
		}


		fogMaterial.SetFloat("_trans1",trans1Time);
		fogMaterial.SetFloat("_trans2",trans2Time);
		fogMaterial.SetFloat("_dropOffx",dropOff.x);
		fogMaterial.SetFloat("_dropOffy",dropOff.y);
		
		fogMaterial.SetFloat("_showScreenMask",showScreenMask ? 1:0);
		//fogMaterial.SetFloat("_Suimono_IsUnderwater",useUnderSurfaceView ? 1:0);

blurSpread = Mathf.Clamp01(blurSpread);
fogMaterial.SetFloat("_blur",blurSpread);


//calculate heightDepth for underwater darkening
if (moduleObject != null){
	hFac = Mathf.Clamp((11.5) - moduleObject.setTrack.transform.localPosition.y, 0.0,500.0);
	heightDepth = hFac;
	hFac = Mathf.Clamp01(Mathf.Lerp(-0.2,1,Mathf.Clamp01(hFac/darkRange)));
	fogMaterial.SetFloat("_hDepth", hFac);

	fogMaterial.SetFloat("_enableUnderwater",moduleObject.enableUnderwaterFX ? 1:0);
}


	//blur
    // Copy source to the 4x4 smaller texture.
    rtW = source.width/4;
    rtH = source.height/4;
    buffer = RenderTexture.GetTemporary(rtW, rtH, 0);
    DownSample4x (source, buffer);

    // Blur the small texture
    for(i = 0; i < iterations; i++){
        buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
        FourTapCone (buffer, buffer2, i);
        RenderTexture.ReleaseTemporary(buffer);
        buffer = buffer2;
    }
    Graphics.Blit(buffer, destination);
    RenderTexture.ReleaseTemporary(buffer);




		pass = 0;
		if (distanceFog && heightFog)
			pass = 0; // distance + height
		else if (distanceFog)
			pass = 1; // distance only
		else
			pass = 2; // height only




	CustomGraphicsBlit (source, destination, fogMaterial, pass);



	}

}



function CustomGraphicsBlit (source : RenderTexture, dest : RenderTexture, fxMaterial : Material, passNr : int){
	RenderTexture.active = dest;
	fxMaterial.SetTexture ("_MainTex", source);

	GL.PushMatrix ();
	GL.LoadOrtho ();
	fxMaterial.SetPass (passNr);

	GL.Begin (GL.QUADS);
	GL.MultiTexCoord2 (0, 0.0f, 0.0f);
	GL.Vertex3 (0.0f, 0.0f, 3.0f); // BL
	GL.MultiTexCoord2 (0, 1.0f, 0.0f);
	GL.Vertex3 (1.0f, 0.0f, 2.0f); // BR
	GL.MultiTexCoord2 (0, 1.0f, 1.0f);
	GL.Vertex3 (1.0f, 1.0f, 1.0f); // TR
	GL.MultiTexCoord2 (0, 0.0f, 1.0f);
	GL.Vertex3 (0.0f, 1.0f, 0.0f); // TL
	GL.End ();
	GL.PopMatrix ();
}





// Performs one blur iteration.
function FourTapCone (source : RenderTexture, dest : RenderTexture, iteration : int){
    offc = 0.5f + iteration*blurSpread*2;
    Graphics.BlitMultiTap (source, dest, fogMaterial,
                           new Vector2(-offc, -offc),
                           new Vector2(-offc,  offc),
                           new Vector2( offc,  offc),
                           new Vector2( offc, -offc)
        );
}

// Downsamples the texture to a quarter resolution.
function DownSample4x (source : RenderTexture, dest : RenderTexture){
   off = 1.0f;
    Graphics.BlitMultiTap (source, dest, fogMaterial,
                           new Vector2(-off, -off),
                           new Vector2(-off,  off),
                           new Vector2( off,  off),
                           new Vector2( off, -off)
        );
}