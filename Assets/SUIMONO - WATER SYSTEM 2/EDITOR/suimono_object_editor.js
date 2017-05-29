
@script ExecuteInEditMode()
@CustomEditor (SuimonoObject)

class suimono_object_editor extends Editor {
	
	var renName : String="";
	var setRename : int = 0;
	
	var localPresetIndex : int = -1;
	var basePos : int = 0;
	
	var showErrors : boolean = false;
	var showPresets : boolean = false;
 	var showSplash : boolean = false;
  	var showWaves : boolean = false;
  	var showGeneral : boolean = false;
  	var showSurface : boolean = false;
   	var showUnderwater : boolean = false;
  	var showEffects : boolean = false;
  	var showColor : boolean = false;
   	var showReflect : boolean = false;
 	var showFoam : boolean = false;
 	
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

	var highlightColor2 : Color = Color(0.7,1,0.2,0.6);
	var highlightColor : Color = Color(1,0.5,0,0.9);



		
    function OnInspectorGUI () {
    	
    	
 	
    	if (localPresetIndex == -1) localPresetIndex = target.presetUseIndex;

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
			logoTex = Resources.Load("textures/gui_tex_suimonologo");
			bgPreset = Resources.Load("textures/gui_bgpreset");
			bgPresetSt = Resources.Load("textures/gui_bgpresetSt");
			bgPresetNd = Resources.Load("textures/gui_bgpresetNd");
			highlightColor = Color(1,0.5,0,0.9);
		#else
			divTex = Resources.Load("textures/gui_tex_suimonodiv_i");
			logoTex = Resources.Load("textures/gui_tex_suimonologo_i");
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
		//GUI.color = colorEnabled;


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
        target.showGeneral = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showGeneral, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("GENERAL SETTINGS"));

        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 250, 20),"")) target.showGeneral = !target.showGeneral;
		GUI.color.a = 1.0;

        EditorGUI.LabelField(Rect(rt.x+margin+240, rt.y+6, 80, 18),"Mode");
        target.editorIndex = EditorGUI.Popup(Rect(rt.x+margin+280, rt.y+6, 100, 18),"",target.editorIndex, target.editorOptions);
        if (target.showGeneral){
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 80, 18),"Surface Type");
			target.typeIndex = EditorGUI.Popup(Rect(rt.x+margin+100, rt.y+30, 145, 18),"",target.typeIndex, target.typeOptions);
			
			if (target.typeIndex == 0){
				EditorGUI.LabelField(Rect(rt.x+margin+260, rt.y+30, 80, 18),"Ocean Scale");
	        	target.oceanScale = EditorGUI.FloatField(Rect(rt.x+margin+343, rt.y+30, 30, 18),"",target.oceanScale);
			}

			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 80, 18),"Surface LOD");
			if (target.enableCustomMesh && target.typeIndex != 0){
				GUI.contentColor = colorWarning;
        		GUI.backgroundColor = colorWarning;
				EditorGUI.LabelField(Rect(rt.x+margin+100, rt.y+50, 275, 18),"NOTE: Not available while using custom mesh!");
				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;
			} else {
				if (target.typeIndex == 0){
					target.lodIndex = 0;
					GUI.contentColor = colorDisabled;
        			GUI.backgroundColor = colorDisabled;
				}
				if (target.typeIndex == 2){
					target.lodIndex = 3;
					GUI.contentColor = colorDisabled;
        			GUI.backgroundColor = colorDisabled;
				}
				target.lodIndex = EditorGUI.Popup(Rect(rt.x+margin+100, rt.y+50, 145, 18),"",target.lodIndex, target.lodOptions);
				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;
			}

			//ADVANCED FX SETTINGS
			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+76,372,1),divHorizTex);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+80, 260, 18),"ADVANCED FX SETTINGS");
    		EditorGUI.LabelField(Rect(rt.x+margin+37, rt.y+100, 150, 18),"Enable Underwater FX");
			target.enableUnderwaterFX = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+100, 30, 18),"", target.enableUnderwaterFX);
    		EditorGUI.LabelField(Rect(rt.x+margin+220, rt.y+100, 150, 18),"Enable Caustic FX");
			target.enableCausticFX = EditorGUI.Toggle(Rect(rt.x+margin+200, rt.y+100, 30, 18),"", target.enableCausticFX);


			//SCENE REFLECTIONS
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	basePos = 127;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+basePos,372,1),divHorizTex);
        	target.enableReflections = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+basePos+5, 20, 18),"", target.useEnableReflections);
			EditorGUI.LabelField(Rect(rt.x+margin+31, rt.y+basePos+5, 160, 18),"SCENE REFLECTIONS");
        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;

			if (target.useEnableReflections){

				target.enableDynamicReflections = EditorGUI.Toggle(Rect(rt.x+margin+27, rt.y+basePos+28, 20, 18),"", target.useEnableDynamicReflections);
				EditorGUI.LabelField(Rect(rt.x+margin+45, rt.y+basePos+28, 160, 18),"Enable Dynamic Reflections");

				if (!target.useEnableDynamicReflections){
					GUI.contentColor = colorDisabled;
        			GUI.backgroundColor = colorDisabled;
				}
				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+48, 180, 18),"Reflect Layers");
				if (target.gameObject.activeInHierarchy){
					target.reflectLayer = EditorGUI.MaskField(Rect(rt.x+margin+120, rt.y+basePos+48, 90, 18),"", target.reflectLayer, target.suiLayerMasks);
				}
				EditorGUI.LabelField(Rect(rt.x+margin+225, rt.y+basePos+48, 180, 18),"Resolution");
				if (target.gameObject.activeInHierarchy){
					target.reflectResolution = EditorGUI.Popup(Rect(rt.x+margin+295, rt.y+basePos+48, 90, 18),"", target.reflectResolution, target.resOptions);
				}

				//EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+68, 180, 18),"Reflection Distance");
				//target.reflectionDistance = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+basePos+68, setWidth, 18),"",target.reflectionDistance,0.0,100000.0);
				//EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+88, 180, 18),"Reflection Spread");
				//target.reflectionSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+basePos+88, setWidth, 18),"",target.reflectionSpread,0.0,1.0);
				

				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;

        		if (target.enableDynamicReflections && target.useEnableDynamicReflections){
					GUI.contentColor = colorDisabled;
        			GUI.backgroundColor = colorDisabled;
				}
				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+78, 180, 18),"Fallback Mode");
				if (target.gameObject.activeInHierarchy){
					target.reflectFallback = EditorGUI.Popup(Rect(rt.x+margin+120, rt.y+basePos+78, 120, 18),"", target.reflectFallback, target.resFallbackOptions);
				}
				if (target.reflectFallback == 2){
					target.customRefCubemap = EditorGUI.ObjectField(Rect(rt.x+margin+250, rt.y+basePos+78, 136, 16), target.customRefCubemap, Texture, true);
				}
				if (target.reflectFallback == 3){
					target.customRefColor = EditorGUI.ColorField(Rect(rt.x+margin+250, rt.y+basePos+78, 136, 16), target.customRefColor);
				}

				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;

				basePos += 102;
				tSpace += 92;

			} else {
				basePos += 25;
			}

