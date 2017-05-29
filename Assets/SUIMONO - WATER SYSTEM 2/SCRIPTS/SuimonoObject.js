
@script ExecuteInEditMode()
#pragma strict
import System.IO;


var systemTime : float = 0.0;
var systemLocalTime : float = 0.0;
var flowSpeed : float = 0.1;
var flowDirection : float = 180.0;
var useBeaufortScale : boolean = false;
var beaufortScale : float = 1.0;
var turbulenceFactor : float = 1.0;
var waveScale : float = 0.5;
var lgWaveHeight : float = 0.0;
var lgWaveScale : float = 1.0;
var waveHeight : float = 1.0;
var heightProjection : float = 1.0;
var useHeightProjection : float = 1.0;
var refractStrength : float = 1.0;
var reflectProjection : float = 1.0;
var reflectBlur : float = 0.0;
var aberrationScale : float = 0.1;
var roughness : float = 0.1;
var roughness2 : float = 0.35;
var reflectTerm : float = 0.0255;
var showDepthMask : boolean = false;
var showWorldMask : boolean = false;
var cameraDistance : float = 1000.0;
var underwaterDepth : float = 5;

//DX9 version
var useDX9Settings : boolean = false;


//objects
private var moduleObject : SuimonoModule;
private var suimonoModuleLibrary : SuimonoModuleLib;
private var suimonoObject : GameObject;
private var surfaceRenderer : Renderer;
private var surfaceMesh : MeshFilter;
private var surfaceCollider : MeshCollider;
private var surfaceReflections : cameraTools;
private var surfaceReflBlur : Suimono_DistanceBlur;
private var scaleObject : GameObject;
private var scaleRenderer : Renderer;
private var scaleMesh : MeshFilter;
private var surfaceVolume : Renderer;

//materials
private var tempMaterial : Material;

//type and options
var suimonoVersionNumber : String;
var showGeneral : boolean = false;
var typeIndex : int = 1;
var typeOptions = new Array("Infinite 3D Ocean", "3D Waves", "Flat Plane");
var editorIndex : int = 1;
var editorUseIndex : int = 1;
var editorOptions = new Array( "Simple","Advanced");

//mesh and lod
var enableCustomMesh : boolean = false;
private var meshWasSet : boolean = false;
var lodIndex : int;
var useLodIndex : int;
var lodOptions = new Array("High Detail","Medium Detail","Low Detail","Single Quad");
var customMesh : Mesh;
var oceanScale : float = 2.0;

//underwater
var enableUnderwaterFX : boolean = true;

//casutics
var enableCausticFX : boolean = true;
var causticsFade : float = 0.55;
var causticsColor : Color = Color(1,1,1,1);

//tessellation
var enableTess : boolean = true;
var useEnableTess : boolean = true;
var waveTessAmt : float = 8.0;
var waveTessMin : float = 0.0;
var waveTessSpread : float = 0.08;

//reflections
var dynamicReflectFlag : float = 1.0;
var enableReflections : boolean = true;
var enableDynamicReflections : boolean = true;

var useEnableReflections : boolean = true;
var useEnableDynamicReflections : boolean = true;

var useReflections : boolean = true;
var useDynReflections : boolean = true;
var reflectLayer : int = 0;
var reflectResolution : int = 4;
var reflectLayerMask : LayerMask;
var reflectionDistance : float = 1000.0;
//var reflectionSpread : float = 0.5;
var suiLayerMasks : Array;
var resOptions = new Array("4096","2048","1024","512","256","128","64","32","16","8");
var resolutions = new Array(4096,2048,1024,512,256,128,64,32,16,8);
var reflectFallback : int = 1;
var resFallbackOptions = new Array("None","skybox","Custom Cubemap","Color");
var customRefCubemap : Texture;
var customRefColor : Color = Color(0.9,0.9,0.9,1.0);
var reflectionColor : Color = Color(1,1,1,1);

//custom textures
var enableCustomTextures : boolean = false;
var customTexNormal1 : Texture2D;
var customTexHeight1 : Texture2D;
var customTexNormal2 : Texture2D;
var customTexHeight2 : Texture2D;
var customTexNormal3 : Texture2D;
var customTexHeight3 : Texture2D;
var useTexNormal1 : Texture2D;
var useTexHeight1 : Texture2D;
var useTexNormal2 : Texture2D;
var useTexHeight2 : Texture2D;
var useTexNormal3 : Texture2D;
var useTexHeight3 : Texture2D;

//waves
var showWaves : boolean = false;
var customWaves : boolean = false;
var localTime : float = 0.0;
private var flow_dir : Vector2 = Vector2(0,0);
private var tempAngle : Vector3;
var beaufortVal : float = 1.0;

//shorelines
var showShore : boolean = false;
var shorelineHeight : float = 0.75;
var shorelineFreq : float = 0.5;
var shorelineScale : float = 0.15;
var shorelineSpeed : float = 2.5;
var shorelineNorm : float = 0.5;

//surface
var showSurface : boolean = false;
var overallBright : float = 1.0;
var overallTransparency : float = 1.0;
var depthAmt : float = 0.1;
var shallowAmt : float = 0.1;
var depthColor : Color;
var shallowColor : Color;
var edgeAmt : float = 0.1;
var specularColor : Color;
var sssColor : Color;
var blendColor : Color;
var overlayColor : Color;

//foam
var showFoam : boolean = false;
var enableFoam : boolean = true;
var foamColor : Color = Color(0.9,0.9,0.9,1.0);
var foamScale : float = 40.0;
var foamSpeed : float = 0.1;
var edgeFoamAmt : float = 0.5;
var shallowFoamAmt : float = 1.0;
var hFoamHeight : float = 1.0;
var hFoamSpread : float = 1.0;
var heightFoamAmt : float = 0.5;

//underwater 
var showUnderwater : boolean = false;
var underwaterColor : Color = Color(1,0,0,1);
var underLightFactor : float = 1.0;
var underRefractionAmount : float = 0.005;
var underRefractionScale : float = 1.5;
var underRefractionSpeed : float = 0.5;
var underwaterFogDist : float = 20.0;
var underwaterFogSpread : float = 0.0;
var enableUnderDebris : boolean = false;
var underBlurAmount : float = 1.0;
var underDarkRange : float = 40.0;

//scaling
var setScale : float = 1.0;
var currentAngles : Vector3 = Vector3(0,0,0);
var currentPosition : Vector3 = Vector3(0,0,0);
var newPos : Vector3 = Vector3(0,0,0);
var spacer : float = 0.0;
var setScaleX : float = 0.0;
var setScaleZ : float = 0.0;
var offamt : float = 0.0;
var savePos : Vector2 = Vector2(0,0);
var recPos : Vector2 = Vector2(0,0);
var _suimono_uv : Vector2 = Vector2(0,0);

//editor
var showSimpleEditor : boolean = false;

//shaders
var useShader : Shader;
var currUseShader : Shader;
var shader_Surface : Shader;
var shader_Scale : Shader;
var shader_Under : Shader;

//presets
var presetDirs : String[];
var presetFiles : String[];
var presetIndex : int = -1;
var presetUseIndex : int = -1;
var presetFileIndex : int = 0;
var presetFileUseIndex : int = 0;
var presetOptions : String[];
var showPresets : boolean = false;
var presetStartTransition : boolean = false;
var presetTransitionCurrent : int = 0;
var presetTransitionTime : float = 5.0;
var presetTransIndexFrm : int = 0;
var presetTransIndexTo : int = 0;
var presetToggleSave : boolean = false;
var presetsLoaded : boolean = false;
var presetDataArray : String[];
var presetDataString : String;
var dir : String = "";
var baseDir : String = "SUIMONO - WATER SYSTEM 2/RESOURCES/";
var presetSaveName : String = "my custom preset";
var presetFile : String = "";
var workData : String;
var workData2 : String;

 var materialPath : String;
