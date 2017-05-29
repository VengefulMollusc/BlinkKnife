#pragma strict

@script ExecuteInEditMode()


//Underwater Effects variables
var suimonoVersionNumber : String = "";

var systemTime : float = 0.0;

//layers
var layerWater : String;
var layerWaterNum : int = 28;
var layerDepth : String;
var layerDepthNum : int = 29;
var layerScreenFX : String;
var layerScreenFXNum : int = 30;
var layerUnderwater : String;

var layersAreSet : boolean = false;

#if UNITY_EDITOR
	var tagManager : SerializedObject;
	var projectlayers : SerializedProperty;
#endif

var unityVersion : String = "---";

var manualCamera : Transform;
var mainCamera : Transform;
var cameraTypeIndex : int = 0;
var cameraTypeOptions = new Array("Auto Select Camera","Manual Select Camera");
var setCamera : Transform;
var setTrack : Transform;
var setLight : Light;

var enableUnderwaterFX : boolean = true;
var enableInteraction : boolean = true;
var objectEnableUnderwaterFX : float = 1.0;

var enableRefraction : boolean = true;
var enableReflections : boolean = true;
var enableDynamicReflections : boolean = true;
var enableCaustics : boolean = true;
var enableCausticsBlending : boolean = false;
var enableAdvancedEdge : boolean = true;
var enableAdvancedDistort : boolean = true;
var enableTenkoku : boolean = false;
var enableAutoAdvance : boolean = true;

var showPerformance : boolean = false;
var showGeneral : boolean = false;

var underwaterColor : Color = Color(0.58,0.61,0.61,0.0);
var enableTransition : boolean = true;
var transition_offset : float = 0.1;
var fxRippleObject : GameObject;

private var underwaterDebris : ParticleSystem;
private var underLightAmt : float = 0.0;
private var reflectColor : Color;
private var refractAmt : float = 0.0;
private var refractSpd : float = 0.0;

private var targetSurface : GameObject;
private var doTransitionTimer : float = 0.0;
 
static var isUnderwater : boolean = false;
static var doWaterTransition : boolean = false;


//transparency
var enableTransparency : boolean = true;
var transResolution : int = 3;
var transLayer : int = 0;
var transLayerMask : LayerMask;
var causticLayer : int = 0;
var causticLayerMask : LayerMask;
var suiLayerMasks : Array;
var resOptions = new Array("4096","2048","1024","512","256","128","64","32","16","8");
var resolutions = new Array(4096,2048,1024,512,256,128,64,32,16,8);
var transRenderDistance : float = 100;
private var transToolsObject : cameraTools;
private var transCamObject : Camera;
private var causticToolsObject : cameraTools;
private var causticHandlerObjectTrans : cameraCausticsHandler;
private var causticHandlerObject : cameraCausticsHandler;
private var causticCamObject : Camera;
private var wakeObject : GameObject;
private var wakeCamObject : Camera;
private var normalsObject : GameObject;
private var normalsCamObject : Camera;

var playSounds : boolean = true;
var playSoundBelow : boolean = true;
var playSoundAbove : boolean = true;
var maxVolume = 1.0;
var maxSounds = 10;
var defaultSplashSound : AudioClip[];
var soundObject : Transform;

var fxObject : SuimonoModuleFX;

private var setvolume = 0.65;

private var sndparentobj : fx_soundModule;
private var underSoundObject : Transform;
private var underSoundComponent : AudioSource;
private var sndComponents : AudioSource[];
private var currentSound = 0;

var currentObjectIsOver : float = 0.0;
var currentObjectDepth : float = 0.0;
var currentTransitionDepth : float = 0.0;
var currentSurfaceLevel : float = 0.0;
var suimonoObject : SuimonoObject;

private var effectBubbleSystem : ParticleSystem;
private var effectBubbles : ParticleSystem.Particle[];
private var effectBubblesNum : int = 1;

var suimonoModuleLibrary : SuimonoModuleLib;


private var underwaterDebrisRendererComponent : Renderer;
public var setCameraComponent : Camera;

private var underTrans : float = 0.0;

//tenkoku specific variables
public var useTenkoku : float = 0.0;
public var tenkokuWindDir : float = 0.0;
public var tenkokuWindAmt : float = 0.0;
public var tenkokuUseWind : boolean = true;
private var tenObject : GameObject;
private var showTenkoku : boolean = true;
private var tenkokuUseReflect : boolean = true;
private var tenkokuWindModule : WindZone;

//collect for GC
private var lx : int;
private var fx : int;
private var px : int;
private var setParticles : ParticleSystem.Particle[];
private var setstep : AudioClip;
private var setpitch : float;
private var waitTime : float;
private var useSoundAudioComponent : AudioSource;
private var useRefract : float; 
private var useLight : float = 1.0; 
private var useLightCol : Color;	
private var flow_dir : Vector2;
private var tempAngle : Vector3;
private var getmap : Color ;
private var getheight : float;
private var getheightC : float;
private var getheightT : float;
private var getheightR : float;
private var isOverWater : boolean;
private var surfaceLevel : float;
private var groundLevel : float;
private var layer : int;
private var layermask : int;
private var testpos : Vector3;
private var i : int;
private var hit : RaycastHit;
private var pixelUV : Vector2;
private var returnValue : float;
private var returnValueAll : float[];
private var h1 : float;
private var setDegrees : float = 0.0;
private var enabledUFX : float = 1.0;
private var enabledCaustics : float = 1.0;

private var setUnderBright : float;
private var causticObject : fx_causticModule;
private var enTrans : float = 0.0;
private var enCaustic : float = 0.0;
private var setEdge : float = 1.0;
private var underwaterObject : Suimono_UnderwaterFog;
private var currentSurfaceObject : GameObject = null;

var heightValues : float[];
var causticObjectLight : Light;
var isForward : float = 0.0;
var isAdvDist : float = 0.0;


//Height Variables
var waveScale : float = 1.0;
var flowSpeed : float = 0.02;
var offset : float = 0.0;
var heightTex : Texture2D;
var heightTexT : Texture2D;
var heightTexR : Texture2D;
var heightObject : Transform;
var relativePos : Vector2 = Vector2(0,0);
var texCoord : Vector3 = Vector3();
var texCoord1 : Vector3 = Vector3();
var texCoordT : Vector3 = Vector3();
var texCoordT1 : Vector3 = Vector3();
var texCoordR : Vector3 = Vector3();
var texCoordR1 : Vector3 = Vector3();
var heightVal0 : Color;
var heightVal1 : Color;
var heightValT0 : Color;
var heightValT1 : Color;
var heightValR0 : Color;
var heightValR1 : Color;
var localTime : float = 0.0;
private var baseHeight : float = 0.0;
private var baseAngle : float = 0.0;
var pixelArray : Color[];
var pixelArrayT : Color[];
var pixelArrayR : Color[];