/*
			//DEPTH CALCULATION
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	//basePos = 172;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+basePos,372,1),divHorizTex);
        	target.enableShore = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+basePos+5, 20, 18),"", target.enableShore);
			EditorGUI.LabelField(Rect(rt.x+margin+31, rt.y+basePos+5, 160, 18),"DEPTH CALCULATION");
        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;

			if (target.enableShore){
			
	     		EditorGUI.LabelField(Rect(rt.x+margin+47, rt.y+basePos+48, 140, 18),"Debug");
	  			target.enableShoreDebug = EditorGUI.Toggle(Rect(rt.x+margin+27, rt.y+basePos+48, 20, 18),"", target.enableShoreDebug);

				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+28, 180, 18),"Shore Layers");
				if (target.gameObject.activeInHierarchy){
					target.shoreLayer = EditorGUI.MaskField(Rect(rt.x+margin+113, rt.y+basePos+28, 90, 18),"", target.shoreLayer, target.suiLayerMasks);
				}
				EditorGUI.LabelField(Rect(rt.x+margin+225, rt.y+basePos+28, 180, 18),"Resolution");
				if (target.gameObject.activeInHierarchy){
					target.shoreResolution = EditorGUI.Popup(Rect(rt.x+margin+295, rt.y+basePos+28, 90, 18),"", target.shoreResolution, target.resOptions);
				}

				EditorGUI.LabelField(Rect(rt.x+margin+130, rt.y+basePos+48, 100, 18),"Depth");
				target.shoreDepth = EditorGUI.Slider(Rect(rt.x+margin+175, rt.y+basePos+48, setWidth-10, 18),"",target.shoreDepth,45.0,15.0);

				basePos += 75;
				tSpace += 56;

			} else {
				basePos += 25;
			}

*/

			//TESSELLATION
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+basePos,372,1),divHorizTex);
        	//basePos = 250;
        	target.enableTess = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+basePos+5, 20, 18),"", target.enableTess);
			EditorGUI.LabelField(Rect(rt.x+margin+31, rt.y+basePos+5, 160, 18),"TESSELLATION");

			if (target.enableTess){

		        GUI.contentColor = colorEnabled;
		        GUI.backgroundColor = colorEnabled;

				#if !UNITY_STANDALONE_OSX
				//if (PlayerSettings.useDirect3D11){
				if (target.typeIndex == 2){
		  			GUI.contentColor = colorDisabled;
					GUI.backgroundColor = colorDisabled;
				//}
				}
					

		  		if (!target.enableTess){
		  			GUI.contentColor = colorDisabled;
					GUI.backgroundColor = colorDisabled;
				}

				//Force dx9 when dx11 isn't available
				if (target.unityVersionIndex == 0){
		  			GUI.contentColor = colorDisabled;
					GUI.backgroundColor = colorDisabled;
				}
				#endif							

				#if UNITY_STANDALONE_OSX
		  			GUI.contentColor = colorDisabled;
					GUI.backgroundColor = colorDisabled;
				#endif

				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+25, 140, 18),"Tessellation Factor");
				target.waveTessAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+basePos+25, setWidth, 18),"",target.waveTessAmt,0.001,100.0);
				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+45, 140, 18),"Tessellation Start");
				target.waveTessMin = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+basePos+45, setWidth, 18),"",target.waveTessMin,0.0,1.0);
				EditorGUI.LabelField(Rect(rt.x+margin+27, rt.y+basePos+65, 140, 18),"Tessellation Spread");
				target.waveTessSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+basePos+65, setWidth, 18),"",target.waveTessSpread,0.0,1.0);
				GUI.contentColor = colorEnabled;
				GUI.backgroundColor = colorEnabled;

			}
			
			//dx11 warning messages
			GUI.contentColor = colorWarning;
        	GUI.backgroundColor = colorWarning;
			if (target.unityVersionIndex == 0){
				EditorGUI.LabelField(Rect(rt.x+margin+137, rt.y+basePos+5, 260, 18),"NOTE: only available on PC in dx11 mode!");
			}
			#if UNITY_STANDALONE_OSX
				EditorGUI.LabelField(Rect(rt.x+margin+137, rt.y+basePos+5, 260, 18),"NOTE: only available on PC in dx11 mode!");
			#endif
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;

			if (target.enableTess){
				basePos += 95;
				tSpace += 70;
			} else {
				basePos += 25;
			}
			



			// CUSTOM TEXTURES
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+basePos,372,1),divHorizTex);

        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
			EditorGUI.LabelField(Rect(rt.x+margin+31, rt.y+basePos+5, 140, 18),"CUSTOM TEXTURES");
  			target.enableCustomTextures = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+basePos+5, 20, 18),"", target.enableCustomTextures);

  		
  			if (target.enableCustomTextures){

				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
  				GUI.Label (Rect (rt.x+margin+38, rt.y+basePos+86, 100, 18), GUIContent ("Shallow Waves"));
				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;
  				GUI.Label (Rect (rt.x+margin+34, rt.y+basePos+72, 100, 18), GUIContent ("Normal"));
  				GUI.Label (Rect (rt.x+margin+92, rt.y+basePos+72, 100, 18), GUIContent ("Height"));
        		target.customTexNormal1 = EditorGUI.ObjectField(Rect(rt.x+margin+34, rt.y+basePos+24, 45, 45), target.customTexNormal1, Texture2D, true);
        		target.customTexHeight1 = EditorGUI.ObjectField(Rect(rt.x+margin+88, rt.y+basePos+24, 45, 45), target.customTexHeight1, Texture2D, true);

				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
				GUI.Label (Rect (rt.x+margin+153, rt.y+basePos+86, 100, 18), GUIContent ("Turbulent Waves"));
				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;
        		GUI.Label (Rect (rt.x+margin+155, rt.y+basePos+72, 100, 18), GUIContent ("Normal"));
  				GUI.Label (Rect (rt.x+margin+211, rt.y+basePos+72, 100, 18), GUIContent ("Height"));
        		target.customTexNormal2 = EditorGUI.ObjectField(Rect(rt.x+margin+155, rt.y+basePos+24, 45, 45), target.customTexNormal2, Texture2D, true);
        		target.customTexHeight2 = EditorGUI.ObjectField(Rect(rt.x+margin+209, rt.y+basePos+24, 45, 45), target.customTexHeight2, Texture2D, true);

				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
  				GUI.Label (Rect (rt.x+margin+292, rt.y+basePos+86, 100, 18), GUIContent ("Deep Waves"));		
  				GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;
  				GUI.Label (Rect (rt.x+margin+278, rt.y+basePos+72, 100, 18), GUIContent ("Normal"));
  				GUI.Label (Rect (rt.x+margin+335, rt.y+basePos+72, 100, 18), GUIContent ("Height"));
        		target.customTexNormal3 = EditorGUI.ObjectField(Rect(rt.x+margin+277, rt.y+basePos+24, 45, 45), target.customTexNormal3, Texture2D, true);
        		target.customTexHeight3 = EditorGUI.ObjectField(Rect(rt.x+margin+333, rt.y+basePos+24, 45, 45), target.customTexHeight3, Texture2D, true);

  				//GUI.Label (Rect (rt.x+margin+323, rt.y+basePos+65, 100, 18), GUIContent ("Reflection"));
        		//target.customTexReflect = EditorGUI.ObjectField(Rect(rt.x+margin+327, rt.y+basePos+8, 55, 55), target.customTexReflect, Texture, true);
        	
        		basePos += 110;
  				tSpace += 87;
			} else {
				basePos += 25;
			}




			// CUSTOM MESH
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+basePos,372,1),divHorizTex);

        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
			EditorGUI.LabelField(Rect(rt.x+margin+31, rt.y+basePos+5, 140, 18),"CUSTOM MESH");
  			target.enableCustomMesh = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+basePos+5, 20, 18),"", target.enableCustomMesh);

  		
  			if (target.enableCustomMesh){

  				if (target.typeIndex != 0){
        			target.customMesh = EditorGUI.ObjectField(Rect(rt.x+margin+171, rt.y+basePos+8, 200, 18), target.customMesh, Mesh, true);
        		}

				//infinite ocean warning messages
				GUI.contentColor = colorWarning;
	        	GUI.backgroundColor = colorWarning;
				if (target.typeIndex == 0){
					EditorGUI.LabelField(Rect(rt.x+margin+132, rt.y+basePos+5, 260, 18),"NOTE: Not available in Infinite Ocean Mode!");
				}
				GUI.contentColor = colorEnabled;
	        	GUI.backgroundColor = colorEnabled;


        		basePos += 35;
  				tSpace += 10;
			} else {
				basePos += 25;
			}



			GUILayout.Space(200.0+tSpace);


			
		}
        GUILayout.Space(10.0);



     

        
    if (target.editorIndex == 1){

        //WAVE SETTINGS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showWaves = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showWaves, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("WAVE SETTINGS"));

        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showWaves = !target.showWaves;
		GUI.color.a = 1.0;

        if (target.showWaves){

        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 140, 18),"Wave Scale (Beaufort)");
        	if (!target.customWaves){
        		target.beaufortScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+30, setWidth, 18),"",target.beaufortScale,0.0,12.0);
        	} else {
        		GUI.contentColor = colorWarning;
	        	GUI.backgroundColor = colorWarning;
	        	EditorGUI.LabelField(Rect(rt.x+margin+165, rt.y+30, setWidth, 18),"Disabled: Using custom settings!");
        	}
        	GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;
			//if (target.useTenkoku == 1.0 && target.tenkokuUseWind){
			//	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 90, 18),"Flow Direction");
			//	GUI.contentColor = colorDisabled;
			//	EditorGUI.LabelField(Rect(rt.x+margin+165, rt.y+30, 290, 18),"Currently using Tenkoku settings...");
			//	GUI.contentColor = colorEnabled;
			//	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 90, 18),"Flow Speed");
			//	GUI.contentColor = colorDisabled;
			//	EditorGUI.LabelField(Rect(rt.x+margin+165, rt.y+50, 290, 18),"Currently using Tenkoku settings...");
			//	GUI.contentColor = colorEnabled;
			//} else {
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 90, 18),"Wave Direction");
        		target.flowDirection = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.flowDirection,0.0,360.0);

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+70, 90, 18),"Wave Speed");
				target.flowSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+70, setWidth, 18),"",target.flowSpeed,0.0,0.1);

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+90, 90, 18),"Wave Scale");
				target.waveScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+90, setWidth, 18),"",target.waveScale,0.0,5.0);
        		
        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+110, 110, 18),"Height Projection");
				target.heightProjection = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+110, setWidth, 18),"",target.heightProjection,0.0,1.0);
			//}


			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+135,372,1),divHorizTex);

       		target.customWaves = EditorGUI.Toggle(Rect (rt.x+margin+10, rt.y+140, 20, 20), "", target.customWaves);
        	GUI.Label (Rect (rt.x+margin+30, rt.y+140, 300, 20), GUIContent ("Use Custom Settings"));
        	if (!target.customWaves){
				GUI.contentColor = colorDisabled;
	        	GUI.backgroundColor = colorDisabled;
        	}

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+160, 90, 18),"Wave Height");
				target.waveHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+160, setWidth, 18),"",target.waveHeight,0.0,4.0);

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+180, 120, 18),"Turbulence Amount");
				target.turbulenceFactor = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+180, setWidth, 18),"",target.turbulenceFactor,0.0,1.0);

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+200, 120, 18),"Large Wave Height");
				target.lgWaveHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+200, setWidth, 18),"",target.lgWaveHeight,0.0,4.0);

        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+220, 120, 18),"Large Wave Scale");
				target.lgWaveScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+220, setWidth, 18),"",target.lgWaveScale,0.0,4.0);

				GUILayout.Space(220.0);

        	//} else {
			//	GUILayout.Space(110.0);
        	//}
        	GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;
        }
		GUILayout.Space(10.0);




        //SHORELINE SETTINGS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showShore = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showShore, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("SHORELINE SETTINGS"));
        
        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showShore = !target.showShore;
		GUI.color.a = 1.0;

        if (target.showShore){

    		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 130, 18),"Shoreline Height");
			target.shorelineHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+30, setWidth, 18),"",target.shorelineHeight,0.0,1.0);
    		
    		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 130, 18),"Shoreline Frequency");
			target.shorelineFreq = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.shorelineFreq,0.0,1.0);

        	//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+70, 130, 18),"Shoreline Scale");
			//target.shorelineScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+70, setWidth, 18),"",target.shorelineScale,0.01,4.0);

    		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+70, 130, 18),"Shoreline Speed");
			target.shorelineSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+70, setWidth, 18),"",target.shorelineSpeed,0.0,10.0);

    		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+90, 130, 18),"Shoreline Normalize");
			target.shorelineNorm = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+90, setWidth, 18),"",target.shorelineNorm,0.0,1.0);


			GUILayout.Space(100.0);

    	}
    	GUILayout.Space(10.0);