//var currCoords : Vector2;
var oceanUseScale : float;
var useSc : float;
var setSc : Vector2;
var scaleOff : Vector2;
var i : int;
var layerName : String;
var skybox : Material;


var setFolder : int;
var setPreset : int;
var presetDirsArr : Array;
var d : int;
var dn : int;
var presetFilesArr : Array;
var pdir : String;
var fileInfo : FileInfo[];
var f : int = 0;
var px : int = 0;
var nx : int = 0;
var ax : int = 0;
var n : int = 0;

var setMode : int;
var tempPresetDirsArr : Array;
var dirInfo : FileInfo[];
var tempPresetDirs : String[];
var tempPresetFilesArr : Array;
var tempPresetFiles : String[];
var oldName : String;
var moveName : String;
var setNum : int;

var sw : StreamWriter;
var sr : StreamReader;
var key : String;
var dat : String;
var pFrom : int;
var pTo : int;
var dx : int;
var datFile : TextAsset;
var dataS : String[];
var retData : String;
var retVal : boolean;




function Start(){

	//DISCONNECT FROM PREFAB
	#if UNITY_EDITOR
	PrefabUtility.DisconnectPrefabInstance(this.gameObject);
	#endif

	//SET PRESET DIRECTORIES
	#if UNITY_EDITOR
		baseDir = "SUIMONO - WATER SYSTEM 2/RESOURCES/";
	#else
		baseDir = "Resources/";
	#endif
	dir = Application.dataPath + "/" + baseDir;


	//get Suimono objects
	if (GameObject.Find("SUIMONO_Module") != null){
		moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule) as SuimonoModule;
		if (moduleObject != null) suimonoModuleLibrary = moduleObject.GetComponent(SuimonoModuleLib) as SuimonoModuleLib;
	}
	
	//get surface objects
	suimonoObject = this.transform.Find("Suimono_Object").gameObject;
	surfaceRenderer = transform.Find("Suimono_Object").gameObject.GetComponent(Renderer);
	surfaceMesh = transform.Find("Suimono_Object").GetComponent(MeshFilter);
	surfaceCollider = transform.Find("Suimono_Object").GetComponent(MeshCollider);
	surfaceReflections = transform.Find("cam_LocalReflections").gameObject.GetComponent(cameraTools) as cameraTools;
	surfaceReflBlur = transform.Find("cam_LocalReflections").gameObject.GetComponent(Suimono_DistanceBlur) as Suimono_DistanceBlur;

	//get scale object (infinite ocean)
	scaleObject = this.transform.Find("Suimono_ObjectScale").gameObject;
	scaleRenderer = transform.Find("Suimono_ObjectScale").gameObject.GetComponent(Renderer);
	scaleMesh = transform.Find("Suimono_ObjectScale").GetComponent(MeshFilter);

	//Store Shader References
	shader_Surface = Shader.Find("Suimono2/surface");
	shader_Scale = Shader.Find("Suimono2/surface_scale");
	shader_Under = Shader.Find("Suimono2/surface_under");

	//save material if not already saved
	#if UNITY_EDITOR
		materialPath = "Assets/SUIMONO - WATER SYSTEM 2/Resources/mat_" + this.gameObject.name + ".mat";
		if (AssetDatabase.LoadAssetAtPath(materialPath,Material) == null){
			tempMaterial = new Material(suimonoObject.GetComponent(Renderer).sharedMaterial);
			AssetDatabase.CreateAsset(tempMaterial, materialPath);
		}
		tempMaterial = AssetDatabase.LoadAssetAtPath(materialPath,Material);
	#else
		tempMaterial = new Material(suimonoObject.GetComponent(Renderer).sharedMaterial);
	#endif

	//setup custom material surface
	if (suimonoObject != null){
		tempMaterial.shader = shader_Surface;
		suimonoObject.GetComponent(Renderer).sharedMaterial = tempMaterial;
		surfaceRenderer = transform.Find("Suimono_Object").gameObject.GetComponent(Renderer);
	}

	//Init Presets
	#if !UNITY_WEBPLAYER
		PresetInit();
		PresetLoad(presetIndex);
	#endif
}