var useDecodeTex : Texture2D;
var useDecodeArray : Color[];
var row : int;
var pixIndex : int;
var pixCol : Color;

var t : int;
var y : int;
#if UNITY_EDITOR
	var layerTP : SerializedProperty;
	var layerWP : SerializedProperty;
	var layerSP : SerializedProperty;
	var layerXP : SerializedProperty;
	var layerN : SerializedProperty;
#endif

var dir: Vector3;
var pivotPoint : Vector3;
var useLocalTime : float;
var flow_dirC : Vector2;
var flowSpeed0 : Vector2;
var flowSpeed1 : Vector2;
var flowSpeed2 : Vector2;
var flowSpeed3 : Vector2;
var tScale : float;
var oPos : Vector2;



//Variables for Unity 5.3+ only
#if UNITY_5_3 || UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
	private var debrisEmission : ParticleSystem.EmissionModule;
#endif





function Awake(){

	//###  SET CURRENT SUIMONO NUMBER   ###
	suimonoVersionNumber = "2.1.1";
	
	//Force name
	gameObject.name = "SUIMONO_Module";
}





function Start(){

	//### DISCONNECT FROM PREFAB ###;
	#if UNITY_EDITOR
		PrefabUtility.DisconnectPrefabInstance(this.gameObject);
	#endif
    
    //### SET LAYERS ###;
	InitLayers();

	//get unity version
	unityVersion = Application.unityVersion.ToString().Substring(0,1);
	
	//Set Camera and Track Objects
	Suimono_CheckCamera();

	//SET PHYSICS LAYER INTERACTIONS
	//This is introduced because Unity 5 no longer handles mesh colliders and triggers without throwing an error.
	//thanks a whole lot guys O_o (for nuthin').  The below physics setup should workaround this problem for everyone.
	for (lx = 0; lx < 32; lx++){
		//loop through and decouple layer collisions for all layers(up to 20).
		//layer 4 is the built-in water layer.
		Physics.IgnoreLayerCollision(lx,layerWaterNum);
	}


	//INITIATE OBJECTS
    suimonoModuleLibrary = this.gameObject.GetComponent(SuimonoModuleLib) as SuimonoModuleLib;
    if (this.gameObject.Find("_caustic_effects") != null) causticObject = this.gameObject.Find("_caustic_effects").GetComponent(fx_causticModule);
	if (causticObject != null) causticObjectLight = causticObject.gameObject.Find("mainCausticObject").GetComponent(Light);

	//transparency objects
	transToolsObject = this.transform.Find("cam_SuimonoTrans").gameObject.GetComponent(cameraTools);
	transCamObject = this.transform.Find("cam_SuimonoTrans").gameObject.GetComponent(Camera) as Camera;
	causticHandlerObjectTrans = this.transform.Find("cam_SuimonoTrans").gameObject.GetComponent(cameraCausticsHandler);

	causticToolsObject = this.transform.Find("cam_SuimonoCaustic").gameObject.GetComponent(cameraTools);
	causticCamObject = this.transform.Find("cam_SuimonoCaustic").gameObject.GetComponent(Camera) as Camera;
	causticHandlerObject = this.transform.Find("cam_SuimonoCaustic").gameObject.GetComponent(cameraCausticsHandler);

	//wake advanced effect objects
	wakeObject = this.transform.Find("cam_SuimonoWake").gameObject;
	wakeCamObject = this.transform.Find("cam_SuimonoWake").gameObject.GetComponent(Camera) as Camera;
	normalsObject = this.transform.Find("cam_SuimonoNormals").gameObject;
	normalsCamObject = this.transform.Find("cam_SuimonoNormals").gameObject.GetComponent(Camera) as Camera;

    //Effects Initialization
    fxObject = this.gameObject.GetComponent(SuimonoModuleFX) as SuimonoModuleFX;
	if (this.gameObject.Find("_sound_effects") != null) sndparentobj = this.gameObject.Find("_sound_effects").gameObject.GetComponent(fx_soundModule);
    if (this.gameObject.Find("effect_underwater_debris") != null) underwaterDebris = this.gameObject.Find("effect_underwater_debris").gameObject.GetComponent(ParticleSystem);
    if (this.gameObject.Find("effect_fx_bubbles") != null) effectBubbleSystem = this.gameObject.Find("effect_fx_bubbles").gameObject.GetComponent(ParticleSystem);
       

	//store component references
	underwaterDebrisRendererComponent = underwaterDebris.GetComponent(Renderer);

	//store audio object
	if (suimonoModuleLibrary != null){
		if (suimonoModuleLibrary.soundObject != null){
			soundObject = suimonoModuleLibrary.soundObject;
		}
	}


	#if UNITY_EDITOR
	if (EditorApplication.isPlaying){
	#endif

	if (soundObject != null && sndparentobj != null){
		maxSounds = sndparentobj.maxSounds;
		sndComponents = new AudioSource[maxSounds];

		//init sound object pool
		for (var sx=0; sx < (maxSounds); sx++){
			var soundObjectPrefab = Instantiate(soundObject, transform.position, transform.rotation);
			soundObjectPrefab.transform.parent = sndparentobj.transform;
			sndComponents[sx] = soundObjectPrefab.gameObject.GetComponent(AudioSource);
		}
		
		//init underwater sound
		if(sndparentobj.underwaterSound != null){
			underSoundObject = Instantiate(soundObject, transform.position, transform.rotation);
			underSoundObject.transform.name = "Underwater Sound";
			underSoundObject.transform.parent = sndparentobj.transform;
			underSoundComponent = underSoundObject.gameObject.GetComponent(AudioSource);
		}
	}

	#if UNITY_EDITOR
	}
	#endif


	//tun off antialiasing (causes unexpected rendering issues.  Recommend post fx aliasing instead)
	QualitySettings.antiAliasing = 0;

	//set linear space flag
	if (QualitySettings.activeColorSpace == ColorSpace.Linear){
		Shader.SetGlobalFloat("_Suimono_isLinear",1.0);
	} else {
		Shader.SetGlobalFloat("_Suimono_isLinear",0.0);
	}



	//store pixel arrays for Height Calculation
	if (suimonoModuleLibrary != null){
		if (suimonoModuleLibrary.texHeightC != null){
			heightTex = suimonoModuleLibrary.texHeightC;
			pixelArray = suimonoModuleLibrary.texHeightC.GetPixels(0);
		}
		if (suimonoModuleLibrary.texHeightT != null){
			heightTexT = suimonoModuleLibrary.texHeightT;
			pixelArrayT = suimonoModuleLibrary.texHeightT.GetPixels(0);
		} 
		if (suimonoModuleLibrary.texHeightR != null){
			heightTexR = suimonoModuleLibrary.texHeightR;
			pixelArrayR = suimonoModuleLibrary.texHeightR.GetPixels(0);
		} 
	}


	//set tenkoku flag
	tenObject = GameObject.Find("Tenkoku DynamicSky");
	Shader.SetGlobalFloat("_useTenkoku",0.0);

}