/*


			if (target.typeIndex != 0 && target.typeIndex != 1) GUI.contentColor = colorDisabled;
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+105, 140, 18),"Height Projection");
				target.projectHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+105, setWidth, 18),"",target.projectHeight,0.0,2.0);
			
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+125, 140, 18),"Wave Height Blend");
				target.colorSurfHigh = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+125, setWidth, 18),"",target.colorSurfHigh);
            	GUI.contentColor = colorEnabled;
            



        		EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+150,372,1),divHorizTex);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+160, 140, 18),"Deep Wave Height");
				target.waveHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+160, setWidth, 18),"",target.waveHeight,0.0,10.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+180, 140, 18),"Deep Wave Scale");
				target.waveScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+180, setWidth, 18),"",target.waveScale,0.0,2.0);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+200, 140, 18),"Detail Wave Height");
				target.detailHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+200, setWidth, 18),"",target.detailHeight,0.0,3.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+220, 140, 18),"Detail Wave Scale");
				target.detailScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+220, setWidth, 18),"",target.detailScale,0.0,1.0);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+240, 140, 18),"Surface Roughness");
				target.surfaceSmooth = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+240, setWidth, 18),"",target.surfaceSmooth,0.0,1.0);



	        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+265,372,1),divHorizTex);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+275, 140, 18),"Normalize Shoreline");
				target.normalShore = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+275, setWidth, 18),"",target.normalShore,0.0,1.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+295, 140, 18),"Shore Wave Height");
				target.waveShoreHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+295, setWidth, 18),"",target.waveShoreHeight,0.0,20.0);
	        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+315, 140, 18),"Shore Wave Scale");
	        	target.waveBreakAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+315, setWidth, 18),"",target.waveBreakAmt,0.0,20.0);
	        	
	        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+335, 140, 18),"Shore Wave Speed");
	        	target.shoreSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+335, setWidth, 18),"",target.shoreSpeed,0.0,1.0);

	*/


		//}
       // GUILayout.Space(10.0);

    //} 
