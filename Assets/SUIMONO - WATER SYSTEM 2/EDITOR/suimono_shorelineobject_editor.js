
@script ExecuteInEditMode()
@CustomEditor (Suimono_ShorelineObject)

class suimono_shorelineobject_editor extends Editor {
	
	var renName : String="";
	var setRename : int = 0;
	
	var localPresetIndex : int = -1;
	var basePos : int = 0;
	
	//var showErrors : boolean = false;
	//var showPresets : boolean = false;
 	//var showSplash : boolean = false;
  	//var showWaves : boolean = false;
  	//var showGeneral : boolean = false;
  	//var showSurface : boolean = false;
   	//var showUnderwater : boolean = false;
  	//var showEffects : boolean = false;
  	//var showColor : boolean = false;
   	//var showReflect : boolean = false;
 	//var showFoam : boolean = false;
 	
 	var logoTex : Texture;// = Resources.Load("textures/gui_tex_suimonologo");
	var divTex : Texture;// = Resources.Load("textures/gui_tex_suimonodiv");
	var divRevTex : Texture;// = Resources.Load("textures/gui_tex_suimonodivrev");
	var divVertTex : Texture;// = Resources.Load("textures/gui_tex_suimono_divvert");
	var divHorizTex : Texture;// = Resources.Load("textures/gui_tex_suimono_divhorz");
	var bgPreset : Texture;// = Resources.Load("textures/gui_bgpreset");
	var bgPresetSt : Texture;// = Resources.Load("textures/gui_bgpresetSt");
	var bgPresetNd : Texture;// = Resources.Load("textures/gui_bgpresetNd");
			
 	var colorEnabled : Color = Color(1.0,1.0,1.0,1.0);
	var colorDisabled : Color = Color(1.0,1.0,1.0,0.25);
	var colorWarning : Color = Color(0.9,0.5,0.1,1.0);

	var highlightColor2 : Color = Color(0.7,1,0.2,0.6);//Color(0.7,1,0.2,0.6);
	var highlightColor : Color = Color(1,0.5,0,0.9);
 	//var highlightColor3 : Color = Color(0.7,1,0.2,0.6);
 	
 	
 	//function OnInspectorUpdate() {
	//	Repaint();
	//}	
		
		
    function OnInspectorGUI () {
    	
    	//load textures
	 	logoTex = Resources.Load("textures/gui_tex_suimonologo");
		divTex = Resources.Load("textures/gui_tex_suimonodiv");
		divRevTex = Resources.Load("textures/gui_tex_suimonodivrev");
		divVertTex = Resources.Load("textures/gui_tex_suimono_divvert");
		divHorizTex = Resources.Load("textures/gui_tex_suimono_divhorz");
		bgPreset = Resources.Load("textures/gui_bgpreset");
		bgPresetSt = Resources.Load("textures/gui_bgpresetSt");
		bgPresetNd = Resources.Load("textures/gui_bgpresetNd");

		#if UNITY_PRO_LICENSE
			divTex = Resources.Load("textures/gui_tex_suimonodiv");
			logoTex = Resources.Load("textures/gui_tex_suimonologoshore");
			bgPreset = Resources.Load("textures/gui_bgpreset");
			bgPresetSt = Resources.Load("textures/gui_bgpresetSt");
			bgPresetNd = Resources.Load("textures/gui_bgpresetNd");
			highlightColor = Color(1,0.5,0,0.9);
		#else
			divTex = Resources.Load("textures/gui_tex_suimonodiv_i");
			logoTex = Resources.Load("textures/gui_tex_suimonologoshore_i");
			bgPreset = Resources.Load("textures/gui_bgpreset_i");
			bgPresetSt = Resources.Load("textures/gui_bgpresetSt_i");
			bgPresetNd = Resources.Load("textures/gui_bgpresetNd_i");
			highlightColor = Color(0.0,0.81,0.9,0.6);
		#endif



		//SET SCREEN WIDTH
		var setWidth = Screen.width-220;
		if (setWidth < 120) setWidth = 120;
		
		//SUIMONO LOGO
		var buttonText : GUIContent = new GUIContent(""); 
		var buttonStyle : GUIStyle = GUIStyle.none; 
		var rt : Rect = GUILayoutUtility.GetRect(buttonText, buttonStyle);
		var margin : int = 15;

		//start menu
        GUI.contentColor = Color(1.0,1.0,1.0,0.4);
		EditorGUI.LabelField(Rect(rt.x+margin+2, rt.y+37, 50, 18),"Version");
		GUI.contentColor = Color(1.0,1.0,1.0,0.6);
		
		var linkVerRect : Rect = Rect(rt.x+margin+51, rt.y+37, 90, 18);
		EditorGUI.LabelField(linkVerRect,target.suimonoVersionNumber);

		GUI.contentColor = Color(1.0,1.0,1.0,1.0);
	    GUI.contentColor = Color(1.0,1.0,1.0,0.4);
	    var linkHelpRect : Rect = Rect(rt.x+margin+165, rt.y+37, 28, 18);
	    var linkBugRect : Rect = Rect(rt.x+margin+165+42, rt.y+37, 65, 18);
	    var linkURLRect : Rect = Rect(rt.x+margin+165+120, rt.y+37, 100, 18);
	    
		if (Event.current.type == EventType.MouseUp && linkHelpRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/forum/");
		if (Event.current.type == EventType.MouseUp && linkBugRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/forum/");
		if (Event.current.type == EventType.MouseUp && linkURLRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/suimono/");

		EditorGUI.LabelField(Rect(rt.x+margin+165+30, rt.y+37, 220, 18),"|");
		EditorGUI.LabelField(Rect(rt.x+margin+165+110, rt.y+37, 220, 18),"|");
		
		GUI.contentColor = Color(1.0,1.0,1.0,0.4);
		EditorGUI.LabelField(linkHelpRect,"help");
		EditorGUI.LabelField(linkBugRect,"report bug");
		EditorGUI.LabelField(linkURLRect,"tanukidigital.com");
		// end menu
		
		
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,36),logoTex);
        GUILayout.Space(40.0);
        
        
		var tSpace : int = 0;





        // GENERAL SETTINGS
        GUI.contentColor = colorEnabled;
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
       
        target.debug = EditorGUI.Toggle(Rect (rt.x+margin+5, rt.y+5, 20, 20), "", target.debug);
        GUI.Label (Rect (rt.x+margin+25, rt.y+5, 300, 20), GUIContent ("Debug Mode"));

        GUI.Label (Rect (rt.x+margin+5, rt.y+25, 120, 18), GUIContent ("Attach to Surface"));
        target.attachToSurface = EditorGUI.ObjectField(Rect(rt.x+margin+130, rt.y+25, setWidth+35, 15), target.attachToSurface, Transform, true);

		
		EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+50,372,1),divHorizTex);