function InitLayers(){


	//check whether layers are set
	#if UNITY_EDITOR
		tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		projectlayers = tagManager.FindProperty("layers");
		layersAreSet = false;
		
		for (t = 8; t <= 31; t++){
	    	layerTP = projectlayers.GetArrayElementAtIndex(t);
	        if (layerTP.stringValue != ""){
	        	if (layerTP.stringValue == "Suimono_Water" || layerTP.stringValue == "Suimono_Depth" || layerTP.stringValue == "Suimono_Screen" || layerTP.stringValue == "Suimono_Underwater"){
	        		layersAreSet = true;
	        	}
	        }
	    }
    #endif




    //Set Layers if Applicable
	if (!layersAreSet){
	#if UNITY_EDITOR
		
        if (projectlayers == null || !projectlayers.isArray){
            Debug.LogWarning("Can't set up Suimono layers.  It's possible the format of the layers and tags data has changed in this version of Unity.");
            Debug.LogWarning("Layers is null: " + (projectlayers == null));
            return;
        }

		layerWater = "Suimono_Water";
		layerDepth = "Suimono_Depth";
		layerScreenFX = "Suimono_Screen";
		layerUnderwater = "Suimono_Underwater";

		//ASSIGN LAYERS
		layerWaterNum = -1;
		layerDepthNum = -1;
		layerScreenFXNum = -1;

		for (y = 8; y <= 31; y++){
        	layerWP = projectlayers.GetArrayElementAtIndex(y);
            if (layerWP.stringValue != layerWater && layerWP.stringValue == "" && layerWaterNum == -1){
            	layerWaterNum = y;
                if (!layersAreSet) Debug.Log("Setting up Suimono layers.  Layer " + layerWaterNum + " is now called " + layerWater);
                layerWP.stringValue = layerWater;
            }
        	layerSP = projectlayers.GetArrayElementAtIndex(y);
            if (layerSP.stringValue != layerDepth && layerWP.stringValue == "" && layerDepthNum == -1){
            	layerDepthNum = y;
                if (!layersAreSet) Debug.Log("Setting up Suimono layers.  Layer " + layerDepthNum + " is now called " + layerDepth);
                layerSP.stringValue = layerDepth;
            }
            layerXP = projectlayers.GetArrayElementAtIndex(y);
            if (layerXP.stringValue != layerScreenFX && layerWP.stringValue == "" && layerScreenFXNum == -1){
            	layerScreenFXNum = y;
                if (!layersAreSet) Debug.Log("Setting up Suimono layers.  Layer " + layerScreenFXNum + " is now called " + layerScreenFX);
                layerXP.stringValue = layerScreenFX;
            }
        }

        if (!layersAreSet) tagManager.ApplyModifiedProperties();

    #endif
    layersAreSet = true;
	}
}








function LateUpdate(){

	//Set Water System Time
	if (systemTime < 0.0) systemTime = 0.0;
	if (enableAutoAdvance) systemTime += Time.deltaTime;


	//set project layer masks
	#if UNITY_EDITOR
		suiLayerMasks = new Array();
		tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
		projectlayers = tagManager.FindProperty("layers");
		for (i = 0; i < projectlayers.arraySize; i++){
			layerN = projectlayers.GetArrayElementAtIndex(i);
			suiLayerMasks.Add(layerN.stringValue);
		}
	#endif


	//GET TENKOKU SPECIFIC VARIABLES
	useTenkoku = 0.0;
	if (tenObject != null){
		if (tenObject.activeInHierarchy){
			useTenkoku = 1.0;
		}

		if (useTenkoku == 1.0){
			if (setLight == null) setLight = GameObject.Find("LIGHT_World").GetComponent(Light);
			if (tenkokuWindModule == null){
				tenkokuWindModule = GameObject.Find("Tenkoku_WindZone").GetComponent(WindZone);
			} else {
				tenkokuWindDir = tenkokuWindModule.transform.eulerAngles.y;
				tenkokuWindAmt = tenkokuWindModule.windMain;
			}
		}
	}
	Shader.SetGlobalFloat("_useTenkoku",useTenkoku);


	//GET RIPPLE OBJECT REFERENCE AND LAYER
	if (Application.isPlaying && fxRippleObject == null){
		fxRippleObject = GameObject.Find("fx_rippleNormals(Clone)");
	}
	if (fxRippleObject != null){
		fxRippleObject.layer = layerScreenFXNum;
	}


	//SET COMPONENT LAYERS
	if (normalsCamObject != null) normalsCamObject.cullingMask = 1<<layerScreenFXNum;
	if (wakeCamObject != null) wakeCamObject.cullingMask = 1<<layerScreenFXNum;



	//HANDLE COMPONENTS

	//Tranparency function
	if (transCamObject != null){
		transLayer = (transLayer & ~(1 << layerWaterNum)); //remove water layer from transparent mask
		transLayer = (transLayer & ~(1 << layerDepthNum)); //remove Depth layer from transparent mask
		transLayer = (transLayer & ~(1 << layerScreenFXNum)); //remove Screen layer from transparent mask

		transCamObject.cullingMask = transLayer; 
		transCamObject.farClipPlane = transRenderDistance;
	} else {
		transCamObject = this.transform.Find("cam_SuimonoTrans").gameObject.GetComponent(Camera) as Camera;
	}

	if (transToolsObject != null){
		transToolsObject.resolution = System.Convert.ToInt32(resolutions[transResolution]);
		if (enableTransparency == false){
			transToolsObject.gameObject.SetActive(false);
		} else {
			transToolsObject.gameObject.SetActive(true);
		}
	} else {
		transToolsObject = this.transform.Find("cam_SuimonoTrans").gameObject.GetComponent(cameraTools);
	}
	



//Caustic function
if (causticCamObject != null){
	if (enableCaustics == false){
		if (!enableCausticsBlending) causticCamObject.gameObject.SetActive(false);
	} else {
		causticCamObject.gameObject.SetActive(enableCausticsBlending);
		transLayer = (transLayer & ~(1 << layerDepthNum)); //remove Depth layer from transparent mask
		transLayer = (transLayer & ~(1 << layerScreenFXNum)); //remove Screen layer from transparent mask
		causticCamObject.cullingMask = transLayer; 
		causticCamObject.farClipPlane = transRenderDistance;
	}

	//remove caustics from transparency function
	causticHandlerObjectTrans.enabled = !enableCausticsBlending;
} else {
	causticCamObject = this.transform.Find("cam_SuimonoCaustic").gameObject.GetComponent(Camera) as Camera;
}


if (causticToolsObject != null){
	causticToolsObject.resolution = System.Convert.ToInt32(resolutions[transResolution]);
} else {
	causticToolsObject = this.transform.Find("cam_SuimonoCaustic").gameObject.GetComponent(cameraTools);
}




	enTrans = 0.0;
	if (enableTransparency) enTrans = 1.0;
	Shader.SetGlobalFloat("_enableTransparency",enTrans);

	//caustics function
	enCaustic = 0.0;
	if (enableCaustics) enCaustic = 1.0;
	Shader.SetGlobalFloat("_suimono_enableCaustic",enCaustic);

	//force suimono layers to caustics casting light
	//(note, this isn't strictly necessary as none of these elements accept caustic lighting, but
	//it's helpful to keep the deferred light occlusion limit more manageable.  These layers don't
	//matter when it comes to lighting, so there's no point in ever having them turned off.
	causticLayer = (causticLayer | (1 << layerWaterNum));
	causticLayer = (causticLayer | (1 << layerDepthNum));
	causticLayer = (causticLayer | (1 << layerScreenFXNum));

	//advanced edge function
	setEdge = 1.0;
	if (!enableAdvancedEdge) setEdge = 0.0;
	Shader.SetGlobalFloat("_suimono_advancedEdge",setEdge);

}






