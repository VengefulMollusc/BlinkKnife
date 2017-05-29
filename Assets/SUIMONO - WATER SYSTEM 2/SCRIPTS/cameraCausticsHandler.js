#pragma strict

@script ExecuteInEditMode()

var isUnderwater : boolean = false;
var causticLight : Light;

enum suiCausToolType{aboveWater,belowWater};
var causticType : suiCausToolType;

private var enableCaustics : boolean = true;
private var moduleObject : SuimonoModule;



function Start () {
	
	//get master object
	moduleObject = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule);

	if (moduleObject != null){
		causticLight = moduleObject.causticObjectLight;
	}
}



function LateUpdate(){
	if (!Application.isPlaying) causticLight.enabled = false;
}



function OnPreCull() {
	if (causticLight != null){

		if (moduleObject != null){
			enableCaustics = moduleObject.enableCaustics;
			//isUnderwater = moduleObject.isUnderwater;
		}
		
		//handle light emission
		if (causticType == suiCausToolType.aboveWater){
			causticLight.enabled = false;
		} else if (causticType == suiCausToolType.belowWater){
			causticLight.enabled = enableCaustics;
		} else {
			causticLight.enabled = false;
		}

		if (isUnderwater) causticLight.enabled = false;

		if (!Application.isPlaying) causticLight.enabled = false;
	}
}




function OnPostRender() {
	if (causticLight != null){

		if (isUnderwater){
			causticLight.enabled = true;
		} else {
			causticLight.enabled = false;
		}
		//handle light emission
		if (causticType == suiCausToolType.belowWater){
			//causticLight.enabled = true;
		//} else if (causticType == suiCausToolType.belowWater){
		//	causticLight.enabled = false;
		//} else {
		//	causticLight.enabled = false;
		}

		if (!Application.isPlaying) causticLight.enabled = false;

	}
}