        EditorGUI.LabelField(Rect(rt.x+margin+5, rt.y+60, 120, 18),"Rendering Mode");
        target.shorelineModeIndex = EditorGUI.Popup(Rect(rt.x+margin+130, rt.y+60, 100, 18),"",target.shorelineModeIndex, target.shorelineModeOptions);
  		

 
  		// MODE = AUTOMATIC
  		if (target.shorelineModeIndex == 0){

  			EditorGUI.LabelField(Rect(rt.x+margin+5, rt.y+80, 120, 18),"Generate Mode");
        	target.shorelineRunIndex = EditorGUI.Popup(Rect(rt.x+margin+130, rt.y+80, 100, 18),"",target.shorelineRunIndex, target.shorelineRunOptions);
  	
			EditorGUI.LabelField(Rect(rt.x+margin+5, rt.y+100, 130, 18),"Depth Range(R)");
			target.sceneDepth = EditorGUI.Slider(Rect(rt.x+margin+130, rt.y+100, setWidth+35, 18),"",target.sceneDepth,0.0,50.0);

			EditorGUI.LabelField(Rect(rt.x+margin+5, rt.y+120, 130, 18),"Shore Range(G)");
			target.shoreDepth = EditorGUI.Slider(Rect(rt.x+margin+130, rt.y+120, setWidth+35, 18),"",target.shoreDepth,0.0,50.0);

			
			EditorGUI.LabelField(Rect(rt.x+margin+5, rt.y+basePos+140, 180, 18),"Calculate Layers");
			if (target.moduleObject != null){
				target.depthLayer = EditorGUI.MaskField(Rect(rt.x+margin+130, rt.y+basePos+140, 90, 18),"", target.depthLayer, target.suiLayerMasks);
			} else {
			//	EditorGUI.LabelField(Rect(rt.x+margin+130, rt.y+basePos+85, 280, 38),"To select layers, make sure the Suimono");
			//	EditorGUI.LabelField(Rect(rt.x+margin+130, rt.y+basePos+99, 280, 38),"Module is enabled in your scene.");
			}

			GUILayout.Space(140);
		}

		// MODE = CUSTOM TEXTURE
		if (target.shorelineModeIndex == 1){
         	GUI.Label (Rect (rt.x+margin+5, rt.y+80, 120, 15), GUIContent ("Depth Texture"));
        	target.customDepthTex = EditorGUI.ObjectField(Rect(rt.x+margin+130, rt.y+80, setWidth, 35), target.customDepthTex, Texture2D, true);

        	GUILayout.Space(120);
		}

			

		//}
        GUILayout.Space(10.0);
  

    //} \     
        

	EditorGUILayout.Space();



        	
    if (GUI.changed) EditorUtility.SetDirty(target);
    }
    
    
    
    
    
    
}