function FixedUpdate () {

	//SET PHYSICS LAYER INTERACTIONS
	//This is introduced because Unity 5 no longer handles mesh colliders and triggers without throwing an error.
	//thanks a whole lot guys O_o (for nuthin').  The below physics setup should workaround this problem for everyone.
	for (lx = 0; lx < 20; lx++){
		//loop through and decouple layer collisions for all layers(up to 20).
		//layer 4 is the built-in water layer.
		Physics.IgnoreLayerCollision(lx,layerWaterNum);
	}


	//Set Camera and Track Objects
	Suimono_CheckCamera();


	//set caustics
	if (causticObject != null){
		
		if (Application.isPlaying){
			causticObject.enableCaustics = enableCaustics;
		} else {
			causticObject.enableCaustics = false;
		}
		

		if (setLight != null){
			causticObject.sceneLightObject = setLight;
		}
	}

	
	//play underwater sounds
	PlayUnderwaterSound();
	
	

	//######## HANDLE FORWARD RENDERING SWITCH #######
	if (setCamera != null){
		isForward = 0.0;
		if (setCameraComponent.actualRenderingPath == RenderingPath.Forward){
			isForward = 1.0;
		}
		Shader.SetGlobalFloat("_isForward",isForward);
	}



	//######## HANDLE ADVANCED DISTORTION SWITCH #######
	if (enableAdvancedDistort){
		isAdvDist = 1.0;
		wakeObject.SetActive(true);
		normalsObject.SetActive(true);
	} else {
		isAdvDist = 0.0;
		wakeObject.SetActive(false);
		normalsObject.SetActive(false);
	}
	Shader.SetGlobalFloat("_suimono_advancedDistort",isAdvDist);



	//######## Set Camera Background Color on Shader #######
	if (setCameraComponent != null){
		if (suimonoObject != null){
			setCameraComponent.backgroundColor = suimonoObject.underwaterColor;
		}
		Shader.SetGlobalColor("_cameraBGColor",setCameraComponent.backgroundColor);
	}

	//######## Set Camera Specific Settings #######
	if (setCameraComponent != null){

		//set camera depth mode to 'Depth'.  The alternative mode
		//'DepthNormals' causes rendering errors in water surfaces
		setCameraComponent.depthTextureMode = DepthTextureMode.Depth;

		//Set Water specific visibility layers on camera
		setCameraComponent.cullingMask = setCameraComponent.cullingMask | (1 <<layerWaterNum);
		setCameraComponent.cullingMask = (setCameraComponent.cullingMask & ~(1 << layerDepthNum) & ~(1 << layerScreenFXNum));
	}


}








//#############################
//	CUSTOM FUNCTIONS
//#############################
function OnDisable(){
	CancelInvoke("StoreSurfaceHeight");
}

function OnEnable(){
	InvokeRepeating("StoreSurfaceHeight",0.01,0.1);
}

function StoreSurfaceHeight(){
	if (this.enabled){
		if (setCamera != null){

			heightValues = SuimonoGetHeightAll(setCamera.transform.position);
			currentSurfaceLevel = heightValues[1];
			currentObjectDepth = heightValues[3];
			currentObjectIsOver = heightValues[4];
			currentTransitionDepth = heightValues[9];
			objectEnableUnderwaterFX = heightValues[10];

			checkUnderwaterEffects();
			checkWaterTransition();
		}
	}
}





function PlayUnderwaterSound(){
if (Application.isPlaying){
	if (underSoundObject != null && setTrack != null && underSoundComponent != null){
		underSoundObject.transform.position = setTrack.transform.position;

		if (currentTransitionDepth > 0.0){
			if (playSoundBelow && playSounds){
				underSoundComponent.clip = sndparentobj.underwaterSound;
				underSoundComponent.volume = maxVolume;
				underSoundComponent.loop = true;
				if (!underSoundComponent.isPlaying){
					underSoundComponent.Play();
				}
			} else {
				underSoundComponent.Stop();
			}
		} else {
			if (sndparentobj.underwaterSound != null){
				if (playSoundAbove && playSounds){
					underSoundComponent.clip = sndparentobj.abovewaterSound;
					underSoundComponent.volume = 0.45*maxVolume;
					underSoundComponent.loop = true;
					if (!underSoundComponent.isPlaying){
						underSoundComponent.Play();
					}
				} else {
					underSoundComponent.Stop();
				}
			}
		}
	}
}
}




