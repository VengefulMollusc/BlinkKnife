@script ExecuteInEditMode()
#pragma strict

enum suiCamToolType{transparent,transparentCaustic,wakeEffects,normals,depthMask,localReflection,underwaterMask,underwater,shorelineObject,shorelineCapture};
var cameraType : suiCamToolType;

enum suiCamToolRender{automatic,deferredShading,deferredLighting,forward};
var renderType : suiCamToolRender = suiCamToolRender.automatic;

var resolution : int = 256;
var cameraOffset : float = 0.0;
var renderTexDiff : RenderTexture;
var renderShader : Shader;
var runInEditMode : boolean = false;
var isUnderwater : boolean = false;

private var usePath : RenderingPath;

private var suimonoModuleObject : SuimonoModule;
@HideInInspector var surfaceRenderer : Renderer;
@HideInInspector var scaleRenderer : Renderer;
private var cam : Camera;
private var copyCam : Camera;
private var currResolution : int = 256;

//Collect variables for reflection
private var clipPlaneOffset : float = 0.07;
@HideInInspector var reflectionDistance : float = 200.0;
@HideInInspector var setLayers : int;

//Collect variables for GC
private var pos : Vector3;
private var normal : Vector3;
private var d : float;
private var reflectionPlane : Vector4;
private var reflection : Matrix4x4;
private var oldpos : Vector3;
private var newpos : Vector3;
private var clipPlane : Vector4;
private var projection : Matrix4x4;
private var euler : Vector3;
private var scaleOffset : Matrix4x4;
private var scale : Vector3;
private var mtx : Matrix4x4;
private var offsetPos : Vector3;
private var m : Matrix4x4;
private var cpos : Vector3;
private var cnormal : Vector3;
private var proj : Matrix4x4;
private var q : Vector4;
private var c : Vector4;

private var hasStarted = 0.0;

//clear material to fix edge artifacts?
//private var clearMaterial : Material;




function Start () {

	if (GameObject.Find("SUIMONO_Module") != null){
		suimonoModuleObject = GameObject.Find("SUIMONO_Module").gameObject.GetComponent(SuimonoModule);
	}

	if (cameraType != suiCamToolType.localReflection){
		if (transform.parent != null) surfaceRenderer = transform.parent.gameObject.GetComponent(Renderer);
		//scaleRenderer = null;
	} else {
		if (transform.parent != null){
			surfaceRenderer = transform.parent.Find("Suimono_Object").gameObject.GetComponent(Renderer);
			scaleRenderer = transform.parent.Find("Suimono_ObjectScale").gameObject.GetComponent(Renderer);
		}
	}

	cam = gameObject.GetComponent(Camera) as Camera;
	if (suimonoModuleObject != null && suimonoModuleObject.setCamera != null){
		copyCam = suimonoModuleObject.setCamera.GetComponent(Camera);
	}

	//setup clamp material
    //clearMaterial = new Material(Shader.Find("Suimono2/ClearFx"));


	UpdateRenderTex();
	CameraUpdate();
}





function OnPreRender(){
	if (Application.isPlaying && cameraType == suiCamToolType.localReflection){
		GL.invertCulling = true;
	}
}
function OnPostRender(){
	if (Application.isPlaying){
		GL.invertCulling = false;
	}
}


function Update(){

	//update shoreline camera during edit mode
	if (!Application.isPlaying && runInEditMode){
		CameraUpdate();
	}

	//set layermasks
	if (cam != null){
		if (cameraType == suiCamToolType.shorelineCapture){
			cam.cullingMask = 1 << suimonoModuleObject.layerDepthNum;
		}
	}
}


function LateUpdate(){
	if (Application.isPlaying){
		if (cameraType == suiCamToolType.shorelineObject){
			if (hasStarted == 0.0 && Time.time > 0.2){
				CameraUpdate();
				hasStarted = 1.0;
			}
		} else {
			CameraUpdate();
		}
	}


}



function CameraRender(){


	//Setup Camera Matrices
	if (cameraType == suiCamToolType.localReflection){
		ReflectionPreRender();
	}

	//RENDER CAMERA
	cam.targetTexture = renderTexDiff;
	if (Application.isPlaying && cameraType == suiCamToolType.shorelineObject){
		cam.enabled = false;
		cam.Render();
	} else {
		cam.enabled = true;
	}

    //Reset Camera Properties
	if (cameraType == suiCamToolType.localReflection) ReflectionPostRender();

}




