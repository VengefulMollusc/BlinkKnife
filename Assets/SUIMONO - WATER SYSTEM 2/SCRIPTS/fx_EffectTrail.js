//#pragma strict
@script RequireComponent (MeshFilter)

var numberSections : int = 0;
var startWidth : float = 1.0;
var endWidth : float = 2.0;
var time : float = 2.0;
var minDistance : float = 0.1;
var setHeight : float = 0.0;

var maxHeight : float = 1.8;

var normalAmt : float = 1.0;
var foamAmt : float = 1.0;
var heightAmt : float = 1.0;

var depthRange1 : float = 1.0;
var depthRange2 : float = 1.0;
var depthStretch : float = 1.0;

private	var getPos : float = 0.0;

private var savePos : Vector3;
private var sections = new Array();
private var section : SuimonoTrailSection;
private var suimonoModule : SuimonoModule;
private var isUnderwater : boolean = false;
private var setBlendColor : Color = Color(1,1,1,1);
private var fadeColor : Color = Color(1,1,1,1);

private var depth : float = 0.0;
private var surfaceY : float = 10.2;
private var heightData : float[];
private var trailRenderer : Renderer;
private var resetTime : float = 0.0;

private var startColor : Color;
private var endColor : Color = Color(0,0,0,0);

// create a simple cheap "smear" effect on the InvokeRepeating processor load.
// by shifting the work into rough groups via a simple static int, that we loop over.
 
// our global counter
static var staggerOffset:int = 0;
 
// our loop, we chose groups of roughly 20
static var staggerModulus:int = 20;
 
// to scale back our int to a usable "skip value in seconds"; 
// roughly 1 over the modulus, to even the spread, or "smear", over the second;
static var staggerMultiplier:float = 0.05f; 

// our actual stagger value 
private var stagger:float;




class SuimonoTrailSection
{
	var point : Vector3;
	var upDir : Vector3;
	var time : float;
	var vCol : Color;
}


function Start(){

	//get objects
	if (GameObject.Find("SUIMONO_Module") != null){
		suimonoModule = GameObject.Find("SUIMONO_Module").GetComponent(SuimonoModule) as SuimonoModule;
	}
	
	trailRenderer = this.gameObject.GetComponent(Renderer);

}



function LateUpdate(){
	if (suimonoModule != null){
		
		//force layer
		gameObject.layer = suimonoModule.layerScreenFXNum;

		transform.localEulerAngles = Vector3(0,90,0);
		foamAmt = Mathf.Clamp01(foamAmt);
		normalAmt = Mathf.Clamp01(normalAmt);
		heightAmt = Mathf.Clamp01(heightAmt);

		EffectTrailUpdate();
	}
}

/*
function Update(){

PerformUpdate();
trailRenderer.enabled = true;
	
	//only generate when advanced effects are enabled on the Module!
	if (suimonoModule.enableAdvancedDistort){
		PerformUpdate();
		trailRenderer.enabled = true;
	} else {
		sections = null;
		trailRenderer.enabled = false;
	}

	
		//reset blending color when not moving
		if (renderWhenMoving){
		if (Vector3.Distance(Vector3(transform.position.x,0.0,transform.position.z),Vector3(savePos.x,0.0,savePos.z)) > (0.5)){
			resetTime = 0.0;
			savePos = transform.position;
		} else {
			resetTime += Time.deltaTime;
		}
		}
		
		fadeColor = Color.Lerp(Color(1,1,1,1),Color(0,0,0,0),resetTime);
		if (resetTime >= time) sections = null;

}
*/


function UnderwaterCheck(){

	
	//if (suimonoModule != null){

		getPos = suimonoModule.SuimonoGetHeight(transform.position,"baseLevel");

		//underwater check
		heightData = suimonoModule.SuimonoGetHeightAll(this.transform.position);
		
		//depth = heightData[3];
		//surfaceY = heightData[2];
	depth = 0.5;
	surfaceY = heightData[2];

		if (heightData[3] > 0.0 && heightData[3] <= maxHeight && heightData[4] == 1.0){
			isUnderwater = true;
		} else {
			isUnderwater = false;
		}
		

		setBlendColor = Color.Lerp(Color(0,0,0,0),Color(1,0,0,1),(depth-depthRange1) / depthStretch); //foam depth 1
		setBlendColor = Color.Lerp(setBlendColor,Color(setBlendColor.r,1,0,1),(depth-depthRange2) / depthStretch); //foam depth 2
		setBlendColor.b = 1.0; // enable height mask


	//}
}