function AddFX(fxSystem : int, effectPos : Vector3, addRate : int, addSize : float, addRot : float, addARot : float, addVeloc : Vector3, addCol : Color){
	if (fxObject != null){
		fx = fxSystem;

		if (fxObject.fxParticles[fx] != null){

			fxObject.fxParticles[fx].Emit(addRate);
			//get particles
			if (setParticles != null) setParticles = null;
			setParticles = new ParticleSystem.Particle[fxObject.fxParticles[fx].particleCount];
			fxObject.fxParticles[fx].GetParticles(setParticles);
			//set particles
			if (fxObject.fxParticles[fx].particleCount > 0.0){
			for (px = (fxObject.fxParticles[fx].particleCount-addRate); px < fxObject.fxParticles[fx].particleCount; px++){
					
					//set position
					setParticles[px].position.x = effectPos.x;
					setParticles[px].position.y = effectPos.y;
					setParticles[px].position.z = effectPos.z;
					
					//set variables
					#if UNITY_5_3 || UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
						setParticles[px].startSize = addSize;
					#else
						setParticles[px].size = addSize;
					#endif

					setParticles[px].rotation = addRot;
					setParticles[px].angularVelocity = addARot;
					
					setParticles[px].velocity.x = addVeloc.x;
					setParticles[px].velocity.y = addVeloc.y;
					setParticles[px].velocity.z = addVeloc.z;
					
					
					#if UNITY_5_3 || UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
						setParticles[px].startColor *= addCol;
					#else
						setParticles[px].color *= addCol;
					#endif

			}
			fxObject.fxParticles[fx].SetParticles(setParticles,setParticles.length);
			fxObject.fxParticles[fx].Play();
			}

		}
	}
}




function AddSoundFX(sndClip : AudioClip, soundPos : Vector3, sndVelocity:Vector3){

	setpitch = 1.0;
	waitTime = 0.4;
	setvolume = 1.0;
	
	if (playSounds && sndparentobj.defaultSplashSound.length >= 1 ){
		setstep = sndparentobj.defaultSplashSound[Random.Range(0,sndparentobj.defaultSplashSound.length-1)];
		waitTime = 0.4;
		setpitch = sndVelocity.y;
		setvolume = sndVelocity.z;
		setvolume = Mathf.Lerp(0.0,1.0,setvolume) * maxVolume;

		//check depth and morph sounds if underwater
		if (currentObjectDepth > 0.0){
			setpitch *=0.25;
			setvolume *=0.5;
		}
		
		useSoundAudioComponent = sndComponents[currentSound];
		useSoundAudioComponent.clip = sndClip;
		if (!useSoundAudioComponent.isPlaying){
			useSoundAudioComponent.transform.position = soundPos;
			useSoundAudioComponent.volume = setvolume;
			useSoundAudioComponent.pitch = setpitch;
			useSoundAudioComponent.minDistance = 4.0;
			useSoundAudioComponent.maxDistance = 20.0;
			useSoundAudioComponent.clip = setstep;
			useSoundAudioComponent.loop = false;
			useSoundAudioComponent.Play();
		}

		currentSound += 1;
		if (currentSound >= (maxSounds-1)) currentSound = 0;
	}

}






function AddSound(sndMode : String, soundPos : Vector3, sndVelocity:Vector3){

if (enableInteraction){

	setpitch = 1.0;
	waitTime = 0.4;
	setvolume = 1.0;
	
	if (playSounds && sndparentobj.defaultSplashSound.length >= 1 ){
		setstep = sndparentobj.defaultSplashSound[Random.Range(0,sndparentobj.defaultSplashSound.length-1)];
		waitTime = 0.4;
		setpitch = sndVelocity.y;
		setvolume = sndVelocity.z;
		setvolume = Mathf.Lerp(0.0,10.0,setvolume);

		//check depth and morph sounds if underwater
		if (currentObjectDepth > 0.0){
			setpitch *=0.25;
			setvolume *=0.5;
		}
		
		useSoundAudioComponent = sndComponents[currentSound];
		if (!useSoundAudioComponent.isPlaying){
			useSoundAudioComponent.transform.position = soundPos;
			useSoundAudioComponent.volume = setvolume;
			useSoundAudioComponent.pitch = setpitch;
			useSoundAudioComponent.minDistance = 4.0;
			useSoundAudioComponent.maxDistance = 20.0;
			useSoundAudioComponent.clip = setstep;
			useSoundAudioComponent.loop = false;
			useSoundAudioComponent.Play();
		}

		currentSound += 1;
		if (currentSound >= (maxSounds-1)) currentSound = 0;
	}
}
}








function checkUnderwaterEffects(){

	if (Application.isPlaying){
		if (currentTransitionDepth > 0.1){
		
			if (enableUnderwaterFX && objectEnableUnderwaterFX==1.0 && currentObjectIsOver==1.0){
				isUnderwater = true;
				Shader.SetGlobalFloat("_Suimono_IsUnderwater",1.0);
				if (suimonoObject != null){
					suimonoObject.useShader = suimonoObject.shader_Under;
				}
				if (causticHandlerObject != null){
					causticHandlerObjectTrans.isUnderwater = true;
					causticHandlerObject.isUnderwater = true;
				}
			}
			
		} else {
			//swap camera rendering to back to default
			isUnderwater = false;
	   		Shader.SetGlobalFloat("_Suimono_IsUnderwater",0.0);
			if (suimonoObject != null){
				suimonoObject.useShader = suimonoObject.shader_Surface;
			}
			if (causticHandlerObject != null){
				causticHandlerObjectTrans.isUnderwater = false;
				causticHandlerObject.isUnderwater = false;
			}
		}
	}
}