function LateUpdate () {
if (moduleObject != null){

	//-------------------------------------------------------
	//###  SET SUIMONO VERSION  ###
	//-------------------------------------------------------
	//inherit suimono version number from module object for display in UI elements
	suimonoVersionNumber = moduleObject.suimonoVersionNumber;


	//-------------------------------------------------------
	//###  SET LOCAL TIME AND DIRECTION  ###
	//-------------------------------------------------------
	if (moduleObject != null) systemLocalTime = moduleObject.systemTime;
	localTime = systemLocalTime * flowSpeed * (1.0/waveScale);
	flow_dir = SuimonoConvertAngleToVector(flowDirection);

	surfaceRenderer.sharedMaterial.SetVector("_suimono_Dir",Vector4(flow_dir.x,1.0,flow_dir.y,localTime));

	//-------------------------------------------------------
	//###  SET LAYER MASK  ###
	//-------------------------------------------------------
	gameObject.layer = moduleObject.layerWaterNum;
	if (suimonoObject != null) suimonoObject.layer = moduleObject.layerWaterNum;
	if (scaleObject != null) scaleObject.layer = moduleObject.layerWaterNum;
	if (surfaceReflections != null) surfaceReflections.gameObject.layer = moduleObject.layerWaterNum; 
	suiLayerMasks = new Array();
	for (i = 0; i < 32; i++){
		layerName = LayerMask.LayerToName(i);
		suiLayerMasks.Add(layerName);
	}

	//-------------------------------------------------------
	//###  FORCE SIZING  ###
	//-------------------------------------------------------
	if (underwaterDepth < 0.1) underwaterDepth = 0.1;
	transform.localScale.y = 1.0;
	suimonoObject.transform.localScale.y = 1.0;
	scaleObject.transform.localScale.y = 1.0;
	surfaceReflections.transform.localScale.y = 1.0;

	//-------------------------------------------------------
	//###  CALCULATE BEAUFORT SCALE  ###
	//-------------------------------------------------------
	useBeaufortScale = !customWaves;
	if (useBeaufortScale){
		beaufortVal = beaufortScale/12.0;
		turbulenceFactor = Mathf.Clamp(Mathf.Lerp(-0.1,2.1,beaufortVal)*0.9,0.0,0.75);
		waveHeight = Mathf.Clamp(Mathf.Lerp(0.0,5.0,beaufortVal),0.0,0.65);
		waveHeight = waveHeight - Mathf.Clamp(Mathf.Lerp(-1.5,1.0,beaufortVal),0.0,0.5);
		lgWaveHeight = Mathf.Clamp(Mathf.Lerp(-0.2,1.1,beaufortVal)*2.8,0.0,3.0);

		//freeze scale for ocean
		if (typeIndex == 0){
			waveScale = 0.5;
			lgWaveScale = 0.03125;
		}
	}


	//-------------------------------------------------------
	//###  LOAD PRESET CHANGES  ###
	//-------------------------------------------------------
	// detect changes to preset and run update when applicable
	if (presetUseIndex != presetIndex){
		presetUseIndex = presetIndex;
		PresetLoad(presetIndex);
	}
	// reload preset settings when editor mode is switched between simple and advanced.
	#if UNITY_EDITOR
		if (editorUseIndex != editorIndex){
			editorUseIndex = editorIndex;
			PresetLoad(presetIndex);
		}
	#endif


	//-------------------------------------------------------
	//###  SET MESH LOD LEVEL  ###
	//-------------------------------------------------------
	// change the surface mesh based on the selected level of detail
	if (typeIndex == 0) useLodIndex = 0; //infinite ocean requires high detail mesh
	if (typeIndex == 1) useLodIndex = lodIndex; //3d waves setting picks mesh set in UI
	if (typeIndex == 2) useLodIndex = 3; // flat surface forces single quad mesh
	if (enableCustomMesh == false){
		if (suimonoModuleLibrary && !meshWasSet){
			if (suimonoModuleLibrary.texNormalC && surfaceMesh != null) surfaceMesh.mesh = suimonoModuleLibrary.meshLevel[useLodIndex];	
			if (suimonoModuleLibrary.texNormalC && surfaceCollider != null) surfaceCollider.sharedMesh = suimonoModuleLibrary.meshLevel[3];
			meshWasSet = true;
		} else {
			meshWasSet = false;
		}
	} else {
		if (customMesh != null){
			if (surfaceMesh != null) surfaceMesh.mesh = customMesh;
			if (surfaceCollider != null) surfaceCollider.sharedMesh = customMesh;
		} else {
			if (suimonoModuleLibrary.texNormalC && surfaceMesh != null) surfaceMesh.mesh = suimonoModuleLibrary.meshLevel[useLodIndex];
			if (suimonoModuleLibrary.texNormalC && surfaceCollider != null) surfaceCollider.sharedMesh = suimonoModuleLibrary.meshLevel[3];
			meshWasSet = false;
		}
	}
	if (useLodIndex == 3){
		useHeightProjection = 0.0;
		useEnableTess = false;
	} else {
		useHeightProjection = heightProjection;
		useEnableTess = enableTess;
	}

	//set scale mesh
	if (suimonoModuleLibrary.texNormalC && scaleMesh != null) scaleMesh.mesh = suimonoModuleLibrary.meshLevel[1];	


	//-------------------------------------------------------
	//###  Set Custom Textures  ###
	//-------------------------------------------------------
	if (enableCustomTextures){
		if (customTexNormal1 != null){
			useTexNormal1 = customTexNormal1;
			} else {
				useTexNormal1 = suimonoModuleLibrary.texNormalC;
			}
		if (customTexHeight1 != null){
			useTexHeight1 = customTexHeight1;
			} else {
				useTexHeight1 = suimonoModuleLibrary.texHeightC;
			}
		if (customTexNormal2 != null){
			useTexNormal2 = customTexNormal2;
			} else {
				useTexNormal2 = suimonoModuleLibrary.texNormalT;
			}
		if (customTexHeight2 != null){
			useTexHeight2 = customTexHeight2;
			} else {
				useTexHeight2 = suimonoModuleLibrary.texHeightT;
			}
		if (customTexNormal3 != null){
			useTexNormal3 = customTexNormal3;
			} else {
				useTexNormal3 = suimonoModuleLibrary.texNormalR;
			}
		if (customTexHeight3 != null){
			useTexHeight3 = customTexHeight3;
			} else {
				useTexHeight3 = suimonoModuleLibrary.texHeightR;
			}

	} else {
		if (suimonoModuleLibrary != null){
			useTexNormal1 = suimonoModuleLibrary.texNormalC;
			useTexHeight1 = suimonoModuleLibrary.texHeightC;
			useTexNormal2 = suimonoModuleLibrary.texNormalT;
			useTexHeight2 = suimonoModuleLibrary.texHeightT;
			useTexNormal3 = suimonoModuleLibrary.texNormalR;
			useTexHeight3 = suimonoModuleLibrary.texHeightR;
		}
	}
	if (suimonoModuleLibrary != null){
		if (surfaceRenderer != null) surfaceRenderer.sharedMaterial.SetTexture("_MaskTex",suimonoModuleLibrary.texMask);
	}


	//-------------------------------------------------------
	//###  SET REFLECTION OBJECT PROPERTIES  ###
	//-------------------------------------------------------
	if (surfaceReflections != null){

		useEnableReflections = enableReflections;
		useEnableDynamicReflections = enableDynamicReflections;

		if (!moduleObject.enableReflections) useEnableReflections = false;
		if (!moduleObject.enableDynamicReflections) useEnableDynamicReflections = false;
		
		//if (!enableDynamicReflections || !enableReflections || !moduleObject.enableReflections || !moduleObject.enableDynamicReflections){
		if (!useEnableReflections || !moduleObject.enableReflections){
			useReflections = false;
			surfaceReflections.gameObject.SetActive(false);
		} else {

			if (!useEnableDynamicReflections || !moduleObject.enableDynamicReflections){
				surfaceReflections.gameObject.SetActive(false);
			} else {
				surfaceReflections.gameObject.SetActive(true);
				useReflections = true;
				reflectLayer = (reflectLayer & ~(1 << moduleObject.layerWaterNum));
				reflectLayer = (reflectLayer & ~(1 << moduleObject.layerDepthNum));
				reflectLayer = (reflectLayer & ~(1 << moduleObject.layerScreenFXNum));
				//reflectLayer = (reflectLayer & ~(1 << moduleObject.layerUnderNum));

				surfaceReflections.setLayers = reflectLayer;
				surfaceReflections.resolution = System.Convert.ToInt32(resolutions[reflectResolution]);
				reflectionDistance = moduleObject.setCameraComponent.farClipPlane+200.0;
				surfaceReflections.reflectionDistance = reflectionDistance;

				//blur settings
				surfaceReflBlur.blurAmt = reflectBlur;//Mathf.Floor(Mathf.Lerp(0,3,reflectBlur));

				if (useShader == shader_Under){
					surfaceReflections.isUnderwater = true;
				} else {
					surfaceReflections.isUnderwater = false;
				}
			}

		}

	}




	//-------------------------------------------------------
	//###  SEND SETTINGS TO SHADER  ###
	//-------------------------------------------------------
	if (surfaceRenderer != null){

		//set shader
		if (Application.isPlaying && useShader != null){
			if (currUseShader != useShader){
				currUseShader = useShader;
				surfaceRenderer.sharedMaterial.shader = currUseShader;
			}
		}

		//set playmode
		if (!Application.isPlaying){
			surfaceRenderer.sharedMaterial.SetFloat("_isPlaying",0.0);
		} else {
			surfaceRenderer.sharedMaterial.SetFloat("_isPlaying",1.0);
		}

		//set texture
		surfaceRenderer.sharedMaterial.SetTexture("_HeightTex",useTexHeight1);
		surfaceRenderer.sharedMaterial.SetTexture("_NormalTex",useTexNormal1);
		surfaceRenderer.sharedMaterial.SetTexture("_HeightTexD",useTexHeight2);
		surfaceRenderer.sharedMaterial.SetTexture("_NormalTexD",useTexNormal2);
		surfaceRenderer.sharedMaterial.SetTexture("_HeightTexR",useTexHeight3);
		surfaceRenderer.sharedMaterial.SetTexture("_NormalTexR",useTexNormal3);

		//set beaufort and waves
		surfaceRenderer.sharedMaterial.SetFloat("_beaufortFlag",useBeaufortScale ? 1.0 : 0.0);
		surfaceRenderer.sharedMaterial.SetFloat("_beaufortScale",beaufortVal);
		surfaceRenderer.sharedMaterial.SetFloat("_turbulenceFactor",turbulenceFactor);
	
		//set texture speed and scale
		surfaceRenderer.sharedMaterial.SetTextureScale("_HeightTex",Vector2((suimonoObject.transform.localScale.x/waveScale)*transform.localScale.x,(suimonoObject.transform.localScale.z/waveScale)*transform.localScale.z));
		surfaceRenderer.sharedMaterial.SetVector("_scaleUVs",Vector4(suimonoObject.transform.localScale.x/waveScale,suimonoObject.transform.localScale.z/waveScale,0,0));
		surfaceRenderer.sharedMaterial.SetFloat("_lgWaveScale",lgWaveScale);
		surfaceRenderer.sharedMaterial.SetFloat("_lgWaveHeight",lgWaveHeight);
		
		//set tessellation settings
		if (typeIndex == 0){
			surfaceRenderer.sharedMaterial.SetFloat("_tessScale",suimonoObject.transform.localScale.x);
		} else {
			surfaceRenderer.sharedMaterial.SetFloat("_tessScale",transform.localScale.x);
		}
		surfaceRenderer.sharedMaterial.SetFloat("_Tess",Mathf.Lerp(0.001,waveTessAmt,useEnableTess ? 1.0 : 0.0));
		surfaceRenderer.sharedMaterial.SetFloat("_minDist", Mathf.Lerp(-180.0,0.0,waveTessMin));
		surfaceRenderer.sharedMaterial.SetFloat("_maxDist", Mathf.Lerp(20.0,500.0,waveTessSpread));

		//set system fog coordinates
		surfaceRenderer.sharedMaterial.SetFloat("_unity_fogstart",RenderSettings.fogStartDistance);
		surfaceRenderer.sharedMaterial.SetFloat("_unity_fogend",RenderSettings.fogEndDistance);

		//set caustics
		surfaceRenderer.sharedMaterial.SetFloat("_causticsFlag",enableCausticFX ? 1.0 : 0.0);
		surfaceRenderer.sharedMaterial.SetFloat("_CausticsFade",Mathf.Lerp(1,500,causticsFade));
		surfaceRenderer.sharedMaterial.SetColor("_CausticsColor",causticsColor);

		//set aberration scale
		surfaceRenderer.sharedMaterial.SetFloat("_aberrationScale",aberrationScale);

		//set foam speed and scale
		surfaceRenderer.sharedMaterial.SetFloat("_EdgeFoamFade",Mathf.Lerp(1500.0,5.0,edgeFoamAmt));
		surfaceRenderer.sharedMaterial.SetFloat("_HeightFoamAmt",heightFoamAmt);
		surfaceRenderer.sharedMaterial.SetFloat("_HeightFoamHeight",hFoamHeight);
		surfaceRenderer.sharedMaterial.SetFloat("_HeightFoamSpread",hFoamSpread);
		surfaceRenderer.sharedMaterial.SetFloat("_foamSpeed",foamSpeed);
		surfaceRenderer.sharedMaterial.SetTextureScale("_FoamTex",foamScale*Vector2((suimonoObject.transform.localScale.x/foamScale)*transform.localScale.x,(suimonoObject.transform.localScale.z/foamScale)*transform.localScale.z));
		surfaceRenderer.sharedMaterial.SetFloat("_foamScale",Mathf.Lerp(160.0,1.0,foamScale));
		surfaceRenderer.sharedMaterial.SetColor("_FoamColor",foamColor);
		surfaceRenderer.sharedMaterial.SetFloat("_ShallowFoamAmt",shallowFoamAmt);

		//set height and normal scales
		surfaceRenderer.sharedMaterial.SetFloat("_heightScaleFac",(1.0/transform.localScale.y));
		surfaceRenderer.sharedMaterial.SetFloat("_heightProjection",useHeightProjection);
		surfaceRenderer.sharedMaterial.SetFloat("_heightScale",waveHeight);
		surfaceRenderer.sharedMaterial.SetFloat("_RefractStrength",refractStrength);
		surfaceRenderer.sharedMaterial.SetFloat("_ReflectStrength",reflectProjection);

		//set shoreline properties
		surfaceRenderer.sharedMaterial.SetFloat("_shorelineHeight",shorelineHeight);
		surfaceRenderer.sharedMaterial.SetFloat("_shorelineFrequency",shorelineFreq);
		surfaceRenderer.sharedMaterial.SetFloat("_shorelineScale",0.1);//shorelineScale - currently being forced to 0.1
		surfaceRenderer.sharedMaterial.SetFloat("_shorelineSpeed",shorelineSpeed);
		surfaceRenderer.sharedMaterial.SetFloat("_shorelineNorm",shorelineNorm);

		//set physical properties
		surfaceRenderer.sharedMaterial.SetFloat("_roughness",roughness);
		surfaceRenderer.sharedMaterial.SetFloat("_roughness2",roughness2);
		surfaceRenderer.sharedMaterial.SetFloat("_reflecTerm",reflectTerm);

		//set surface settings
		surfaceRenderer.sharedMaterial.SetFloat("_overallBrightness", overallBright);
		surfaceRenderer.sharedMaterial.SetFloat("_overallTransparency", overallTransparency);
		surfaceRenderer.sharedMaterial.SetFloat("_DepthFade",Mathf.Lerp(0.1,200.0,depthAmt));
		surfaceRenderer.sharedMaterial.SetFloat("_ShallowFade",Mathf.Lerp(0.1,800.0,shallowAmt));
		surfaceRenderer.sharedMaterial.SetColor("_depthColor",depthColor);
		surfaceRenderer.sharedMaterial.SetColor("_shallowColor",shallowColor);
		surfaceRenderer.sharedMaterial.SetFloat("_EdgeFade", Mathf.Lerp(10.0,1000.0,edgeAmt));
		surfaceRenderer.sharedMaterial.SetColor("_SpecularColor",specularColor);
		surfaceRenderer.sharedMaterial.SetColor("_SSSColor",sssColor);
		surfaceRenderer.sharedMaterial.SetColor("_BlendColor",blendColor);
		surfaceRenderer.sharedMaterial.SetColor("_OverlayColor",overlayColor);
		surfaceRenderer.sharedMaterial.SetColor("_UnderwaterColor",underwaterColor);

		//set reflection properties
		surfaceRenderer.sharedMaterial.SetFloat("_reflectFlag",useEnableReflections ? 1.0 : 0.0);
		surfaceRenderer.sharedMaterial.SetFloat("_reflectDynamicFlag",useEnableDynamicReflections ? 1.0 : 0.0);
		surfaceRenderer.sharedMaterial.SetFloat("_reflectFallback", reflectFallback);
		surfaceRenderer.sharedMaterial.SetColor("_reflectFallbackColor", customRefColor);
		surfaceRenderer.sharedMaterial.SetColor("_ReflectionColor", reflectionColor);

		//set skybox texture
        skybox = RenderSettings.skybox;
        if(skybox != null && skybox.HasProperty("_Tex") && skybox.HasProperty("_Tint") && skybox.HasProperty("_Exposure") && skybox.HasProperty("_Rotation")){
			surfaceRenderer.sharedMaterial.SetTexture("_SkyCubemap",skybox.GetTexture("_Tex"));
			surfaceRenderer.sharedMaterial.SetColor("_SkyTint", skybox.GetColor("_Tint"));
        	surfaceRenderer.sharedMaterial.SetFloat("_SkyExposure", skybox.GetFloat("_Exposure"));
        	surfaceRenderer.sharedMaterial.SetFloat("_SkyRotation", skybox.GetFloat("_Rotation"));
		}

		//set custom cubemap
		if (customRefCubemap != null)
			surfaceRenderer.sharedMaterial.SetTexture("_CubeTex", customRefCubemap);

		//set camera properties
		surfaceRenderer.sharedMaterial.SetFloat("_cameraDistance",cameraDistance);



		//-------------------------------------------------------
		//###  SET SHADER DEFINE KEYWORDS  ###
		//-------------------------------------------------------
		if (surfaceRenderer != null){
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_TESS_ON");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_TRANS_ON");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_CAUST_ON");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_DYNREFL_ON");
			if (enableTess) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_TESS_ON");
			if (moduleObject.enableTransparency) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_TRANS_ON");
			if (moduleObject.enableCaustics) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_CAUST_ON");
			if (useDynReflections) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_DYNREFL_ON");

			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_OFF");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_SKY");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_CUBE");
			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_COLOR");
			if (reflectFallback == 0) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_OFF");
			if (reflectFallback == 1) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_SKY");
			if (reflectFallback == 2) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_CUBE");
			if (reflectFallback == 3) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_COLOR");

			surfaceRenderer.sharedMaterial.DisableKeyword("SUIMONO_FOAM_ON");
			if (Application.isPlaying && enableFoam) surfaceRenderer.sharedMaterial.EnableKeyword("SUIMONO_FOAM_ON");
		}

		if (scaleRenderer != null && typeIndex == 0){
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_TESS_ON");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_TRANS_ON");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_CAUST_ON");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_DYNREFL_ON");
			if (enableTess) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_TESS_ON");
			if (moduleObject.enableTransparency) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_TRANS_ON");
			if (moduleObject.enableCaustics) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_CAUST_ON");
			if (useDynReflections) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_DYNREFL_ON");

			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_OFF");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_SKY");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_CUBE");
			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_REFL_COLOR");
			if (reflectFallback == 0) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_OFF");
			if (reflectFallback == 1) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_SKY");
			if (reflectFallback == 2) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_CUBE");
			if (reflectFallback == 3) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_REFL_COLOR");

			scaleRenderer.sharedMaterial.DisableKeyword("SUIMONO_FOAM_ON");
			if (Application.isPlaying && enableFoam) scaleRenderer.sharedMaterial.EnableKeyword("SUIMONO_FOAM_ON");

		}

	}


	//-------------------------------------------------------
	//###  enable / disable infinite scale surface  ###
	//-------------------------------------------------------
	if (typeIndex == 0 && Application.isPlaying){
		if (moduleObject.isUnderwater){
			if (scaleRenderer != null) scaleRenderer.enabled = false;
		} else {
			if (scaleRenderer != null) scaleRenderer.enabled = true;
		}
	} else {
		if (scaleRenderer != null) scaleRenderer.enabled = false;
	}



	//-------------------------------------------------------
	//###  set position and rotation for infinite ocean  ###
	//-------------------------------------------------------
	//force 'infinite' size in scene view
	//if (typeIndex == 0){
	//	if (!Application.isPlaying){
		//	suimonoObject.transform.localScale = Vector3(50,1,50);
		//} else {
	//		suimonoObject.transform.localScale = Vector3(1,1,1);
	//	}
	//}

	if (Application.isPlaying){
		if (typeIndex == 0){

			//force rotation
			transform.eulerAngles.y = 0.0;

			//calculate scales
			if (oceanScale < 1.0) oceanScale = 1.0;
			offamt = (0.4027 * oceanScale)/waveScale;
			spacer = (suimonoObject.transform.localScale.x * 4.0);
			newPos = Vector3(moduleObject.setCamera.position.x,suimonoObject.transform.position.y,moduleObject.setCamera.position.z);
			if (Mathf.Abs(suimonoObject.transform.position.x - newPos.x) > spacer){
				if (suimonoObject.transform.position.x > newPos.x) setScaleX -= offamt;
				if (suimonoObject.transform.position.x < newPos.x) setScaleX += offamt;
				suimonoObject.transform.position.x = newPos.x;
				scaleObject.transform.position.x = newPos.x;
			}
			if (Mathf.Abs(suimonoObject.transform.position.z - newPos.z) > spacer){
				if (suimonoObject.transform.position.z > newPos.z) setScaleZ -= offamt;
				if (suimonoObject.transform.position.z < newPos.z) setScaleZ += offamt;
				suimonoObject.transform.position.z = newPos.z;
				scaleObject.transform.position.z = newPos.z;
			}

			//update position
			if (currentPosition != suimonoObject.transform.position){
				currentPosition = suimonoObject.transform.position;
				savePos = Vector2(setScaleX,setScaleZ);
			}

			//set shader offset
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_uvx",0.0-(savePos.x));
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_uvy",0.0-(savePos.y));

			//set scale object offset
			scaleObject.transform.localPosition.y = -0.1;

			//set infinite ocean object scaling
			if (scaleRenderer != null){
				setScale = Mathf.Ceil(moduleObject.setCameraComponent.farClipPlane/20.0)*suimonoObject.transform.localScale.x;
				scaleObject.transform.localScale = Vector3(setScale*0.5,1.0,setScale*0.5);

				oceanUseScale = 4.0;
				this.transform.localScale = Vector3(1,1,1);
				suimonoObject.transform.localScale = Vector3(oceanUseScale*oceanScale,1.0,oceanUseScale*oceanScale);

				//copy shader settings to infinite scale surface
				if (scaleRenderer != null){
					scaleRenderer.sharedMaterial.CopyPropertiesFromMaterial(surfaceRenderer.sharedMaterial);
						scaleRenderer.sharedMaterial.SetFloat("_suimono_uvx",0.0-savePos.x);
						scaleRenderer.sharedMaterial.SetFloat("_suimono_uvy",0.0-savePos.y);
				
						setSc = scaleRenderer.sharedMaterial.GetTextureScale("_HeightTex");

						useSc = (scaleObject.transform.localScale.x/suimonoObject.transform.localScale.x);
						scaleRenderer.sharedMaterial.SetTextureScale("_HeightTex", setSc*useSc);
						scaleRenderer.sharedMaterial.SetTextureScale("_FoamTex", setSc*useSc);
				}

			}


		} else {
			savePos = Vector3(0,0,0);
			suimonoObject.transform.localScale = Vector3(1,1,1);
			scaleObject.transform.localScale = Vector3(1,1,1);
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_uvx",0.0);
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_uvy",0.0);

		}
	}



	//-------------------------------------------------------
	//###  Set Debug Modes  ###
	//-------------------------------------------------------
	if (surfaceRenderer != null){
		if (showDepthMask){
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_DebugDepthMask",1.0);
		} else {
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_DebugDepthMask",0.0);
		}

		if (showWorldMask){
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_DebugWorldNormalMask",1.0);
		} else {
			surfaceRenderer.sharedMaterial.SetFloat("_suimono_DebugWorldNormalMask",0.0);
		}

	}


	//-------------------------------------------------------
	//###  Update Preset Listing  ###
	//-------------------------------------------------------
	#if UNITY_EDITOR
		if (presetFileUseIndex != presetFileIndex){
			presetFileUseIndex = presetFileIndex;
			PresetInit();
		}
	#endif

}
}