function EffectTrailUpdate() {


numberSections = sections.length;
	/*
	//only generate when advanced effects are enabled on the Module!
	if (suimonoModule.enableAdvancedDistort){
		PerformUpdate();
		trailRenderer.enabled = true;
	} else {
		sections = null;
		trailRenderer.enabled = false;
	}
	*/


	//reset blending color when not moving
	if (Vector3.Distance(Vector3(transform.position.x,0.0,transform.position.z),Vector3(savePos.x,0.0,savePos.z)) > (0.5)){
		resetTime = 0.0;
		savePos = transform.position;
	} else {
		resetTime += Time.deltaTime;
	}

	
	fadeColor = Color.Lerp(Color(1,1,1,1),Color(0,0,0,0),resetTime);
	if (resetTime >= time) sections = null;





	if (sections == null) sections = new Array();


	var position = transform.position;
	var now = Time.time;

	// Remove old sections
	//while (sections.length > 0 && now > sections[sections.length - 1].time + time) {
		//sections.Pop();
	//}


	//if (renderWhenMoving){
		//if (Vector3.Distance(transform.position,savePos) > 0.05){

			UnderwaterCheck();

			// Add a new trail section
			if (isUnderwater){
			//if (sections.length == 0 || (sections[0].point - position).sqrMagnitude > minDistance * minDistance)
			//{

				section = SuimonoTrailSection();
				section.point = position;

				setHeight = surfaceY;
				if (setHeight > 0.0){
					section.point.y = setHeight;
				}
				//if (alwaysUp){
					section.upDir = (Vector3(transform.forward.x,0,transform.forward.z));
				//} else {
				//	section.upDir = transform.TransformDirection(Vector3.up);
				//}
				section.time = now;

				//set vertex color based on water depth
				section.vCol = setBlendColor;

				sections.Unshift(section);
			//}
			}
		//}
	//}

	// Rebuild the mesh
	var mesh : Mesh = GetComponent(MeshFilter).mesh;
	mesh.Clear();
	
	// We need at least 2 sections to create the line
	if (sections.length < 2)
		return;

	var vertices = new Vector3[sections.length * 2];
	var colors = new Color[sections.length * 2];
	var uv = new Vector2[sections.length * 2];
	
	var previousSection : SuimonoTrailSection = sections[0];
	var currentSection : SuimonoTrailSection = sections[0];

	// Use matrix instead of transform.TransformPoint for performance reasons
	var localSpaceTransform = transform.worldToLocalMatrix;


	// Generate vertex, uv and colors
	for (var i=0;i<sections.length;i++)
	//for (var i=sections.length-1;i>=0;i--)
	{
		previousSection = currentSection;
		currentSection = sections[i];
		
		// Calculate u for texture uv and color interpolation
		var u = 0.0;		
		if (i != 0)
			u = Mathf.Clamp01((Time.time - currentSection.time) / time);
		
		// Calculate upwards direction
		var upDir = currentSection.upDir;

		// Generate vertices
		var spreadAmt : float = (startWidth+((endWidth-startWidth)*u));
		var raiseAmt : Vector3 = (Vector3.up * Mathf.Lerp(0.0,0.0,u));
		vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(currentSection.point + (upDir * spreadAmt) - raiseAmt);
		vertices[i * 2 + 0] = localSpaceTransform.MultiplyPoint(currentSection.point - (upDir * spreadAmt) - raiseAmt);

		uv[i * 2 + 0] = Vector2(u, 0);
		uv[i * 2 + 1] = Vector2(u, 1);
		
		// fade colors out over time
		//var fColor : Color = Color(1,1,1,1);
		//fColor = fadeColor = Color.Lerp(Color(1,1,1,1),Color(0,0,0,0),u);
		startColor.r = foamAmt;
		startColor.g = foamAmt;
		startColor.b = heightAmt;
		startColor.a = normalAmt * 4;
		var interpolatedColor = Color.Lerp(startColor, endColor, u*0.5);
		colors[i * 2 + 0] = interpolatedColor * currentSection.vCol;// * fColor;
		colors[i * 2 + 1] = interpolatedColor * currentSection.vCol;// * fColor;

		if (u >= 1.0){
			sections.RemoveAt(i);

		}
	}

	// Generate triangles indices
	var triangles = new int[(sections.length - 1) * 2 * 3];
	for (i=0;i<triangles.length / 6;i++)
	{
		triangles[i * 6 + 0] = i * 2;
		triangles[i * 6 + 1] = i * 2 + 1;
		triangles[i * 6 + 2] = i * 2 + 2;

		triangles[i * 6 + 3] = i * 2 + 2;
		triangles[i * 6 + 4] = i * 2 + 1;
		triangles[i * 6 + 5] = i * 2 + 3;
	}

	// Assign to mesh	
	mesh.vertices = vertices;
	mesh.colors = colors;
	mesh.uv = uv;
	mesh.triangles = triangles;
}