function checkWaterTransition () {
if (Application.isPlaying){

	doTransitionTimer += Time.deltaTime;
	
	//SET COLORS
	reflectColor = Color(0.827,0.941,1.0,1.0);


		if (currentTransitionDepth > 0.1 && currentObjectIsOver==1.0){
			
			doWaterTransition = true;

		    //set underwater debris
		    if (suimonoObject != null && setCamera != null){

				if (enableUnderwaterFX && objectEnableUnderwaterFX==1.0){

					if (suimonoObject.enableUnderDebris){
			       		underwaterDebris.transform.position = setCamera.transform.position;
					    underwaterDebris.transform.rotation = setCamera.transform.rotation;
					    underwaterDebris.transform.Translate(Vector3.forward * 5.0);

						underwaterDebrisRendererComponent.enabled=true;

						#if UNITY_5_3 || UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
							debrisEmission = underwaterDebris.emission;
							debrisEmission.enabled = true;
						#else
							underwaterDebris.enableEmission=true;
						#endif

						underwaterDebris.Play();
					} else {
						if (underwaterDebris != null) underwaterDebrisRendererComponent.enabled = false;
					}

					setUnderBright = underLightAmt;
					setUnderBright *= 0.5;


			       	//set attributes to shader
			       	useLight = 1.0;
			       	useLightCol = Color(1,1,1,1);
			       	useRefract = 1.0;
			       	if (setLight != null){
			       		useLight = setLight.intensity;
			       		useLightCol = setLight.color;
			       	}
			       	if (!enableRefraction) useRefract = 0.0;


					if (underwaterObject == null){
						if (setCamera.gameObject.GetComponent(Suimono_UnderwaterFog) != null){
							underwaterObject = setCamera.gameObject.GetComponent(Suimono_UnderwaterFog);
						}
					}
					if (underwaterObject != null){
						underwaterObject.lightFactor = suimonoObject.underLightFactor * useLight;
						underwaterObject.refractAmt = suimonoObject.underRefractionAmount * useRefract;
						underwaterObject.refractScale = suimonoObject.underRefractionScale;
						underwaterObject.refractSpd = suimonoObject.underRefractionSpeed * useRefract;
						underwaterObject.fogEnd = suimonoObject.underwaterFogDist;
						underwaterObject.fogStart = suimonoObject.underwaterFogSpread;
						underwaterObject.blurSpread = suimonoObject.underBlurAmount;
						underwaterObject.underwaterColor = suimonoObject.underwaterColor;
						underwaterObject.darkRange = suimonoObject.underDarkRange;

						Shader.SetGlobalColor("_suimono_lightColor",useLightCol);
						underwaterObject.doTransition = false;

						//set caustic and underwater light brightness
						if (causticObject != null){
							
							if (Application.isPlaying){

								if (causticObject != null){
									causticObject.heightFac = underwaterObject.hFac*2.0;
								}
								//if (setLight != null){
									//if (useTenkoku == 1.0){
										//tenObject
										//setLight.intensity *= Mathf.Clamp(1.0-underwaterObject.hFac,1.0,0.1);
									//}
								//}
							}
						}
					}


				} else {
					if (underwaterDebris != null) underwaterDebrisRendererComponent.enabled = false;
				}
			}


			if (underwaterObject != null){
				underwaterObject.cancelTransition = true;
			}


	    } else {

	        //reset underwater debris
	        if (underwaterDebris != null){
	       		underwaterDebrisRendererComponent.enabled=false;
	       	}

	     	//show water transition
	     	if (enableTransition){
	     	if (doWaterTransition && setCamera != null){
	     		
	     		doTransitionTimer = 0.0;

	      		if (underwaterObject != null){
					underwaterObject.doTransition = true;
				}

	       		doWaterTransition = false;
	
		       		
	     	} else {
	     		
	     		underTrans = 1.0;

	     	}
	       	}
	    }

    
	    if (!enableUnderwaterFX){
	    	if (underwaterDebrisRendererComponent != null){
				underwaterDebrisRendererComponent.enabled=false;
			}
	    }

}
}









function Suimono_CheckCamera(){




	//get main camera object
	if (cameraTypeIndex == 0){
		if (Camera.main != null){
			mainCamera = Camera.main.transform;
		}
		manualCamera = null;
	}


	if (cameraTypeIndex == 1){
		if (manualCamera != null){
			mainCamera = manualCamera;
		} else {
			if (Camera.main != null){
				mainCamera = Camera.main.transform;
			}
		}
	}
	//if (setCamera != mainCamera){ 
		//update camera reference flag
		//setCamera = mainCamera;
	//}




	//set camera
	if (setCamera != mainCamera){
		//if (Camera.main != null){
		//if (Camera.main.transform != null){
			setCamera = mainCamera;
			//setCamera = Camera.main.transform;
			setCameraComponent = setCamera.gameObject.GetComponent(Camera);
			underwaterObject = setCamera.gameObject.GetComponent(Suimono_UnderwaterFog);

		//}
		//}
	}

	//set camera component
	if (setCameraComponent == null && setCamera != null){
			setCameraComponent = setCamera.gameObject.GetComponent(Camera);
	}

	//reset camera component
	if (setCamera != null && setCameraComponent != null){
		if (setCameraComponent.transform != setCamera){
			setCameraComponent = setCamera.gameObject.GetComponent(Camera);
			underwaterObject = setCamera.gameObject.GetComponent(Suimono_UnderwaterFog);
		}
	}
	
	//set track object
	if (setTrack == null && setCamera != null){
		setTrack = setCamera.transform;
	}

	//install camera effects
	InstallCameraEffect();

}




