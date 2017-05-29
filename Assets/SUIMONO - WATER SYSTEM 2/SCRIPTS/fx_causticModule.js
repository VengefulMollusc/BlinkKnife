#pragma strict
#pragma implicit
#pragma downcast

//PUBLIC VARIABLES
var enableCaustics : boolean = true;
var sceneLightObject : Light;
var inheritLightColor : boolean = false;
var inheritLightDirection : boolean = false;
var causticTint : Color = Color(1,1,1,1);
var causticIntensity : float = 2.0;
var causticScale : float = 4.0;
var heightFac : float = 0.0;
var causticFPS : int = 32;
var causticFrames : Texture2D[];

//PUBLIC VARIABLES
public var useTex : Texture2D;

//PRIVATE VARIABLES
private var causticsTime : float = 0.0;
private var moduleObject : SuimonoModule;
private var lightObject : GameObject;
private var frameIndex : int = 0;



function Start(){

	//get master objects
	moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule);
	lightObject = transform.Find("mainCausticObject").gameObject;
}



function LateUpdate() {
	
	if (this.enabled){
		
  		useTex = causticFrames[frameIndex];

    	causticsTime += Time.deltaTime;
    	if (causticsTime > (1.0/(causticFPS*1.0))){
    		causticsTime = 0.0;
    		frameIndex += 1;
    	}

    	if (frameIndex == causticFrames.length) frameIndex = 0;

    	if (moduleObject != null){
    		if (moduleObject.setLight != null){
    			sceneLightObject = moduleObject.setLight;
    		}

    		if (lightObject != null){
    			lightObject.SetActive(moduleObject.enableCaustics);
    		}
    	}

    }

}