function CameraUpdate() {


	if (suimonoModuleObject != null){
		if (suimonoModuleObject.setCameraComponent != null){
			copyCam = suimonoModuleObject.setCameraComponent;
		}
	}


	if (copyCam != null && cam != null){

		//set camera settings
		if (cameraType != suiCamToolType.shorelineObject){
			cam.transform.position = copyCam.transform.position;
			cam.transform.rotation = copyCam.transform.rotation;
			cam.projectionMatrix = copyCam.projectionMatrix;;
			cam.fieldOfView = copyCam.fieldOfView;
		}



		//re-project camera for screen-space effects
		if (cameraOffset != 0.0){
			cam.transform.Translate(Vector3.forward * cameraOffset);
		}

			//select rendering path
			if (renderType == suiCamToolRender.automatic){
				usePath = copyCam.actualRenderingPath;

				//specific settings for transparent camera
				if (cameraType == suiCamToolType.transparent){
					if (copyCam.renderingPath == RenderingPath.Forward){
						usePath = RenderingPath.DeferredLighting;
					} else {
						usePath = copyCam.renderingPath;
					}
				}
			
			} else if (renderType == suiCamToolRender.deferredShading){
				usePath = RenderingPath.DeferredShading;

			} else if (renderType == suiCamToolRender.deferredLighting){
				usePath = RenderingPath.DeferredLighting;

			} else if (renderType == suiCamToolRender.forward){
				usePath = RenderingPath.Forward;
			}


			
			//set effect rendering path
			cam.renderingPath = usePath;



		if (renderTexDiff != null){

			//update texture resolution
			if (resolution != currResolution){
				currResolution = resolution;
				UpdateRenderTex();
			}


			//render custom normal effects shader
			if (cameraType == suiCamToolType.normals){
				if (suimonoModuleObject.enableAdvancedDistort){
					cam.hdr = false;
					cam.SetReplacementShader(renderShader,"RenderType");
					CameraRender();
				} else {
					renderTexDiff == null;
				}

			//render customwake effects shader
			} else if (cameraType == suiCamToolType.wakeEffects){
				if (suimonoModuleObject.enableAdvancedDistort){
					cam.SetReplacementShader(renderShader,"RenderType");
					CameraRender();
				} else {
					renderTexDiff == null;
				}

			//render transparency effects
			} else if (cameraType == suiCamToolType.transparent){
				if (suimonoModuleObject.enableTransparency){
					CameraRender();
				} else {
					renderTexDiff == null;
				}

			//render caustics effects
			} else if (cameraType == suiCamToolType.transparentCaustic){
				if (suimonoModuleObject.enableCaustics){
					CameraRender();
				} else {
					renderTexDiff == null;
				}

				//render custom normal effects shader
				//if (cameraType == suiCamToolType.normals){
				//	cam.SetReplacementShader(renderShader,"RenderType");
				//	CameraRender();
				//}
			} else {
				CameraRender();
			}
			


			//pass texture to shader
			if (cameraType == suiCamToolType.transparent){
				Shader.SetGlobalTexture("_suimono_TransTex",renderTexDiff);
				if (!suimonoModuleObject.enableCausticsBlending) Shader.SetGlobalTexture("_suimono_CausticTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.transparentCaustic){
				if (suimonoModuleObject.enableCausticsBlending) Shader.SetGlobalTexture("_suimono_CausticTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.wakeEffects){
				Shader.SetGlobalTexture("_suimono_WakeTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.normals){
				Shader.SetGlobalTexture("_suimono_NormalsTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.depthMask){
				Shader.SetGlobalTexture("_suimono_depthMaskTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.underwaterMask){
				Shader.SetGlobalTexture("_suimono_underwaterMaskTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.underwater){
				Shader.SetGlobalTexture("_suimono_underwaterTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.localReflection){
				if (surfaceRenderer != null) surfaceRenderer.sharedMaterial.SetTexture("_ReflectionTex",renderTexDiff);
				//if (scaleRenderer != null) scaleRenderer.sharedMaterial.SetTexture("_ReflectionTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.shorelineObject){
				if (surfaceRenderer != null) surfaceRenderer.sharedMaterial.SetTexture("_MainTex",renderTexDiff);
				//if (scaleRenderer != null) scaleRenderer.sharedMaterial.SetTexture("_MainTex",renderTexDiff);
			}
			if (cameraType == suiCamToolType.shorelineCapture){
				Shader.SetGlobalTexture("_suimono_shorelineTex",renderTexDiff);
			}

		} else {
			UpdateRenderTex();
		}
		
	}

	//remove edgepixel to fix texture clamp issues
	//if (clearMaterial != null) DrawBorder(renderTexDiff, clearMaterial);

}




function UpdateRenderTex(){

	if (resolution < 4) resolution = 4;
		
	if (renderTexDiff != null){
		if (cam != null) cam.targetTexture = null;
		DestroyImmediate(renderTexDiff);
	}
	renderTexDiff = new RenderTexture(resolution,resolution,24,RenderTextureFormat.ARGBHalf,RenderTextureReadWrite.Linear);
	
	#if UNITY_5_4 || UNITY_5_6 || UNITY_5_7 || UNITY_5_8 || UNITY_5_9
		renderTexDiff.dimension = Rendering.TextureDimension.Tex2D;
	#else
		renderTexDiff.isCubemap = false;
	#endif

	renderTexDiff.generateMips = false;
	renderTexDiff.anisoLevel = 1;
	renderTexDiff.filterMode = FilterMode.Trilinear;
	renderTexDiff.wrapMode = TextureWrapMode.Clamp;
	

}





function ReflectionPreRender(){

    // find out the reflection plane: position and normal in world space
    pos = transform.parent.position;

    if (isUnderwater){
    	normal = -transform.parent.transform.up; //underwater
	} else {
		normal = transform.parent.transform.up; //above water
	}

    //set camera properties
    cam.CopyFrom(copyCam);

	if (isUnderwater){
    	cam.farClipPlane = 3;
    	cam.clearFlags = CameraClearFlags.Color;
    	cam.depthTextureMode = DepthTextureMode.Depth;
	} else {
		cam.farClipPlane = reflectionDistance;
		cam.clearFlags = CameraClearFlags.Skybox;
	}

	//render transparency effects
	if (cameraType == suiCamToolType.localReflection){
		if (renderShader != null){
			cam.SetReplacementShader(renderShader,null);

		}
	}




	cam.cullingMask = setLayers;

    // Render reflection
    // Reflect camera around reflection plane
    d = -Vector3.Dot (normal, pos) - clipPlaneOffset;
    reflectionPlane = Vector4(normal.x, normal.y, normal.z, d);
 
    reflection  = Matrix4x4.zero;
	reflection = Set_CalculateReflectionMatrix (reflectionPlane);

    oldpos = copyCam.transform.position;
    newpos = reflection.MultiplyPoint( oldpos );
    cam.worldToCameraMatrix = copyCam.worldToCameraMatrix * reflection;
 
    // Setup oblique projection matrix so that near plane is our reflection
    // plane. This way we clip everything below/above it for free.
    clipPlane = Set_CameraSpacePlane(cam, pos, normal, 1.0);
    projection = copyCam.projectionMatrix;
    projection = Set_CalculateObliqueMatrix (clipPlane);
    cam.projectionMatrix = projection;

	GL.invertCulling = true;

	cam.transform.position = newpos;
    euler = copyCam.transform.eulerAngles;
    cam.transform.eulerAngles = Vector3(0, euler.y, euler.z);
}



function ReflectionPostRender(){
    cam.transform.position = oldpos;
	GL.invertCulling = false;
    scaleOffset = Matrix4x4.TRS(Vector3(0.5f,0.5f,0.5f), Quaternion.identity, Vector3(0.5f,0.5f,0.5f) );
    scale = transform.lossyScale;
    mtx = transform.localToWorldMatrix * Matrix4x4.Scale(Vector3(1.0f/scale.x, -1.0f/scale.y, 1.0f/scale.z) );
    mtx = scaleOffset * copyCam.projectionMatrix * copyCam.worldToCameraMatrix * mtx;
}


function Set_sgn( a : float) : float{
    if (a > 0.0f) return 1.0f;
    if (a < 0.0f) return -1.0f;
    return 0.0f;
}


function Set_CameraSpacePlane (cm : Camera , pos : Vector3 , normal : Vector3 , sideSign : float) : Vector4 {
    offsetPos = pos + normal * (clipPlaneOffset);
    m = cm.worldToCameraMatrix;
    cpos = m.MultiplyPoint( offsetPos );
    cnormal = m.MultiplyVector( normal ).normalized * sideSign;
    return Vector4( cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos,cnormal) );
}


function Set_CalculateObliqueMatrix (clipPlane : Vector4) : Matrix4x4 {
	proj = copyCam.projectionMatrix;
    q = proj.inverse * Vector4(Set_sgn(clipPlane.x),Set_sgn(clipPlane.y),1.0,1.0);
    c = clipPlane * (2.0 / (Vector4.Dot (clipPlane, q)));
    proj[2] = c.x - proj[3];
    proj[6] = c.y - proj[7];
    proj[10] = c.z - proj[11];
    proj[14] = c.w - proj[15];
    return proj;
}


function Set_CalculateReflectionMatrix (plane : Vector4) : Matrix4x4 {
	
	var reflectionMat : Matrix4x4 = Matrix4x4.zero;
    
    reflectionMat.m00 = (1F - 2F*plane[0]*plane[0]);
    reflectionMat.m01 = (   - 2F*plane[0]*plane[1]);
    reflectionMat.m02 = (   - 2F*plane[0]*plane[2]);
    reflectionMat.m03 = (   - 2F*plane[3]*plane[0]);

    reflectionMat.m10 = (   - 2F*plane[1]*plane[0]);
    reflectionMat.m11 = (1F - 2F*plane[1]*plane[1]);
    reflectionMat.m12 = (   - 2F*plane[1]*plane[2]);
    reflectionMat.m13 = (   - 2F*plane[3]*plane[1]);

    reflectionMat.m20 = (   - 2F*plane[2]*plane[0]);
    reflectionMat.m21 = (   - 2F*plane[2]*plane[1]);
    reflectionMat.m22 = (1F - 2F*plane[2]*plane[2]);
    reflectionMat.m23 = (   - 2F*plane[3]*plane[2]);

    reflectionMat.m30 = 0F;
    reflectionMat.m31 = 0F;
    reflectionMat.m32 = 0F;
    reflectionMat.m33 = 1F;

    return reflectionMat;
}





function DrawBorder( dest : RenderTexture, material : Material){

    var x1 : float;
    var x2 : float;
    var y1 : float;
    var y2 : float;

    RenderTexture.active = dest;
    var invertY : boolean = true; // source.texelSize.y < 0.0ff;

    // Set up the simple Matrix
    GL.PushMatrix();
    GL.LoadOrtho();

    for (var i : int = 0; i < material.passCount; i++)
    {
        material.SetPass(i);

        var y1_ : float;
        var y2_ : float;

        if (invertY){
            y1_ = 1.0f;
            y2_ = 0.0f;
        } else {
            y1_ = 0.0f;
            y2_ = 1.0f;
        }

        // left
        x1 = 0.0f;
        x2 = 0.0f + 1.0f/(dest.width*1.0f);
        y1 = 0.0f;
        y2 = 1.0f;
        GL.Begin(GL.QUADS);

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // right
        x1 = 1.0f - 1.0f/(dest.width*1.0f);
        x2 = 1.0f;
        y1 = 0.0f;
        y2 = 1.0f;

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // top
        x1 = 0.0f;
        x2 = 1.0f;
        y1 = 0.0f;
        y2 = 0.0f + 1.0f/(dest.height*1.0f);

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        // bottom
        x1 = 0.0f;
        x2 = 1.0f;
        y1 = 1.0f - 1.0f/(dest.height*1.0f);
        y2 = 1.0f;

        GL.TexCoord2(0.0f, y1_); GL.Vertex3(x1, y1, 0.1f);
        GL.TexCoord2(1.0f, y1_); GL.Vertex3(x2, y1, 0.1f);
        GL.TexCoord2(1.0f, y2_); GL.Vertex3(x2, y2, 0.1f);
        GL.TexCoord2(0.0f, y2_); GL.Vertex3(x1, y2, 0.1f);

        GL.End();
    }

    GL.PopMatrix();
}