function SuimonoConvertAngleToDegrees(convertAngle : float) : Vector2{

	flow_dir = Vector3(0,0);
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



function SuimonoConvertAngleToVector(convertAngle : float) : Vector2{
	//Note this is the same function as above, but renamed for better clarity.
	//eventually the above function should be deprecated.
	flow_dir = Vector3(0,0);
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







function SuimonoGetHeight(testObject : Vector3, returnMode : String) : float {

	// Get Heights
	CalculateHeights(testObject);

	// Return values
	returnValue = 0.0;
	
	if (returnMode == "height") returnValue = getheight;
	if (returnMode == "surfaceLevel") returnValue = surfaceLevel+getheight;
	if (returnMode == "baseLevel") returnValue = surfaceLevel;
	if (returnMode == "object depth") returnValue = getheight-testObject.y;
	if (returnMode == "isOverWater" && isOverWater) returnValue = 1.0;
	if (returnMode == "isOverWater" && !isOverWater) returnValue = 0.0;
	
	if (returnMode == "isAtSurface"){
		if (((surfaceLevel+getheight)-testObject.y) < 0.25 && ((surfaceLevel+getheight)-testObject.y) > -0.25)
			returnValue = 1.0;
	}
	
	if (suimonoObject != null){
		if (returnMode == "direction") returnValue = suimonoObject.flowDirection;
		if (returnMode == "speed") returnValue = suimonoObject.flowSpeed;
			
		if (returnMode == "wave height"){
			h1 = 0.0;
			returnValue = getheight/h1;
		}
	}

	if (returnMode == "transitionDepth") returnValue = ((surfaceLevel+getheight)-(testObject.y-(transition_offset*underTrans)));


	if (returnMode == "underwaterEnabled"){
		enabledUFX = 1;
		if (!suimonoObject.enableUnderwaterFX) enabledUFX = 0;
		returnValue = enabledUFX;
	}

	if (returnMode == "causticsEnabled"){
		enabledCaustics = 1;
		if (!suimonoObject.enableCausticFX) enabledCaustics = 0;
		returnValue = enabledCaustics;
	}


	return returnValue;

	
}







function SuimonoGetHeightAll(testObject : Vector3) : float[] {

	// Get Heights
	CalculateHeights(testObject);

	// Return values
	returnValueAll = new float[12];
	
	// 0 height
	returnValueAll[0]=(getheight);
	
	// 1 surface level
	returnValueAll[1]=(getheight);

	// 2 base level
	returnValueAll[2]=(surfaceLevel);
	
	// 3 object depth
	returnValueAll[3]=((getheight)-testObject.y);

	// 4 is Over Water
	if (isOverWater) returnValue = 1.0;
	if (!isOverWater) returnValue = 0.0;
	returnValueAll[4]=(returnValue);
	
	// 5 is at surface
	returnValue = 0.0;
	if (((getheight)-testObject.y) < 0.25 && ((getheight)-testObject.y) > -0.25) returnValue = 1.0;
	returnValueAll[5]=(returnValue);
	
	
	// 6 direction
	if (suimonoObject != null){
		setDegrees = suimonoObject.flowDirection + suimonoObject.transform.eulerAngles.y;
		if (setDegrees < 0.0) setDegrees = 365.0 + setDegrees;
		if (setDegrees > 365.0) setDegrees = setDegrees-365.0;
		if (suimonoObject != null) returnValueAll[6]= setDegrees;
		if (suimonoObject == null) returnValueAll[6]= 0.0;
		
		// 7 speed
		if (suimonoObject != null) returnValueAll[7]=(suimonoObject.flowSpeed);
		if (suimonoObject == null) returnValueAll[7]=0.0;
		
		// 8 wave height
		if (suimonoObject != null) h1 = (suimonoObject.lgWaveHeight);
		if (suimonoObject == null) h1 = 0.0;
		returnValueAll[8]=(getheight/h1);
	}
	
	// 9 transition depth
	returnValueAll[9] = ((getheight)-(testObject.y-(transition_offset*underTrans)));

	// 10 enabled Underwater FX
	enabledUFX = 1;
	if (suimonoObject != null){
		if (!suimonoObject.enableUnderwaterFX) enabledUFX = 0;
		returnValueAll[10] = enabledUFX;
	}
	// 11 enabled Underwater FX
	enabledCaustics = 1;
	if (suimonoObject != null){
		if (!suimonoObject.enableCausticFX) enabledCaustics = 0;
		returnValueAll[11] = enabledCaustics;
	}
	
	return returnValueAll;

}








 function RotatePointAroundPivot(point: Vector3, pivot: Vector3, angles: Vector3): Vector3 {
   dir = point - pivot;
   dir = Quaternion.Euler(angles * -1) * dir;
   point = dir + pivot;
   return point;
 }




function DecodeHeightPixels(texPosx : float, texPosy : float, texNum : int) : Color{

	if (texNum == 0){
		useDecodeTex = heightTex;
		useDecodeArray = pixelArray;
	}
	if (texNum == 1){
		useDecodeTex = heightTexT;
		useDecodeArray = pixelArrayT;
	}
	if (texNum == 2){
		useDecodeTex = heightTexR;
		useDecodeArray = pixelArrayR;
	}

	texPosx = (texPosx % useDecodeTex.width);
	texPosy = (texPosy % useDecodeTex.height);
	if (texPosx < 0) texPosx = useDecodeTex.width - Mathf.Abs(texPosx);
	if (texPosy < 0) texPosy = useDecodeTex.height - Mathf.Abs(texPosy);
	if (texPosx > useDecodeTex.width) texPosx = texPosx - useDecodeTex.width;
	if (texPosy > useDecodeTex.height) texPosy = texPosy - useDecodeTex.height;

	row = (useDecodeArray.length/useDecodeTex.height) - Mathf.FloorToInt(texPosy);
	pixIndex = ((Mathf.FloorToInt(texPosx) + 1) + (useDecodeArray.length - (useDecodeTex.width * row))) - 1;
	if (pixIndex > useDecodeArray.length) pixIndex = pixIndex - (useDecodeArray.length);
	if (pixIndex < 0) pixIndex = useDecodeArray.length - pixIndex;

	pixCol = useDecodeArray[pixIndex];


	if (QualitySettings.activeColorSpace == ColorSpace.Linear){
		pixCol.r = Mathf.GammaToLinearSpace(pixCol.r);
	}
	if (QualitySettings.activeColorSpace == ColorSpace.Gamma){
		pixCol.r = pixCol.r * 0.464646;
	}



	return pixCol;
}





function CalculateHeights(testObject : Vector3){

	getmap = Color(0.0,0.0,0.0,0.0);
	getheight = -1.0;
	getheightC = -1.0;
	getheightT = -1.0;
	getheightR = 0.0;
	isOverWater = false;
	surfaceLevel = -1.0;
	groundLevel = 0.0;

	layermask = 1 <<layerWaterNum;
	testpos = Vector3(testObject.x,testObject.y+5000,testObject.z);

	if(Physics.Raycast(testpos, -Vector3.up,hit,10000,layermask)){

		targetSurface = hit.transform.gameObject;
		if (currentSurfaceObject != targetSurface || suimonoObject == null){
			currentSurfaceObject = targetSurface;
			suimonoObject = hit.transform.parent.gameObject.GetComponent(SuimonoObject);
		}

		if (suimonoObject.typeIndex == 0){
			heightObject = hit.transform;
		} else {
			heightObject = hit.transform.parent;
		}

		if (suimonoObject != null && hit.collider != null){

			isOverWater = true;
			surfaceLevel = heightObject.position.y;//hit.point.y;



			//calculate relative position
			if (heightObject != null){
				baseHeight = heightObject.position.y;
				baseAngle = heightObject.rotation.y;
				relativePos.x = ((heightObject.position.x - testObject.x)/(20.0*heightObject.localScale.x) + 1) * 0.5 * heightObject.localScale.x;
				relativePos.y = ((heightObject.position.z - testObject.z)/(20.0*heightObject.localScale.z) + 1) * 0.5 * heightObject.localScale.z;
			}



			//calculate offset
			useLocalTime = suimonoObject.localTime;
			flow_dirC = SuimonoConvertAngleToVector(suimonoObject.flowDirection);
			flowSpeed0 = Vector2(flow_dirC.x*useLocalTime,flow_dirC.y*useLocalTime);
			flowSpeed1 = Vector2(flow_dirC.x*useLocalTime*0.25,flow_dirC.y*useLocalTime*0.25);
			flowSpeed2 = Vector2(flow_dirC.x*useLocalTime*0.0625,flow_dirC.y*useLocalTime*0.0625);
			flowSpeed3 = Vector2(flow_dirC.x*useLocalTime*0.125,flow_dirC.y*useLocalTime*0.125);
			tScale = (1.0/(suimonoObject.waveScale));
			oPos = Vector2(0.0-suimonoObject.savePos.x,0.0-suimonoObject.savePos.y);

			//calculate texture coordinates
			if (heightTex != null){

				texCoord.x = (relativePos.x * tScale + flowSpeed0.x + (oPos.x)) * heightTex.width;
				texCoord.z = (relativePos.y * tScale + flowSpeed0.y + (oPos.y)) * heightTex.height;
				texCoord1.x = ((relativePos.x * tScale * 0.75) - flowSpeed1.x + (oPos.x*0.75)) * heightTex.width;
				texCoord1.z = ((relativePos.y * tScale * 0.75) - flowSpeed1.y + (oPos.y*0.75)) * heightTex.height;

				texCoordT.x = (relativePos.x * tScale + flowSpeed0.x + (oPos.x)) * heightTexT.width;
				texCoordT.z = (relativePos.y * tScale + flowSpeed0.y + (oPos.y)) * heightTexT.height;
				texCoordT1.x = ((relativePos.x * tScale * 0.5) - flowSpeed1.x + (oPos.x*0.5)) * heightTexT.width;
				texCoordT1.z = ((relativePos.y * tScale * 0.5) - flowSpeed1.y + (oPos.y*0.5)) * heightTexT.height;

				texCoordR.x = (relativePos.x * suimonoObject.lgWaveScale * tScale + flowSpeed2.x + (oPos.x*suimonoObject.lgWaveScale)) * heightTexR.width;
				texCoordR.z = (relativePos.y * suimonoObject.lgWaveScale * tScale + flowSpeed2.y + (oPos.y*suimonoObject.lgWaveScale)) * heightTexR.height;
				texCoordR1.x = ((relativePos.x * suimonoObject.lgWaveScale * tScale) + flowSpeed3.x + (oPos.x*suimonoObject.lgWaveScale)) * heightTexR.width;
				texCoordR1.z = ((relativePos.y * suimonoObject.lgWaveScale * tScale ) + flowSpeed3.y + (oPos.y*suimonoObject.lgWaveScale)) * heightTexR.height;


				//rotate coordinates
				if (baseAngle != 0.0){
		    	
		    		pivotPoint = Vector3(heightTex.width*heightObject.localScale.x*tScale*0.5+(flowSpeed0.x*heightTex.width),0,heightTex.height*heightObject.localScale.z*tScale*0.5+(flowSpeed0.y*heightTex.height));
		    		texCoord = RotatePointAroundPivot(texCoord,pivotPoint,heightObject.eulerAngles);
		    		pivotPoint = Vector3(heightTex.width*heightObject.localScale.x*tScale*0.5*0.75-(flowSpeed1.x*heightTex.width),0,heightTex.height*heightObject.localScale.z*tScale*0.5*0.75-(flowSpeed1.y*heightTex.height));
		    		texCoord1 = RotatePointAroundPivot(texCoord1,pivotPoint,heightObject.eulerAngles);

		    		pivotPoint = Vector3(heightTexT.width*heightObject.localScale.x*tScale*0.5+(flowSpeed0.x*heightTexT.width),0,heightTexT.height*heightObject.localScale.z*tScale*0.5+(flowSpeed0.y*heightTexT.height));
		    		texCoordT = RotatePointAroundPivot(texCoordT,pivotPoint,heightObject.eulerAngles);
		    		pivotPoint = Vector3(heightTexT.width*heightObject.localScale.x*tScale*0.5*0.5-(flowSpeed1.x*heightTexT.width),0,heightTexT.height*heightObject.localScale.z*tScale*0.5*0.5-(flowSpeed1.y*heightTexT.height));
		    		texCoordT1 = RotatePointAroundPivot(texCoordT1,pivotPoint,heightObject.eulerAngles);

			    	pivotPoint = Vector3(heightTexR.width*heightObject.localScale.x*suimonoObject.lgWaveScale*tScale*0.5+(flowSpeed2.x*heightTexR.width),0,heightTexR.height*heightObject.localScale.z*suimonoObject.lgWaveScale*tScale*0.5+(flowSpeed2.y*heightTexR.height));
		    		texCoordR = RotatePointAroundPivot(texCoordR,pivotPoint,heightObject.eulerAngles);
		    		pivotPoint = Vector3(heightTexR.width*heightObject.localScale.x*suimonoObject.lgWaveScale*tScale*0.5+(flowSpeed3.x*heightTexR.width),0,heightTexR.height*heightObject.localScale.z*suimonoObject.lgWaveScale*tScale*0.5+(flowSpeed3.y*heightTexR.height));
		    		texCoordR1 = RotatePointAroundPivot(texCoordR1,pivotPoint,heightObject.eulerAngles);
		    	}

				//decode height value
				heightVal0 = DecodeHeightPixels(texCoord.x,texCoord.z,0);
				heightVal1 = DecodeHeightPixels(texCoord1.x,texCoord1.z,0);
				heightValT0 = DecodeHeightPixels(texCoordT.x,texCoordT.z,1);
				heightValT1 = DecodeHeightPixels(texCoordT1.x,texCoordT1.z,1);
				heightValR0 = DecodeHeightPixels(texCoordR.x,texCoordR.z,2);
				heightValR1 = DecodeHeightPixels(texCoordR1.x,texCoordR1.z,2);

				//set heightvalue
				getheightC = (heightVal0.r + heightVal1.r) * 0.8;
				getheightT = ((heightValT0.r*0.2) + (heightValT0.r * heightValT1.r * 0.8)) * suimonoObject.turbulenceFactor * 0.5;
				getheightR = ((heightValR0.r * 4.0) + (heightValR1.r * 3.0));

				getheight = baseHeight + (getheightC * suimonoObject.waveHeight);
				getheight += (getheightT * suimonoObject.waveHeight);
				getheight += (getheightR * suimonoObject.lgWaveHeight);
				getheight = Mathf.Lerp(baseHeight,getheight,suimonoObject.useHeightProjection);
			}

		}
	}
}



function InstallCameraEffect(){

	//Installs Camera effect if it doesn't already exist.
	if (setCameraComponent != null){
		if (setCameraComponent.gameObject.GetComponent(Suimono_UnderwaterFog) != null){
			//do nothing
		} else {
			setCameraComponent.gameObject.AddComponent(Suimono_UnderwaterFog);
		}
	}
}