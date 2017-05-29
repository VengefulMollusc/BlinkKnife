
@script ExecuteInEditMode()

#pragma strict


//PUBLIC VARIABLES
var lodIndex : int;

var shorelineModeIndex : int;
var shorelineModeOptions = new Array("Auto-Generate","Custom Texture");

var shorelineRunIndex : int;
var shorelineRunOptions = new Array("At Start","Continuous");

var attachToSurface : Transform;
var sceneDepth : float = 14.5;
var shoreDepth : float = 27.7;
var debug : boolean = false;

var suimonoVersionNumber : String;

var depthLayer : int = 2;
var suiLayerMasks : Array;

var customDepthTex : Texture2D;

//PRIVATE VARIABLES
private var useMat : Material;
private var reflTex : Texture;
private var envTex : Texture;
private var MV : Matrix4x4;
private var CamInfo : Camera;
private var CamTools : cameraTools;
private var CamDepth : SuimonoCamera_depth;

private var curr_sceneDepth : float;
private var curr_shoreDepth : float;
private var curr_foamDepth : float;
private var curr_edgeDepth : float;

private var currPos : Vector3;
private var currScale : Vector3;
private var currRot : Quaternion;

private var camCoords : Vector4 = Vector4(1,1,0,0);
private var localMaterial : Material;
private var renderObject : Renderer;
private var meshObject : MeshFilter;
private var matObject : Material;
private var moduleObject : SuimonoModule;
private var maxScale : float;

private var i : int;
private var layerName : String;
private var hasRendered : boolean = false;
private var renderPass : boolean = true;
private var saveMode : int = -1;


function Start () {

	//DISCONNECT FROM PREFAB
	#if UNITY_EDITOR
	PrefabUtility.DisconnectPrefabInstance(this.gameObject);
	#endif
	
	//turn off debig at start
	if (Application.isPlaying){
		debug = false;
	}

	//get main object
	if (GameObject.Find("SUIMONO_Module") != null){
		moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule) as SuimonoModule;
		suimonoVersionNumber = moduleObject.suimonoVersionNumber;
	}

	//setup camera
	CamInfo = transform.Find("cam_LocalShore").gameObject.GetComponent(Camera);
	CamInfo.depthTextureMode = DepthTextureMode.DepthNormals;
	CamTools = transform.Find("cam_LocalShore").gameObject.GetComponent(cameraTools) as cameraTools;
	CamDepth = transform.Find("cam_LocalShore").gameObject.GetComponent(SuimonoCamera_depth) as SuimonoCamera_depth;

	//setup renderer
	renderObject = gameObject.GetComponent(Renderer) as Renderer;
	meshObject = gameObject.GetComponent(MeshFilter) as MeshFilter;

	//find parent surface
	if (transform.parent){
		if (transform.parent.gameObject.GetComponent(SuimonoObject) != null){
			attachToSurface = transform.parent;
		}
	}

	//turn on surface
	if (attachToSurface != null){
		attachToSurface.Find("Suimono_Object").gameObject.GetComponent(Renderer).enabled = true;
	}

	//setup material
	matObject = new Material (Shader.Find("Suimono2/Suimono2_FX_ShorelineObject"));
	renderObject.material = matObject;

	//set rendering flag
	hasRendered = false;
}







