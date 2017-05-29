#pragma strict

//PUBLIC VARIABLES
var applyToParent : boolean = false;
var engageBuoyancy : boolean = false;
var activationRange : float = 500.0;
var inheritForce : boolean = false;
var keepAtSurface : boolean = false;
var buoyancyOffset : float = 0.0;
var buoyancyStrength : float = 1.0;
var forceAmount : float = 1.0;
var forceHeightFactor : float = 0.0;

// PRIVATE VARIABLES
private var maxVerticalSpeed: float = 5.0;
private var surfaceRange : float = 0.2;
private var buoyancy : float = 0.0;
private var isUnder : boolean = false;
private var surfaceLevel : float = 0.0;
private var underwaterLevel : float = 0.0;
private var isUnderwater : boolean = false;
private var physTarget : Transform;
private var moduleObject : SuimonoModule;
private var height : float = -1;
private var waveHeight : float = 0.0;
private var modTime : float = 0.0;
private var splitFac : float = 1.0;
private var rigidbodyComponent : Rigidbody;
private var isOver : float = 0.0;
private var forceAngles : Vector2 = Vector2(0.0,0.0);
private var forceSpeed : float = 0.0;
private var waveHt : float = 0.0;

private var displace : float;

//collect for GC
private var gizPos : Vector3;
private var testObjectHeight : float;
private var buoyancyFactor : float;
private var forceMod : float;
private var waveFac : float;
private var heightValues : float[];
private var isEnabled = true;
private var performHeight : boolean = false;
private var currRange : float = -1.0;
private var camRange : float = -1.0;
private var currCamPos : Vector3 = Vector3(-1,-1,-1);



function OnDrawGizmos (){
	gizPos = transform.position;
	gizPos.y += 0.03;
	Gizmos.DrawIcon(gizPos, "gui_icon_buoy.psd", true);
	gizPos.y -= 0.03;
}


function Start(){

	if (GameObject.Find("SUIMONO_Module") != null){
		moduleObject = GameObject.Find("SUIMONO_Module").gameObject.GetComponent(SuimonoModule);
	}

	//get number of buoyant objects
	if (applyToParent){
		var buoyancyObjects : Component[];
		buoyancyObjects = transform.parent.gameObject.GetComponentsInChildren(fx_buoyancy);
		if (buoyancyObjects != null){
			splitFac = 1.0/buoyancyObjects.Length;
		}
	}

		//set physics target
	if (applyToParent){
		physTarget = this.transform.parent.transform;
		if (physTarget != null){
		if (rigidbodyComponent == null){
			rigidbodyComponent = physTarget.GetComponent(Rigidbody);
		}}
	} else {
		physTarget = this.transform;
		if (physTarget != null){
		if (rigidbodyComponent == null){
			rigidbodyComponent = GetComponent(Rigidbody);
		}}
	}
}


function FixedUpdate(){

	SetUpdate();

	displace = Mathf.Clamp(surfaceLevel - (transform.position.y+buoyancyOffset-0.5),0.0,5.0);
	maxVerticalSpeed = displace;
	buoyancy = 1 + (displace * buoyancyStrength);

}




function SetUpdate () {
if (moduleObject != null){

	//check activations
	performHeight = true;
	if (physTarget != null && moduleObject.setCamera != null){
	
		//check for range activation
		currRange = Vector3.Distance(moduleObject.setCamera.transform.position, physTarget.transform.position);
		if (currRange >= activationRange){
			performHeight = false;
		}
		
		//check for frustrum activation
		camRange = 0.2;
		if (moduleObject != null){
		if (moduleObject.setCameraComponent != null){
			currCamPos = moduleObject.setCameraComponent.WorldToViewportPoint(physTarget.transform.position);
			if (currCamPos.x > (1.0+camRange) || currCamPos.y > (1.0+camRange)){
				performHeight = false;
			}
			if (currCamPos.x < (0.0-camRange) || currCamPos.y < (0.0-camRange)){
				performHeight = false;
			}
		}
		}
		//check for enable activation
		if (!isEnabled){
			performHeight = false;
		}
	}
	

	//perform height check
	if (performHeight){
		// Get all height variables from Suimono Module object
		heightValues = moduleObject.SuimonoGetHeightAll(this.transform.position);
		isOver = heightValues[4];
		waveHt = heightValues[8];
		surfaceLevel = heightValues[0];
		forceAngles = moduleObject.SuimonoConvertAngleToDegrees(heightValues[6]);
		forceSpeed = heightValues[7]*0.1;
	}

	//clamp variables
	forceHeightFactor = Mathf.Clamp(forceHeightFactor,0.0,1.0);
	
	//Reset values
	isUnderwater = false;
	underwaterLevel = 0.0;

	//calculate scaling
	testObjectHeight = (transform.position.y+buoyancyOffset-0.5);
	
		waveHeight = surfaceLevel;
		if (testObjectHeight < waveHeight){
			isUnderwater = true;
		}
		underwaterLevel =  waveHeight-testObjectHeight;


	//set buoyancy
	if (engageBuoyancy && isOver == 1.0){
	if (rigidbodyComponent && !rigidbodyComponent.isKinematic){
			
			buoyancyFactor = 10.0;

			if (isUnderwater){

				if (this.transform.position.y+buoyancyOffset-0.5 < waveHeight-surfaceRange){
					
					// add vertical force to buoyancy while underwater
					isUnder = true;
					forceMod = (buoyancyFactor * (buoyancy * rigidbodyComponent.mass) * (underwaterLevel) * splitFac * (isUnderwater ? 1.0 : 0.0) );
					if (rigidbodyComponent.velocity.y < maxVerticalSpeed){
						rigidbodyComponent.AddForceAtPosition(Vector3(0,1,0) * forceMod, transform.position);
					}
					modTime = 0.0;
					
				} else {
					
					// slow down vertical velocity as it reaches water surface or wave zenith
					isUnder = false;
					modTime = (this.transform.position.y+buoyancyOffset-0.5) / (waveHeight+Random.Range(0.0,0.25) * (isUnderwater ? 1.0 : 0.0));
					if (rigidbodyComponent.velocity.y > 0.0){
						rigidbodyComponent.velocity.y = Mathf.SmoothStep(rigidbodyComponent.velocity.y,0.0,modTime);
					}
				}
			
			
				//Add Water Force / Direction to Buoyancy Object
				if (inheritForce){
				if (this.transform.position.y+buoyancyOffset-0.5 <= waveHeight){
					waveFac = Mathf.Lerp(0.0,forceHeightFactor,waveHt);
					if (forceHeightFactor == 0.0) waveFac = 1.0;
					rigidbodyComponent.AddForceAtPosition(Vector3(forceAngles.x,0,forceAngles.y) * (buoyancyFactor*2.0) * forceSpeed * waveFac * splitFac * forceAmount, transform.position);
				}
				}

			}
			
	}
	}
	
}
}
