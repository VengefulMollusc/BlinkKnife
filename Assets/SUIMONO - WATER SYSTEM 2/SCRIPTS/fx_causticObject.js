#pragma strict

private var moduleObject : SuimonoModule;
private var causticObject : fx_causticModule;
private var lightComponent : Light;
//private var surfaceEnabled : float = 1.0;


function Start() {
	
	//get master objects
	moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule);
	causticObject = GameObject.Find("_caustic_effects").GetComponent(fx_causticModule);
	lightComponent = GetComponent(Light);
	
}



function LateUpdate() {

	if (causticObject.enableCaustics){

		//get the current light texture from Module
		lightComponent.cookie = causticObject.useTex;
		lightComponent.cullingMask = moduleObject.causticLayer;
		lightComponent.color = causticObject.causticTint;
		lightComponent.intensity = causticObject.causticIntensity * (1.0-causticObject.heightFac);
		lightComponent.cookieSize = causticObject.causticScale;

		//get scene lighting
		if (causticObject.sceneLightObject != null ){

			//set caustic color based on scene lighting
			if (causticObject.inheritLightColor){
				lightComponent.color = causticObject.sceneLightObject.color * causticObject.causticTint;
				lightComponent.intensity = lightComponent.intensity * causticObject.sceneLightObject.intensity;
			} else {
				lightComponent.color = causticObject.causticTint;
			}

			//set caustic direction based on scene light direction
			if (causticObject.inheritLightDirection){
				transform.eulerAngles = causticObject.sceneLightObject.transform.eulerAngles;
			} else {
				transform.eulerAngles = Vector3(90,0,0);
			}

		}


	}

}