//}
//GUILayout.Space(250.0+tSpace);

        
      
        
        // SURFACE SETTINGS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showSurface = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showSurface, "");
        GUI.Label (Rect(rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("WATER SURFACE"));

		GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showSurface = !target.showSurface;
		GUI.color.a = 1.0;
        
        if (target.showSurface){
        
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 140, 18),"Overall Brightness");
            target.overallBright = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+30, setWidth, 18),"",target.overallBright,0.0,10.0);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 140, 18),"Overall Transparency");
        	target.overallTransparency = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.overallTransparency,0.0,1.0);


			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+75,372,1),divHorizTex);

			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+85, 130, 18),"Edge Blending");
        	target.edgeAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+85, setWidth, 18),"",target.edgeAmt,0.0,1.0);

			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+105, 140, 18),"Depth Absorption");
			target.depthAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+105, setWidth, 18),"",target.depthAmt,0.0,1.0);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+125, 140, 18),"Shallow Absorption");
			target.shallowAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+125, setWidth, 18),"",target.shallowAmt,0.0,1.0);

            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+145, 140, 18),"Depth Color");
            target.depthColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+145, setWidth, 18),"",target.depthColor);
            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+165, 140, 18),"Shallow Color");
            target.shallowColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+165, setWidth, 18),"",target.shallowColor);

        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+185, 130, 18),"Surface Blend Color");
            target.blendColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+185, setWidth, 18),"",target.blendColor);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+205, 130, 18),"Surface Overlay Color");
            target.overlayColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+205, setWidth, 18),"",target.overlayColor);


			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+230,372,1),divHorizTex);


			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+240, 130, 18),"Refraction Amount");
        	target.refractStrength = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+240, setWidth, 18),"",target.refractStrength,0.0,1.0);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+260, 130, 18),"Chromatic Shift");
        	target.aberrationScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+260, setWidth, 18),"",target.aberrationScale,0.0,1.0);

        	if (!target.moduleObject.enableCausticsBlending){
	    		GUI.contentColor = colorDisabled;
	        	GUI.backgroundColor = colorDisabled;
        	}
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+280, 130, 18),"Caustics Blend");
        	target.causticsFade = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+280, setWidth, 18),"",target.causticsFade,0.0,1.0);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+300, 130, 18),"Caustics Color");
            target.causticsColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+300, setWidth, 18),"",target.causticsColor);
	    	GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;

			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+325,372,1),divHorizTex);


			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+335, 130, 18),"Reflection Blur");
        	target.reflectBlur = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+335, setWidth, 18),"",target.reflectBlur,0.0,1.0);	
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+355, 130, 18),"Reflection Distortion");
        	target.reflectProjection = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+355, setWidth, 18),"",target.reflectProjection,0.0,1.0);		
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+375, 130, 18),"Reflection Term");
        	target.reflectTerm = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+375, setWidth, 18),"",target.reflectTerm,0.0,1.0);		
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+395, 130, 18),"Reflection Color");
            target.reflectionColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+395, setWidth, 18),"",target.reflectionColor);

			
			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+420,372,1),divHorizTex);


			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+430, 140, 18),"Hot Specular");
			target.roughness = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+430, setWidth, 18),"",target.roughness,0.0,1.0);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+450, 140, 18),"Wide Specular");
			target.roughness2 = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+450, setWidth, 18),"",target.roughness2,0.0,1.0);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+470, 130, 18),"Specular Color");
            target.specularColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+470, setWidth, 18),"",target.specularColor);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+490, 130, 18),"Back Light Scatter");
            target.sssColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+490, setWidth, 18),"",target.sssColor);




			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+125, 140, 18),"Refraction Amount");
			//target.lightRefract = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+125, setWidth, 18),"",target.lightRefract,0.0,1.0);

			//GUI.contentColor = colorDisabled;
			//GUI.backgroundColor = colorDisabled;
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+145, 140, 18),"Chromatic Shift");
			//target.refractShift = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+145, setWidth, 18),"",target.refractShift,0.0,1.0);
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+165, 140, 18),"Blur Amount");
			//target.blurSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+165, setWidth, 18),"",target.blurSpread,0.0,1.0);
			//GUI.contentColor = colorEnabled;
			//GUI.backgroundColor = colorEnabled;

			//if(!target.enableCausticFX){
			//	GUI.contentColor = colorDisabled;
			//	GUI.backgroundColor = colorDisabled;
			//}
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+185, 140, 18),"Caustics Amount");
			//target.causticSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+185, setWidth, 18),"",target.causticSpread,0.0,1.0);
			//GUI.contentColor = colorEnabled;
			//GUI.backgroundColor = colorEnabled;

			//EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+210,372,1),divHorizTex);

			
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+220, 140, 18),"Reflection Distance");
			//target.reflectDist = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+220, setWidth, 18),"",target.reflectDist,0.0,1.0);
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+240, 140, 18),"Reflection Spread");
			//target.reflectSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+240, setWidth, 18),"",target.reflectSpread,0.0,1.0);
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+260, 140, 18),"Reflection Distortion");
			//target.reflectionOffset = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+260, setWidth, 18),"",target.reflectionOffset,0.0,1.0);
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+280, 140, 18),"Reflection Term");
			//target.reflectionTerm = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+280, setWidth, 18),"",target.reflectionTerm,0.0,1.0);
			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+300, 140, 18),"Reflection Color");
			//target.colorDynReflect = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+300, setWidth, 18),"",target.colorDynReflect);
			

			//EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+328,372,1),divHorizTex);


            //EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+340, 140, 18),"Surface Blend Color");
            //target.depthColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+340, setWidth, 18),"",target.depthColor);
            //EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+360, 140, 18),"Surface Overlay Color");
            //target.colorSurfLow = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+360, setWidth, 18),"",target.colorSurfLow);

            //EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+380, 140, 18),"Distance Fade Color");
            //target.depthColorR = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+380, setWidth, 18),"",target.depthColorR);

            
            //EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+440, 140, 18),"Back Light Scatter");
            //target.specColorL = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+440, setWidth, 18),"",target.specColorL);
            

            //EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+470,372,1),divHorizTex);

            
           	//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+485, 140, 18),"Specular Hot");
            //target.specColorH = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+485, setWidth, 18),"",target.specColorH);
            //EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+505, 140, 18),"Hot Specular Amount");
            //target.specScatterWidth = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+505, setWidth, 18),"",target.specScatterWidth,0.0,1.0);

  			
			GUILayout.Space(495.0);
		}
        GUILayout.Space(10.0);

      
      
    
      
 
      
      
        // FOAM SETTINGS
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showFoam = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showFoam, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("FOAM SETTINGS"));

        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showFoam = !target.showFoam;
		GUI.color.a = 1.0;

        if (target.showFoam){
			
			target.enableFoam = EditorGUI.Toggle(Rect (rt.x+margin+10, rt.y+30, 20, 20),"", target.enableFoam);
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+30, 90, 18),"Enable Foam");
			if (!target.enableFoam){
	    		GUI.contentColor = colorDisabled;
	        	GUI.backgroundColor = colorDisabled;
	        }
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 90, 18),"Foam Scale");
        	target.foamScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.foamScale,0.0,1.0);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+70, 90, 18),"Foam Speed");
        	target.foamSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+70, setWidth, 18),"",target.foamSpeed,0.0,1.0);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+90, 90, 18),"Foam Color");
        	target.foamColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+90, setWidth, 18),"",target.foamColor);
        	
			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+115,372,1),divHorizTex);

        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+125, 90, 18),"Edge Foam");
        	target.edgeFoamAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+125, setWidth, 18),"",target.edgeFoamAmt,0.0,0.9);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+145, 120, 18),"Shoreline Wave Foam");
        	target.shallowFoamAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+145, setWidth, 18),"",target.shallowFoamAmt,0.0,2.0);

			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+170,372,1),divHorizTex);

			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+180, 90, 18),"Wave Foam");
        	target.heightFoamAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+180, setWidth, 18),"",target.heightFoamAmt,0.0,1.0);
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+200, 90, 18),"Wave Height");
        	target.hFoamHeight = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+200, setWidth, 18),"",target.hFoamHeight,0.0,1.0);
        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+220, 90, 18),"Wave Spread");
        	target.hFoamSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+220, setWidth, 18),"",target.hFoamSpread,0.0,1.0);

	    	GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;

			GUILayout.Space(220.0);
		}
        GUILayout.Space(10.0);
        
        


    
        
        
        

        // UNDERWATER SETTINGS
        if (target.enableUnderwaterFX){
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showUnderwater = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showUnderwater, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("UNDERWATER"));

        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showUnderwater = !target.showUnderwater;
		GUI.color.a = 1.0;

        if (target.showUnderwater){

            //GUI.contentColor = colorDisabled;
	        //GUI.backgroundColor = colorDisabled; 
				EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+30, 90, 18),"Enable Debris");
				target.enableUnderDebris = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+30, 30, 18),"", target.enableUnderDebris);
	    	GUI.contentColor = colorEnabled;
	        GUI.backgroundColor = colorEnabled;

			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 130, 18),"Underwater Depth");
            //target.underwaterDepth = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.underwaterDepth,0.0,100.0);

			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+55,372,1),divHorizTex);


			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+65, 130, 18),"Light Factor");
            target.underLightFactor = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+65, setWidth, 18),"",target.underLightFactor,0.0,1.0);

            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+85, 130, 18),"Refraction Amount");
            target.underRefractionAmount = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+85, setWidth, 18),"",target.underRefractionAmount,0.0,0.1);
            
            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+105, 130, 18),"Refraction Scale");
            target.underRefractionScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+105, setWidth, 18),"",target.underRefractionScale,0.0,3.0);
            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+125, 130, 18),"Refraction Speed");
            target.underRefractionSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+125, setWidth, 18),"",target.underRefractionSpeed,0.0,5.0);

           // GUI.contentColor = colorDisabled;
	        //GUI.backgroundColor = colorDisabled;      
          	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+145, 90, 18),"Blur Amount");
            target.underBlurAmount = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+145, setWidth, 18),"",target.underBlurAmount,0.0,1.0);

            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+165, 170, 18),"Depth Darkening Range");
            target.underDarkRange = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+165, setWidth, 18),"",target.underDarkRange,0.0,500.0);
	    	//GUI.contentColor = colorEnabled;
	        //GUI.backgroundColor = colorEnabled;


			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+190,372,1),divHorizTex);


            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+200, 90, 18),"Fog Distance");
			target.underwaterFogDist = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+200, setWidth, 18),"",target.underwaterFogDist,0.0,100.0);
            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+220, 90, 18),"Fog Spread");
			target.underwaterFogSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+220, setWidth, 18),"",target.underwaterFogSpread,-20.0,20.0);

            EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+240, 90, 18),"Fog Color");
			target.underwaterColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+240, setWidth, 18),"",target.underwaterColor);
            
           
            GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
            

			GUILayout.Space(245.0);
		}
        GUILayout.Space(10.0);
        }
        
    }      
    



    	
        // SIMPLE SETTINGS
		if (target.editorIndex == 0){
		
	        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
	        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
	        target.showSimpleEditor = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showSimpleEditor, "");
	        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("SIMPLE WATER SETTINGS"));

	        GUI.color.a = 0.0;
			if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showSimpleEditor = !target.showSimpleEditor;
			GUI.color.a = 1.0;

	        //set settings
			//target.flowSpeed = Mathf.Lerp(0.0,0.2,Mathf.Clamp01(target.waveFlowSpeed*20));
			//target.surfaceSmooth = Mathf.Lerp(0.0,1.0,target.simpleWaveHeight);
			//target.detailHeight = Mathf.Lerp(0.0,1.25,Mathf.Clamp(target.simpleWaveHeight*2,0.0,1.0));
			//target.detailScale = 0.1;
			//target.waveHeight = Mathf.Lerp(0.0,3.0,target.simpleWaveHeight);



	        if (target.showSimpleEditor){

	    		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 160, 18),"Wave Scale (Beaufort)");
        		target.beaufortScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+30, setWidth, 18),"",target.beaufortScale,0.0,20.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 90, 18),"Wave Direction");
        		target.flowDirection = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.flowDirection,0.0,360.0);
        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+70, 90, 18),"Flow Speed");
				target.flowSpeed = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+70, setWidth, 18),"",target.flowSpeed,0.0,0.1);
        		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+90, 90, 18),"Wave Scale");
				target.waveScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+90, setWidth, 18),"",target.waveScale,0.0,1.0);


        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+115,372,1),divHorizTex);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+120, 140, 18),"Refraction Amount");
				target.refractStrength = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+120, setWidth, 18),"",target.refractStrength,0.0,1.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+140, 140, 18),"Reflection Distortion");
				target.reflectProjection = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+140, setWidth, 18),"",target.reflectProjection,0.0,1.0);
				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+160, 140, 18),"Reflection Color");
				target.reflectionColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+160, setWidth, 18),"",target.reflectionColor);

        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+185,372,1),divHorizTex);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+190, 140, 18),"Depth Absorption");
				target.depthAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+190, setWidth, 18),"",target.depthAmt,0.0,1.0);
                EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+210, 140, 18),"Depth Color");
            	target.depthColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+210, setWidth, 18),"",target.depthColor);
				target.shallowColor = Color(0,0,0,0);


        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+235,372,1),divHorizTex);

				EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+240, 90, 18),"Foam Scale");
	        	target.foamScale = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+240, setWidth, 18),"",target.foamScale,0.0,1.0);
	        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+260, 90, 18),"Foam Amount");
        		target.edgeFoamAmt = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+260, setWidth, 18),"",target.edgeFoamAmt,0.0,1.0);
	        	EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+280, 90, 18),"Foam Color");
	        	target.foamColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+280, setWidth, 18),"",target.foamColor);

        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+305,372,1),divHorizTex);

        		if(!target.enableUnderwaterFX){
        		    GUI.contentColor = colorDisabled;
        			GUI.backgroundColor = colorDisabled;
        		}

		        EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+310, 190, 18),"Underwater Refraction");
		        target.underRefractionAmount = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+310, setWidth, 18),"",target.underRefractionAmount,0.0,0.1);
		        EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+330, 190, 18),"Underwater Density");
				target.underwaterFogSpread = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+330, setWidth, 18),"",target.underwaterFogSpread,-20.0,20.0);
		        EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+350, 190, 18),"Underwater Depth Color");
				target.underwaterColor = EditorGUI.ColorField(Rect(rt.x+margin+165, rt.y+350, setWidth, 18),"",target.underwaterColor);      
			    target.underLightFactor = 1.0;
			    target.underRefractionScale = 0.5;
			    target.underRefractionSpeed = 1.0;
			    target.underwaterFogDist = 15.0;

        		GUI.contentColor = colorEnabled;
        		GUI.backgroundColor = colorEnabled;

	
			GUILayout.Space(355.0);
			}

        	GUILayout.Space(10.0);
		
		
   		}

		