function SuimonoConvertAngleToVector(convertAngle : float) : Vector2{
	//Note this is the same function as above, but renamed for better clarity.
	//eventually the above function should be deprecated.
	flow_dir = Vector2(0,0);
	tempAngle = Vector3(0,0,0);
	if (convertAngle <= 180.0){
		tempAngle = Vector3.Slerp(Vector3.forward,-Vector3.forward,(convertAngle)/180.0);
		flow_dir = Vector2(tempAngle.x,tempAngle.z);
	}
	if (convertAngle > 180.0){
		tempAngle = Vector3.Slerp(-Vector3.forward,Vector3.forward,(convertAngle-180.0)/180.0);
		flow_dir = Vector2(-tempAngle.x,tempAngle.z);
	}
	
	return flow_dir;
}









// ########## PUBLIC FUNCTIONS #######################################################################################################################

function SuimonoSetPreset( fName : String, pName : String){
	PresetLoadBuild(fName,pName);
}



function SuimonoSavePreset( fName : String, pName : String){
	setFolder = -1;
	setPreset = -1;

	setFolder = PresetGetNum("folder",fName);
	setPreset = PresetGetNum("preset",pName);

	if (setFolder >= 0 && setPreset >= 0){
		PresetSave(setFolder,setPreset);
	} else {
		Debug.Log("The Preset "+pName+" in folder "+fName+" cannot be found!");
	}
}