function LateUpdate () {
if (moduleObject != null){

	//version number
	suimonoVersionNumber = moduleObject.suimonoVersionNumber;

	//set layers and tags
	gameObject.layer = moduleObject.layerDepthNum;
	CamInfo.gameObject.layer = moduleObject.layerDepthNum;

	gameObject.tag = "Untagged";
	CamInfo.gameObject.tag = "Untagged";
	



//---------
	//set layer mask array
	suiLayerMasks = new Array();
	for (i = 0; i < 32; i++){
		layerName = LayerMask.LayerToName(i);
		suiLayerMasks.Add(layerName);
	}


	if (!Application.isPlaying){
		if (attachToSurface != null){
			if (debug){
				transform.position.y = attachToSurface.position.y;
				attachToSurface.Find("Suimono_Object").gameObject.GetComponent(Renderer).enabled = false;
			} else {
				attachToSurface.Find("Suimono_Object").gameObject.GetComponent(Renderer).enabled = true;
			}
		}
	}


	if (shorelineModeIndex == 0){
		// set camera culling
		if (CamInfo != null){
			CamInfo.enabled = true;
			CamInfo.cullingMask = depthLayer;
		}
	} else {
		if (CamInfo != null) CamInfo.enabled = false;
	}


	//Handle Debug Mode
	if (debug){ //} || !Application.isPlaying){
		if (renderObject != null) renderObject.hideFlags = HideFlags.None;
		if (meshObject != null) meshObject.hideFlags = HideFlags.None;
		if (matObject != null) matObject.hideFlags = HideFlags.None;
		if (shorelineModeIndex == 0){
			if (CamInfo != null) CamInfo.gameObject.hideFlags = HideFlags.None;
			if (CamTools != null){
				CamTools.runInEditMode = true;
				CamTools.CameraUpdate();
			}
		}
		if (renderObject != null) renderObject.enabled = true;
	} else {
		if (renderObject != null) renderObject.hideFlags = HideFlags.HideInInspector;
		if (meshObject != null) meshObject.hideFlags = HideFlags.HideInInspector;
		if (matObject != null) matObject.hideFlags = HideFlags.HideInInspector;
		if (shorelineModeIndex == 0){
			if (CamInfo != null) CamInfo.gameObject.hideFlags = HideFlags.HideInHierarchy;
			if (CamTools != null) CamTools.runInEditMode = false;
		}
		if (!Application.isPlaying && renderObject != null){
			renderObject.enabled = false;
		} else {
			renderObject.enabled = true;
		}
	}
	//---------

	

	//flag mode setting
	if (saveMode != shorelineModeIndex){
		saveMode = shorelineModeIndex;
		hasRendered = false;
	}
	

	//CALCULATE RENDER PASS FLAG
	renderPass = true;
	if (shorelineModeIndex == 0){
		if (shorelineRunIndex == 0 && hasRendered && Application.isPlaying) renderPass = false;
		if (shorelineRunIndex == 1) renderPass = true;
	}
	if (shorelineModeIndex == 1 && hasRendered && Application.isPlaying) renderPass = false;




	//RENDER
	if (!renderPass){

		if (CamInfo != null) CamInfo.enabled = false;
		if (CamTools != null) CamTools.enabled = false;

	} else {

		if (CamInfo != null) CamInfo.enabled = true;
		if (CamTools != null) CamTools.enabled = true;
		if (CamDepth != null) CamDepth.enabled = true;

		//set Depth Thresholds
		if (shorelineModeIndex == 0){
			CamDepth._sceneDepth = sceneDepth;
			CamDepth._shoreDepth = shoreDepth;
		}

		if (attachToSurface != null){

			//force y height
			transform.localScale.y = 1.0;

			//force y position based on surface attachment
			if (attachToSurface != null){
				transform.position.y = attachToSurface.position.y;
			}


			//AUTO GENERATION MODE --------------------------------------------------
			if (shorelineModeIndex == 0){

				//Set object and camera Projection Size
				maxScale = Mathf.Max(transform.localScale.x,transform.localScale.z);
				CamInfo.orthographicSize = maxScale * 20.0;
				if (transform.localScale.x < transform.localScale.z){
					camCoords = Vector4(transform.localScale.x/transform.localScale.z,
					1.0,
					0.5-((transform.localScale.x/transform.localScale.z)*0.5),
					0.0);
				} else if (transform.localScale.x > transform.localScale.z){
					camCoords = Vector4(1.0,
					transform.localScale.z/transform.localScale.x,
					0.0,
					0.5-((transform.localScale.z/transform.localScale.x)*0.5));
				}
				CamTools.surfaceRenderer.sharedMaterial.SetColor("_Mult",camCoords);

				//Update when moved,rotated, or scaled, or edited
				if (CamTools != null){
					if (currPos != transform.position){
						currPos = transform.position;
						CamTools.CameraUpdate();
					}
					if (currScale != transform.localScale){
						currScale = transform.localScale;
						CamTools.CameraUpdate();
					}
					if (currRot != transform.rotation){
						currRot = transform.rotation;
						CamTools.CameraUpdate();
					}

					if (curr_sceneDepth != sceneDepth){
						curr_sceneDepth = sceneDepth;
						CamTools.CameraUpdate();
					}
					if (curr_shoreDepth != shoreDepth){
						curr_shoreDepth = shoreDepth;
						CamTools.CameraUpdate();
					}

					if (Application.isPlaying) CamTools.CameraUpdate();

				}
			}


			//CUSTOM TEXTURE MODE --------------------------------------------------
			if (shorelineModeIndex == 1){
				if (customDepthTex != null){
					if (renderObject != null){
						renderObject.sharedMaterial.SetColor("_Mult",Vector4(1,1,0,0));
						renderObject.sharedMaterial.SetTexture("_MainTex",customDepthTex);
					}
				}
				
			}


			if (Application.isPlaying && Time.time > 1.0) hasRendered = true;


		}

	}

}
}
	