//target.testScale = EditorGUI.Slider(Rect(rt.x+margin+10, rt.y+190, 350, 18),"",target.testScale,0.35,0.6);



        // PRESET MANAGER
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        target.showPresets = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showPresets, "");
        GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("PRESET MANAGER"));

        GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 16),"")) target.showPresets = !target.showPresets;
		GUI.color.a = 1.0;

        if (target.showPresets){

			var presetWidth = Screen.width-78;
			if (presetWidth < 120) presetWidth = 120;
		
			//select preset file
			EditorGUI.LabelField(Rect(rt.x+margin+18, rt.y+24, 110, 18),"Use Preset File:");
			target.presetFileIndex = EditorGUI.Popup(Rect(rt.x+margin+125, rt.y+24, 258, 13),"",target.presetFileIndex, target.presetDirs);
    GUI.contentColor = colorDisabled;
    GUI.backgroundColor = colorDisabled;
			EditorGUI.LabelField(Rect(rt.x+margin+18, rt.y+44, 100, 18),"Transition:");
			target.presetTransIndexFrm = EditorGUI.Popup(Rect(rt.x+margin+85, rt.y+44, 80, 13),"",target.presetTransIndexFrm, target.presetOptions);
			EditorGUI.LabelField(Rect(rt.x+margin+167, rt.y+44, 100, 18),"-->");
			target.presetTransIndexTo = EditorGUI.Popup(Rect(rt.x+margin+194, rt.y+44, 80, 13),"",target.presetTransIndexTo, target.presetOptions);
			target.presetTransitionTime = EditorGUI.FloatField(Rect(rt.x+margin+285, rt.y+43, 30, 18),target.presetTransitionTime);
    		var transAction : String = "Start";
        	if (target.presetStartTransition) transAction = (target.presetTransitionCurrent*target.presetTransitionTime).ToString("F2");//"Stop";
        	if(GUI.Button(Rect(rt.x+margin+324, rt.y+44, 60, 15), transAction)){
        		target.presetStartTransition = !target.presetStartTransition;
        	}
    GUI.contentColor = colorEnabled;
    GUI.backgroundColor = colorEnabled;  	
			//}
			
			
			//start presets
			GUI.color = Color(1,1,1,0.1);
			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+20,rt.y+65,presetWidth,5),bgPresetSt); //364
			
			//fill presets
			//presetOptions
			for (var pr:int = 0; pr <= target.presetFiles.length; pr++){
			
				if (pr > 0){
					//background
					GUI.color = Color(1,1,1,0.1);
					if ((pr/2.0) > Mathf.Floor(pr/2.0)) GUI.color = Color(1,1,1,0.13);
					if (target.presetIndex == pr-1) GUI.color = highlightColor;
					EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+20,rt.y+70+(pr*13),presetWidth,12),bgPreset); //364
					

	            	
	            	//rename
	            	GUI.color = Color(1,1,1,0.4);
	            	if (target.presetIndex == pr-1) GUI.color = highlightColor;
					if (GUI.Button(Rect(rt.x+margin+21, rt.y+67+(pr*13)+2, 11, 11),"")){
						Debug.Log("rename");
						setRename = (pr +1);
					}
					if (setRename == (pr+1)){
						renName = EditorGUI.TextField(Rect(rt.x+margin+32, rt.y+69+(pr*13), 200, 14), renName);
						GUI.color = highlightColor2;
						if (GUI.Button(Rect(rt.x+margin+230, rt.y+69+(pr*13), 30, 14),"OK")){
							setRename = 0;
							target.PresetRename(pr-1,renName);
							renName="";
							
						}
						GUI.color = Color(1,1,1,0.4);
						if (GUI.Button(Rect(rt.x+margin+262, rt.y+69+(pr*13), 20, 14),"X")){
							setRename = 0;
						}
	            	}
	            	
	            	//add/delete
	            	GUI.color = Color(1,1,1,0.35);
	            	if (target.presetIndex == pr-1) GUI.color = highlightColor;
					if (GUI.Button(Rect(rt.x+margin+(presetWidth-35), rt.y+68+(pr*13)+1, 25, 12),"+")) target.PresetSave(target.presetFileIndex,pr-1);
	            	if (GUI.Button(Rect(rt.x+margin+(presetWidth-9), rt.y+68+(pr*13)+1, 25, 12),"-")) target.PresetDelete(target.presetFileIndex,pr-1);

	           		GUI.color = Color(1,1,1,1);


					//preset name/button
					if (setRename != (pr+1)){
						GUI.color = Color(1,1,1,0.75);
	            		EditorGUI.LabelField(Rect(rt.x+margin+32, rt.y+67+(pr*13), 300, 16), target.presetFiles[pr-1]);
	            		GUI.color = Color(1,1,1,0.12);
	            		if (GUI.Button(Rect(rt.x+margin+32, rt.y+67+(pr*13)+2, (presetWidth-72), 13),"")){
	            			localPresetIndex = pr;
	            			target.presetIndex = pr-1;
	            			target.PresetLoad(pr-1);
	            		}
	            	}

	           	} else {
	           	
	           	
	           		//background
					GUI.color = Color(1,1,1,0.1);
					if ((pr/2.0) > Mathf.Floor(pr/2.0)) GUI.color = Color(1,1,1,0.13);
					if (target.presetIndex == pr-1) GUI.color = highlightColor;
					EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+20,rt.y+70+(pr*13),presetWidth,12),bgPreset);
					
					//preset name/button
					if (setRename != (pr+1)){
						GUI.color = Color(1,1,1,0.75);
	            		EditorGUI.LabelField(Rect(rt.x+margin+32, rt.y+67+(pr*13), 300, 16), "- NONE -");
	            		GUI.color = Color(0,0,0,0.06);
	            		if (GUI.Button(Rect(rt.x+margin+32, rt.y+67+(pr*13)+2, (presetWidth-15), 13),"")){
	            			localPresetIndex = 0;
	            			target.presetIndex = -1;
	            		}
	            	}

	           	
	           	}
           	}
           	//end presets
           	GUI.color = Color(1,1,1,0.1);
			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+20,rt.y+81+((pr-1)*13),presetWidth,23),bgPresetNd);
			
			GUI.color = Color(1,1,1,1);
			GUI.color = Color(1,1,1,0.55);
			if (GUI.Button(Rect(rt.x+margin+(presetWidth-49), rt.y+86+((pr-1)*13), 65, 18),"+ NEW")) target.PresetAdd();
			
			GUI.color = colorEnabled;
			

			
			GUILayout.Space(80.0+(pr*12)+10);

		}
        GUILayout.Space(10.0);
        

	    EditorGUILayout.Space();


		
        	
        if (GUI.changed) EditorUtility.SetDirty(target);
    }

    
    
}