//########### NEW PRESETS #########################################################################################################################
function PresetInit(){

	#if !UNITY_WEBPLAYER

	//get preset directories
	presetDirsArr = new Array();
	dirInfo = DirectoryInfo(dir+"/").GetFiles("SUIMONO_PRESETS_*");
	if (DirectoryInfo(dir+"/") != null){
		for (d = 0; d < dirInfo.Length; d++){
			presetDirsArr.Add(dirInfo[d].ToString());
		}
	}
	presetDirs = new String[presetDirsArr.length];
	for (dn = 0; dn < presetDirsArr.length; dn++){
		presetDirs[dn] = presetDirsArr[dn].ToString();
		presetDirs[dn] = presetDirs[dn].Remove(0,dir.length);
		presetDirs[dn] = presetDirs[dn].Replace("SUIMONO_PRESETS_","");
		presetDirs[dn] = presetDirs[dn].Replace(".meta","");
	}


	//get preset files listing
	presetFilesArr = new Array();
	pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[presetFileIndex];

	fileInfo = DirectoryInfo(pdir).GetFiles("SUIMONO_PRESET_*");
	if (DirectoryInfo(pdir) != null){
		for (f = 0; f < fileInfo.Length; f++){
			presetFilesArr.Add(fileInfo[f].ToString());
		}
	}
	px = 0;
	for (nx = 0; nx < presetFilesArr.length; nx++){
		if (!presetFilesArr[nx].ToString().Contains(".meta")) px++;
	}
	presetFiles = new String[px];
	ax = 0;
	for (n = 0; n < presetFilesArr.length; n++){
		if (!presetFilesArr[n].ToString().Contains(".meta")){
			presetFiles[ax] = presetFilesArr[n].ToString();
			presetFiles[ax] = presetFiles[ax].Remove(0,pdir.length);
			presetFiles[ax] = presetFiles[ax].Replace("SUIMONO_PRESET_","");
			presetFiles[ax] = presetFiles[ax].Replace(".txt","");
			ax++;
		}
	}

	#endif
}







function PresetGetNum( mode : String, pName : String) : int {

	#if !UNITY_WEBPLAYER

	setMode = -1;
	setFolder = -1;
	setPreset = -1;

	if (mode == "folder"){
		//get preset directories
		tempPresetDirsArr = new Array();
		dirInfo = DirectoryInfo(dir+"/").GetFiles("SUIMONO_PRESETS_*");
		if (DirectoryInfo(dir+"/") != null){
			for (d = 0; d < dirInfo.Length; d++){
				tempPresetDirsArr.Add(dirInfo[d].ToString());
			}
		}
		tempPresetDirs = new String[tempPresetDirsArr.length];
		for (dn = 0; dn < tempPresetDirsArr.length; dn++){
			tempPresetDirs[dn] = tempPresetDirsArr[dn].ToString();
			tempPresetDirs[dn] = tempPresetDirs[dn].Remove(0,dir.length);
			tempPresetDirs[dn] = tempPresetDirs[dn].Replace("SUIMONO_PRESETS_","");
			tempPresetDirs[dn] = tempPresetDirs[dn].Replace(".meta","");
			if (tempPresetDirs[dn] == pName) setFolder = dn;
		}
		setMode = setFolder;
	}

	if (mode == "preset"){
		//get preset files listing
		tempPresetFilesArr = new Array();
		pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[presetFileIndex];
		fileInfo = DirectoryInfo(pdir).GetFiles("SUIMONO_PRESET_*");
		if (DirectoryInfo(pdir) != null){
			for (f = 0; f < fileInfo.Length; f++){
				tempPresetFilesArr.Add(fileInfo[f].ToString());
			}
		}
		px = 0;
		for (nx = 0; nx < tempPresetFilesArr.length; nx++){
			if (!tempPresetFilesArr[nx].ToString().Contains(".meta")) px++;
		}
		tempPresetFiles = new String[px];
		ax = 0;
		for (n = 0; n < tempPresetFilesArr.length; n++){
			if (!tempPresetFilesArr[n].ToString().Contains(".meta")){
				tempPresetFiles[ax] = tempPresetFilesArr[n].ToString();
				tempPresetFiles[ax] = tempPresetFiles[ax].Remove(0,pdir.length);
				tempPresetFiles[ax] = tempPresetFiles[ax].Replace("SUIMONO_PRESET_","");
				tempPresetFiles[ax] = tempPresetFiles[ax].Replace(".txt","");
				if (tempPresetFiles[ax] == pName) setPreset = ax;
				ax++;
			}
		}
		setMode = setPreset;
	}

	return setMode;

	#endif
}






function PresetRename( ppos : int, newName : String ){
#if UNITY_EDITOR
#if !UNITY_WEBPLAYER
	pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[presetFileIndex];
	oldName = pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt";
	moveName = pdir+"/SUIMONO_PRESET_"+newName+".txt";
	File.Move(oldName,moveName);
	AssetDatabase.Refresh();
	PresetInit();
#endif
#endif
}


function PresetAdd(){
#if UNITY_EDITOR
#if !UNITY_WEBPLAYER
	setNum = presetFiles.length;
	pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[presetFileIndex];
	while (File.Exists(pdir+"/SUIMONO_PRESET_New Preset "+setNum+".txt")){
		setNum += 1;
	}
	if (presetFiles.length >= 1){
		File.Create(pdir+"/SUIMONO_PRESET_New Preset "+setNum+".txt").Close();
	} else {
		File.Create(pdir+"/SUIMONO_PRESET_New Preset 1.txt").Close();
		setNum = 1;
	}
	AssetDatabase.Refresh();
	PresetInit();
	SuimonoSavePreset(presetDirs[presetFileIndex],"New Preset "+setNum);
#endif
#endif
}


function PresetDelete( fpos : int, ppos : int ){
#if UNITY_EDITOR
#if !UNITY_WEBPLAYER
	pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[fpos];
	if (File.Exists(pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt")){
		File.Delete(pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt");
		if (presetIndex == ppos) presetIndex = -1;
	}
	AssetDatabase.Refresh();
	PresetInit();
#endif
#endif
}


function PresetSave( fpos : int, ppos : int ){
#if UNITY_EDITOR
#if !UNITY_WEBPLAYER
	pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[fpos];
	if (File.Exists(pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt")){

		//Caclulate data
		presetDataString = "";
		presetDataString += (PresetEncode("color_depth")) + "\n";
		presetDataString += (PresetEncode("color_shallow")) + "\n";
		presetDataString += (PresetEncode("color_blend")) + "\n";	
		presetDataString += (PresetEncode("color_overlay")) + "\n";
		presetDataString += (PresetEncode("color_caustics")) + "\n";
		presetDataString += (PresetEncode("color_reflection")) + "\n";
		presetDataString += (PresetEncode("color_specular")) + "\n";
		presetDataString += (PresetEncode("color_sss")) + "\n";
		presetDataString += (PresetEncode("color_foam")) + "\n";
		presetDataString += (PresetEncode("color_underwater")) + "\n";
		presetDataString += (PresetEncode("data_beaufort")) + "\n";
		presetDataString += (PresetEncode("data_flowdir")) + "\n";
		presetDataString += (PresetEncode("data_flowspeed")) + "\n";
		presetDataString += (PresetEncode("data_wavescale")) + "\n";
		presetDataString += (PresetEncode("data_customwaves")) + "\n";
		presetDataString += (PresetEncode("data_waveheight")) + "\n";
		presetDataString += (PresetEncode("data_heightprojection")) + "\n";
		presetDataString += (PresetEncode("data_turbulence")) + "\n";
		presetDataString += (PresetEncode("data_lgwaveheight")) + "\n";
		presetDataString += (PresetEncode("data_lgwavescale")) + "\n";
		presetDataString += (PresetEncode("data_shorelineheight")) + "\n";
		presetDataString += (PresetEncode("data_shorelinefreq")) + "\n";
		presetDataString += (PresetEncode("data_shorelinescale")) + "\n";
		presetDataString += (PresetEncode("data_shorelinespeed")) + "\n";
		presetDataString += (PresetEncode("data_shorelinenorm")) + "\n";
		presetDataString += (PresetEncode("data_overallbright")) + "\n";
		presetDataString += (PresetEncode("data_overalltransparency")) + "\n";
		presetDataString += (PresetEncode("data_edgeamt")) + "\n";
		presetDataString += (PresetEncode("data_depthamt")) + "\n";
		presetDataString += (PresetEncode("data_shallowamt")) + "\n";
		presetDataString += (PresetEncode("data_refractstrength")) + "\n";
		presetDataString += (PresetEncode("data_aberrationscale")) + "\n";
		presetDataString += (PresetEncode("data_causticsfade")) + "\n";
		presetDataString += (PresetEncode("data_reflectprojection")) + "\n";
		presetDataString += (PresetEncode("data_reflectblur")) + "\n";
		presetDataString += (PresetEncode("data_reflectterm")) + "\n";
		presetDataString += (PresetEncode("data_roughness")) + "\n";
		presetDataString += (PresetEncode("data_roughness2")) + "\n";
		presetDataString += (PresetEncode("data_enablefoam")) + "\n";
		presetDataString += (PresetEncode("data_foamscale")) + "\n";
		presetDataString += (PresetEncode("data_foamspeed")) + "\n";
		presetDataString += (PresetEncode("data_edgefoamamt")) + "\n";
		presetDataString += (PresetEncode("data_shallowfoamamt")) + "\n";
		presetDataString += (PresetEncode("data_heightfoamamt")) + "\n";
		presetDataString += (PresetEncode("data_hfoamheight")) + "\n";
		presetDataString += (PresetEncode("data_hfoamspread")) + "\n";
		presetDataString += (PresetEncode("data_enableunderdebris")) + "\n";
		presetDataString += (PresetEncode("data_underlightfactor")) + "\n";
		presetDataString += (PresetEncode("data_underrefractionamount")) + "\n";
		presetDataString += (PresetEncode("data_underrefractionscale")) + "\n";
		presetDataString += (PresetEncode("data_underrefractionspeed")) + "\n";
		presetDataString += (PresetEncode("data_underbluramount")) + "\n";
		presetDataString += (PresetEncode("data_underwaterfogdist")) + "\n";
		presetDataString += (PresetEncode("data_underwaterfogspread")) + "\n";
		presetDataString += (PresetEncode("data_underDarkRange")) + "\n";



		//save data
		sw = new StreamWriter(pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt");
		sw.AutoFlush = true;
		sw.Write(presetDataString);
	    sw.Close();

		Debug.Log("Preset '"+presetFiles[ppos]+"' has been saved!");
	}
#endif
#endif
}




function PresetLoad( ppos : int ){
	if (presetIndex >= 0){
		pdir = dir + "/SUIMONO_PRESETS_"+presetDirs[presetFileIndex];
		sr = new StreamReader(pdir+"/SUIMONO_PRESET_"+presetFiles[ppos]+".txt");
	    presetDataString = sr.ReadToEnd();
	    sr.Close();

	    presetDataArray = presetDataString.Split("\n"[0]);

		//Decode Data
		for (dx = 0; dx < presetDataArray.Length; dx++){
			if (presetDataArray[dx] != "" && presetDataArray[dx] != "\n"){
				pFrom = presetDataArray[dx].IndexOf("<") + "<".Length;
				pTo = presetDataArray[dx].LastIndexOf(">");
				key = presetDataArray[dx].Substring(pFrom, pTo - pFrom);

				pFrom = presetDataArray[dx].IndexOf("(") + "(".Length;
				pTo = presetDataArray[dx].LastIndexOf(")");
				dat = presetDataArray[dx].Substring(pFrom, pTo - pFrom);

				PresetDecode(key,dat);
			}
		}
	}
}




function PresetLoadBuild( fName : String, pName : String ){
	#if !UNITY_WEBPLAYER
	datFile = Resources.Load("SUIMONO_PRESETS_"+fName+"/SUIMONO_PRESET_"+pName) as TextAsset;
	presetDataString = datFile.text;
	presetDataArray = presetDataString.Split("\n"[0]);

		//Decode Data
		for (dx = 0; dx < presetDataArray.Length; dx++){
			if (presetDataArray[dx] != "" && presetDataArray[dx] != "\n"){
				pFrom = presetDataArray[dx].IndexOf("<") + "<".Length;
				pTo = presetDataArray[dx].LastIndexOf(">");
				key = presetDataArray[dx].Substring(pFrom, pTo - pFrom);

				pFrom = presetDataArray[dx].IndexOf("(") + "(".Length;
				pTo = presetDataArray[dx].LastIndexOf(")");
				dat = presetDataArray[dx].Substring(pFrom, pTo - pFrom);

				PresetDecode(key,dat);
			}
		}
	#endif
}




function PresetDecode( key : String, dat : String ){
	dataS = dat.Split(","[0]);

	if (key == "color_depth") depthColor = DecodeColor(dataS);
	if (key == "color_shallow") shallowColor = DecodeColor(dataS);
	if (key == "color_blend") blendColor = DecodeColor(dataS);
	if (key == "color_overlay") overlayColor = DecodeColor(dataS);
	if (key == "color_caustics") causticsColor = DecodeColor(dataS);
	if (key == "color_reflection") reflectionColor = DecodeColor(dataS);
	if (key == "color_specular") specularColor = DecodeColor(dataS);
	if (key == "color_sss") sssColor = DecodeColor(dataS);
	if (key == "color_foam") foamColor = DecodeColor(dataS);
	if (key == "color_underwater") underwaterColor = DecodeColor(dataS);
	if (key == "data_beaufort") beaufortScale = DecodeFloat(dataS);
	if (key == "data_flowdir") flowDirection = DecodeFloat(dataS);
	if (key == "data_flowspeed") flowSpeed = DecodeFloat(dataS);
	if (key == "data_wavescale") waveScale = DecodeFloat(dataS);
	if (key == "data_customwaves") customWaves = DecodeBool(dataS);
	if (key == "data_waveheight") waveHeight = DecodeFloat(dataS);
	if (key == "data_heightprojection") heightProjection = DecodeFloat(dataS);
	if (key == "data_turbulence") turbulenceFactor = DecodeFloat(dataS);
	if (key == "data_lgwaveheight") lgWaveHeight = DecodeFloat(dataS);
	if (key == "data_lgwavescale") lgWaveScale = DecodeFloat(dataS);
	if (key == "data_shorelineheight") shorelineHeight = DecodeFloat(dataS);
	if (key == "data_shorelinefreq") shorelineFreq = DecodeFloat(dataS);
	if (key == "data_shorelinescale") shorelineScale = DecodeFloat(dataS);
	if (key == "data_shorelinespeed") shorelineSpeed = DecodeFloat(dataS);
	if (key == "data_shorelinenorm") shorelineNorm = DecodeFloat(dataS);
	if (key == "data_overallbright") overallBright = DecodeFloat(dataS);
	if (key == "data_overalltransparency") overallTransparency = DecodeFloat(dataS);
	if (key == "data_edgeamt") edgeAmt = DecodeFloat(dataS);
	if (key == "data_depthamt") depthAmt = DecodeFloat(dataS);
	if (key == "data_shallowamt") shallowAmt = DecodeFloat(dataS);
	if (key == "data_refractstrength") refractStrength = DecodeFloat(dataS);
	if (key == "data_aberrationscale") aberrationScale = DecodeFloat(dataS);
	if (key == "data_causticsfade") causticsFade = DecodeFloat(dataS);
	if (key == "data_reflectprojection") reflectProjection = DecodeFloat(dataS);
	if (key == "data_reflectblur") reflectBlur = DecodeFloat(dataS);
	if (key == "data_reflectterm") reflectTerm = DecodeFloat(dataS);
	if (key == "data_roughness") roughness = DecodeFloat(dataS);
	if (key == "data_roughness2") roughness2 = DecodeFloat(dataS);
	if (key == "data_enablefoam") enableFoam = DecodeBool(dataS);
	if (key == "data_foamscale") foamScale = DecodeFloat(dataS);
	if (key == "data_foamspeed") foamSpeed = DecodeFloat(dataS);
	if (key == "data_edgefoamamt") edgeFoamAmt = DecodeFloat(dataS);
	if (key == "data_shallowfoamamt") shallowFoamAmt = DecodeFloat(dataS);
	if (key == "data_heightfoamamt") heightFoamAmt = DecodeFloat(dataS);
	if (key == "data_hfoamheight") hFoamHeight = DecodeFloat(dataS);
	if (key == "data_hfoamspread") hFoamSpread = DecodeFloat(dataS);
	if (key == "data_enableunderdebris") enableUnderDebris = DecodeBool(dataS);
	if (key == "data_underlightfactor") underLightFactor = DecodeFloat(dataS);
	if (key == "data_underrefractionamount") underRefractionAmount = DecodeFloat(dataS);
	if (key == "data_underrefractionscale") underRefractionScale = DecodeFloat(dataS);
	if (key == "data_underrefractionspeed") underRefractionSpeed = DecodeFloat(dataS);
	if (key == "data_underbluramount") underBlurAmount = DecodeFloat(dataS);
	if (key == "data_underwaterfogdist") underwaterFogDist = DecodeFloat(dataS);
	if (key == "data_underwaterfogspread") underwaterFogSpread = DecodeFloat(dataS);
	if (key == "data_underDarkRange") underDarkRange = DecodeFloat(dataS);

}
function DecodeColor(data : String[]) : Color{
	return Color(float.Parse(data[0]),float.Parse(data[1]),float.Parse(data[2]),float.Parse(data[3]));
}
function DecodeFloat(data : String[]) : float{
	return float.Parse(data[0]);
}
function DecodeInt(data : String[]) : int{
	return int.Parse(data[0]);
}
function DecodeBool(data : String[]) : boolean{
	retVal = false;
	if (data[0] == "True") retVal = true;
	return retVal;
}




function PresetEncode( key : String ) : String{
	retData = "";

	if (key == "color_depth") retData = depthColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_shallow") retData = shallowColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_blend") retData = blendColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_overlay") retData = overlayColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_caustics") retData = causticsColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_reflection") retData = reflectionColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_specular") retData = specularColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_sss") retData = sssColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_foam") retData = foamColor.ToString().Substring(4).Replace(" ","");
	if (key == "color_underwater") retData = underwaterColor.ToString().Substring(4).Replace(" ","");
	if (key == "data_beaufort") retData = "("+beaufortScale.ToString().Replace(" ","")+")";
	if (key == "data_flowdir") retData = "("+flowDirection.ToString().Replace(" ","")+")";
	if (key == "data_flowspeed") retData = "("+flowSpeed.ToString().Replace(" ","")+")";
	if (key == "data_wavescale") retData = "("+waveScale.ToString().Replace(" ","")+")";
	if (key == "data_customwaves") retData = "("+customWaves.ToString().Replace(" ","")+")";
	if (key == "data_waveheight") retData = "("+waveHeight.ToString().Replace(" ","")+")";
	if (key == "data_heightprojection") retData = "("+heightProjection.ToString().Replace(" ","")+")";
	if (key == "data_turbulence") retData = "("+turbulenceFactor.ToString().Replace(" ","")+")";
	if (key == "data_lgwaveheight") retData = "("+lgWaveHeight.ToString().Replace(" ","")+")";
	if (key == "data_lgwavescale") retData = "("+lgWaveScale.ToString().Replace(" ","")+")";
	if (key == "data_shorelineheight") retData = "("+shorelineHeight.ToString().Replace(" ","")+")";
	if (key == "data_shorelinefreq") retData = "("+shorelineFreq.ToString().Replace(" ","")+")";
	if (key == "data_shorelinescale") retData = "("+shorelineScale.ToString().Replace(" ","")+")";
	if (key == "data_shorelinespeed") retData = "("+shorelineSpeed.ToString().Replace(" ","")+")";
	if (key == "data_shorelinenorm") retData = "("+shorelineNorm.ToString().Replace(" ","")+")";
	if (key == "data_overallbright") retData = "("+overallBright.ToString().Replace(" ","")+")";
	if (key == "data_overalltransparency") retData = "("+overallTransparency.ToString().Replace(" ","")+")";
	if (key == "data_edgeamt") retData = "("+edgeAmt.ToString().Replace(" ","")+")";
	if (key == "data_depthamt") retData = "("+depthAmt.ToString().Replace(" ","")+")";
	if (key == "data_shallowamt") retData = "("+shallowAmt.ToString().Replace(" ","")+")";
	if (key == "data_refractstrength") retData = "("+refractStrength.ToString().Replace(" ","")+")";
	if (key == "data_aberrationscale") retData = "("+aberrationScale.ToString().Replace(" ","")+")";
	if (key == "data_causticsfade") retData = "("+causticsFade.ToString().Replace(" ","")+")";
	if (key == "data_reflectprojection") retData = "("+reflectProjection.ToString().Replace(" ","")+")";
	if (key == "data_reflectblur") retData = "("+reflectBlur.ToString().Replace(" ","")+")";
	if (key == "data_reflectterm") retData = "("+reflectTerm.ToString().Replace(" ","")+")";
	if (key == "data_roughness") retData = "("+roughness.ToString().Replace(" ","")+")";
	if (key == "data_roughness2") retData = "("+roughness2.ToString().Replace(" ","")+")";
	if (key == "data_enablefoam") retData = "("+enableFoam.ToString().Replace(" ","")+")";
	if (key == "data_foamscale") retData = "("+foamScale.ToString().Replace(" ","")+")";
	if (key == "data_foamspeed") retData = "("+foamSpeed.ToString().Replace(" ","")+")";
	if (key == "data_edgefoamamt") retData = "("+edgeFoamAmt.ToString().Replace(" ","")+")";
	if (key == "data_shallowfoamamt") retData = "("+shallowFoamAmt.ToString().Replace(" ","")+")";
	if (key == "data_heightfoamamt") retData = "("+heightFoamAmt.ToString().Replace(" ","")+")";
	if (key == "data_hfoamheight") retData = "("+hFoamHeight.ToString().Replace(" ","")+")";
	if (key == "data_hfoamspread") retData = "("+hFoamSpread.ToString().Replace(" ","")+")";
	if (key == "data_enableunderdebris") retData = "("+enableUnderDebris.ToString().Replace(" ","")+")";
	if (key == "data_underlightfactor") retData = "("+underLightFactor.ToString().Replace(" ","")+")";
	if (key == "data_underrefractionamount") retData = "("+underRefractionAmount.ToString().Replace(" ","")+")";
	if (key == "data_underrefractionscale") retData = "("+underRefractionScale.ToString().Replace(" ","")+")";
	if (key == "data_underrefractionspeed") retData = "("+underRefractionSpeed.ToString().Replace(" ","")+")";
	if (key == "data_underbluramount") retData = "("+underBlurAmount.ToString().Replace(" ","")+")";
	if (key == "data_underwaterfogdist") retData = "("+underwaterFogDist.ToString().Replace(" ","")+")";
	if (key == "data_underwaterfogspread") retData = "("+underwaterFogSpread.ToString().Replace(" ","")+")";
	if (key == "data_underDarkRange") retData = "("+underDarkRange.ToString().Replace(" ","")+")";


	retData = "<"+key+">" + retData;
	return retData;
}







// ########## OLD PRESET FUNCTIONS ####################################################################################################################


function PresetDoTransition(){
	
	//set final
	if (presetTransitionCurrent >= 1.0){
		//reset
		presetIndex = presetTransIndexTo;
		presetStartTransition = false;
		presetTransitionCurrent = 0.0;
	}	

}




