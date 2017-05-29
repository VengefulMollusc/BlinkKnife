
//@script ExecuteInEditMode()
@CustomEditor (SuimonoModule)






class suimono_module_editor extends Editor {

	var isPro : boolean = true;
	var showErrors : boolean = false;
	var showUnderwater : boolean = false;
	var colorEnabled : Color = Color(1.0,1.0,1.0,1.0);
	var colorDisabled : Color = Color(1.0,1.0,1.0,0.25);
	var colorWarning : Color = Color(0.9,0.5,0.1,1.0);

	var logoTexb : Texture;// = Resources.Load("textures/gui_tex_suimonologob");
	var divTex : Texture;// = Resources.Load("textures/gui_tex_suimonodiv");
	var divRevTex : Texture;// = Resources.Load("textures/gui_tex_suimonodivrev");
	var divVertTex : Texture;// = Resources.Load("textures/gui_tex_suimono_divvert");
	var divHorizTex : Texture;// = Resources.Load("textures/gui_tex_suimono_divhorz");
	
 	var showCaustic : boolean = false;
 	//var showSplash : boolean = false;
  	//var showWaves : boolean = false;
  	//var showGeneral : boolean = false;
  	//var showColor : boolean = false;
   	//var showReflect : boolean = false;
	//var showUnder : boolean = false;
 	var verAdd : int = 0;
 	
 	
    function OnInspectorGUI () {
    		
    		
        //load textures
		logoTexb = Resources.Load("textures/gui_tex_suimonologob");
		divTex = Resources.Load("textures/gui_tex_suimonodiv");
		divRevTex = Resources.Load("textures/gui_tex_suimonodivrev");
		divVertTex = Resources.Load("textures/gui_tex_suimono_divvert");
		divHorizTex = Resources.Load("textures/gui_tex_suimono_divhorz");

		#if UNITY_PRO_LICENSE
			divTex = Resources.Load("textures/gui_tex_suimonodiv");
			logoTexb = Resources.Load("textures/gui_tex_suimonologob");
		#else
			divTex = Resources.Load("textures/gui_tex_suimonodiv_i");
			logoTexb = Resources.Load("textures/gui_tex_suimonologob_i");	
		#endif


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
		//if (Event.current.type == EventType.MouseUp && linkVerRect.Contains(Event.current.mousePosition)) Application.OpenURL("http://www.tanukidigital.com/suimono/");
		
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

		GUI.contentColor  = Color(1.0,1.0,1.0,1.0);
		

        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,36),logoTexb);
        GUILayout.Space(40.0);
        
      
        rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
        


    
    
        //showGeneral = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 300, 20), showGeneral, "");
        //GUI.Label (Rect (rt.x+margin+20, rt.y+5, 300, 20), GUIContent ("SETTINGS"));
        //if (showGeneral){

			//GUILayout.Space(60.0);
		//}
        //GUILayout.Space(10.0);
        verAdd = 0;
        GUI.contentColor = colorEnabled;
		GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("CONFIGURATION"));


		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+25, 180, 18),"Camera Mode");
		target.cameraTypeIndex = EditorGUI.Popup(Rect(rt.x+margin+165, rt.y+25, 150, 18),"", target.cameraTypeIndex, target.cameraTypeOptions);
		if (target.cameraTypeIndex == 0){
        	GUI.contentColor = colorDisabled;
        	GUI.backgroundColor = colorDisabled;
		}

		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+verAdd+45, 140, 18),"Scene Camera Object");
		target.manualCamera = EditorGUI.ObjectField(Rect(rt.x+margin+165, rt.y+verAdd+45, setWidth, 18),"",target.manualCamera, Transform, true);

        GUI.contentColor = colorEnabled;
        GUI.backgroundColor = colorEnabled;
		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+verAdd+75, 140, 18),"Scene Track Object");
		target.setTrack = EditorGUI.ObjectField(Rect(rt.x+margin+165, rt.y+verAdd+75, setWidth, 18),"",target.setTrack, Transform, true);
		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+verAdd+95, 140, 18),"Scene Light Object");
		target.setLight = EditorGUI.ObjectField(Rect(rt.x+margin+165, rt.y+verAdd+95, setWidth, 18),"",target.setLight, Light, true);
			
		GUILayout.Space(110.0+verAdd);
		






		rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
		target.showGeneral = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showGeneral, "");
       	GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("GENERAL SETTINGS"));
       	
       	GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showGeneral = !target.showGeneral;
		GUI.color.a = 1.0;

       	if (target.showGeneral){
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+30, 140, 18),"Enable Sounds");
			target.playSounds = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+30, setWidth, 18),"", target.playSounds);
			if (!target.playSounds){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+50, 140, 18),"Max Sound Volume");
			target.maxVolume = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+50, setWidth, 18),"",target.maxVolume,0.0,1.0);
			GUI.contentColor = colorEnabled;
			GUI.backgroundColor = colorEnabled;

				EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+70, 160, 18),"Enable Underwater Sound");
				target.playSoundBelow = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+70, 20, 18),"", target.playSoundBelow);
				EditorGUI.LabelField(Rect(rt.x+margin+220, rt.y+70, 160, 18),"Enable Above-Water Sound");
				target.playSoundAbove = EditorGUI.Toggle(Rect(rt.x+margin+200, rt.y+70, 20, 18),"", target.playSoundAbove);

			EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+95,372,1),divHorizTex);

			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+100, 140, 18),"Enable Underwater FX");
			target.enableUnderwaterFX = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+100, 130, 18),"", target.enableUnderwaterFX);
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+120, 140, 18),"Enable Transition FX");		
			target.enableTransition = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+120, 130, 18),"", target.enableTransition);
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+140, 110, 18),"Enable Interaction");
			target.enableInteraction = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+140, 130, 18),"", target.enableInteraction);



			//EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+122,372,1),divHorizTex);

			//EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+130, 140, 18),"FX Offset");
			//target.cameraPlane_offset = EditorGUI.Slider(Rect(rt.x+margin+165, rt.y+130, setWidth, 18),"",target.cameraPlane_offset,0.001,5.0);


			GUILayout.Space(120.0);
		}
		GUILayout.Space(10.0);
		
		



		rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
       	target.showPerformance = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showPerformance, "");
       	GUI.Label (Rect (rt.x+margin+10, rt.y+5, 300, 20), GUIContent ("ADVANCED WATER SETTINGS"));
       	
       	GUI.color.a = 0.0;
		if (GUI.Button(Rect(rt.x+margin+10, rt.y+5, 370, 20),"")) target.showPerformance = !target.showPerformance;
		GUI.color.a = 1.0;


       	if (target.showPerformance){




        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	//EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+26,372,1),divHorizTex);
			target.enableTransparency = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+30, 20, 18),"", target.enableTransparency);
			if (!target.enableTransparency){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+30, 160, 18),"WATER TRANSPARENCY");

			if (!target.enableTransparency){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+230, rt.y+47, 180, 18),"Render Layers");
			if (target.gameObject.activeInHierarchy){
				target.transLayer = EditorGUI.MaskField(Rect(rt.x+margin+230, rt.y+67, 150, 18),"", target.transLayer, target.suiLayerMasks);
			}
			EditorGUI.LabelField(Rect(rt.x+margin+110, rt.y+47, 180, 18),"Use Resolution");
			if (target.gameObject.activeInHierarchy){
				target.transResolution = EditorGUI.Popup(Rect(rt.x+margin+110, rt.y+67, 100, 18),"", target.transResolution, target.resOptions);
			}
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+47, 100, 18),"Distance");
	        target.transRenderDistance = EditorGUI.FloatField(Rect(rt.x+margin+30, rt.y+67, 60, 18),"",target.transRenderDistance);



			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;
        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+95,372,1),divHorizTex);
			target.enableReflections = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+103, 20, 18),"", target.enableReflections);
			if (!target.enableReflections){
				GUI.contentColor = colorDisabled;
				GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+103, 160, 18),"WATER REFLECTIONS");
			EditorGUI.LabelField(Rect(rt.x+margin+50, rt.y+123, 170, 18),"Enable Dynamic Reflections");
			target.enableDynamicReflections = EditorGUI.Toggle(Rect(rt.x+margin+30, rt.y+123, setWidth, 18),"", target.enableDynamicReflections);
        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;




        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+145,372,1),divHorizTex);
			target.enableCaustics = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+150, 20, 18),"", target.enableCaustics);

			if (!target.enableCaustics){
				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}

			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+150, 160, 18),"CAUSTIC FX");

			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+170, 140, 18),"FPS");
			target.causticObject.causticFPS = EditorGUI.IntField(Rect(rt.x+margin+30, rt.y+185, 30, 18),"",target.causticObject.causticFPS);

			EditorGUI.LabelField(Rect(rt.x+margin+90, rt.y+170, 140, 18),"Caustic Tint");
			target.causticObject.causticTint = EditorGUI.ColorField(Rect(rt.x+margin+90, rt.y+187, 120, 14),"",target.causticObject.causticTint);
			
			EditorGUI.LabelField(Rect(rt.x+margin+230, rt.y+170, 100, 18),"Render Layers");
			if (target.gameObject.activeInHierarchy){
				target.causticLayer = EditorGUI.MaskField(Rect(rt.x+margin+230, rt.y+187, 155, 18),"", target.causticLayer, target.suiLayerMasks);
			}

			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+210, 100, 18),"Bright");
			target.causticObject.causticIntensity = EditorGUI.Slider(Rect(rt.x+margin+90, rt.y+210, 120, 18),"",target.causticObject.causticIntensity,0.0,3.0);

			EditorGUI.LabelField(Rect(rt.x+margin+230, rt.y+210, 80, 18),"Scale");
			target.causticObject.causticScale = EditorGUI.Slider(Rect(rt.x+margin+275, rt.y+210, 115, 18),"",target.causticObject.causticScale,0.5,15.0);


			//EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+235, 100, 18),"Scene Light");
			//target.causticObject.sceneLightObject = EditorGUI.ObjectField(Rect(rt.x+margin+110, rt.y+235, 110, 18),"",target.causticObject.sceneLightObject, Light, true);
			
			if (target.setLight == null){
				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
			target.causticObject.inheritLightColor = EditorGUI.Toggle(Rect(rt.x+margin+30, rt.y+235, 120, 18),"", target.causticObject.inheritLightColor);
			EditorGUI.LabelField(Rect(rt.x+margin+50, rt.y+235, 140, 18),"Inherit Light Color");
			target.causticObject.inheritLightDirection = EditorGUI.Toggle(Rect(rt.x+margin+200, rt.y+235, 120, 18),"", target.causticObject.inheritLightDirection);
			EditorGUI.LabelField(Rect(rt.x+margin+220, rt.y+235, 140, 18),"Inherit Light Direction");
        	
        	if (target.enableCaustics){
	        	GUI.contentColor = colorEnabled;
	        	GUI.backgroundColor = colorEnabled;
	        }
			target.enableCausticsBlending = EditorGUI.Toggle(Rect(rt.x+margin+30, rt.y+255, 120, 18),"", target.enableCausticsBlending);
			EditorGUI.LabelField(Rect(rt.x+margin+50, rt.y+255, 320, 18),"Enable Advanced Caustic FX (effects performance)");

        	GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;






        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+285,372,1),divHorizTex);
			target.enableAdvancedDistort = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+290, 20, 18),"", target.enableAdvancedDistort);
			if (!target.enableAdvancedDistort){
				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+290, 340, 18),"ADVANCED WAKE AND DISTORTION EFFECTS");
        	GUI.contentColor = colorDisabled;
        	GUI.backgroundColor = colorDisabled;
        	EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+305, 340, 18),"Enables rendering of advanced scene effects such as wake");
        	EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+317, 340, 18),"and boat trail generation and water ripple distortion fx.");
			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;



        	EditorGUI.DrawPreviewTexture(Rect(rt.x+margin+10,rt.y+340,372,1),divHorizTex);
			target.enableAutoAdvance = EditorGUI.Toggle(Rect(rt.x+margin+10, rt.y+345, 20, 18),"", target.enableAutoAdvance);
			if (!target.enableAutoAdvance){
				GUI.contentColor = colorDisabled;
        		GUI.backgroundColor = colorDisabled;
			}
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+345, 340, 18),"AUTO-ADVANCE SYSTEM TIMER");
	        target.systemTime = EditorGUI.FloatField(Rect(rt.x+margin+260, rt.y+345, 120, 18),"",target.systemTime);
			

        	GUI.contentColor = colorDisabled;
        	GUI.backgroundColor = colorDisabled;
        	EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+360, 340, 18),"the 'systemTime' variable is automatically advanced by");
        	EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+372, 340, 18),"default.  This variable can be shared across a network to");
			EditorGUI.LabelField(Rect(rt.x+margin+30, rt.y+384, 340, 18),"sync wave positions between client and server computers.");


			GUI.contentColor = colorEnabled;
        	GUI.backgroundColor = colorEnabled;


			
			GUILayout.Space(390.0);
				
		}
       	GUILayout.Space(10.0);
			

				






       	if (target.useTenkoku == 1.0){
			rt = GUILayoutUtility.GetRect(buttonText, buttonStyle);
	        EditorGUI.DrawPreviewTexture(Rect(rt.x+margin,rt.y,387,24),divTex);
	       	target.showTenkoku = EditorGUI.Foldout(Rect (rt.x+margin+3, rt.y+5, 20, 20), target.showTenkoku, "");
	       	GUI.Label (Rect (rt.x+margin+20, rt.y+5, 300, 20), GUIContent ("TENKOKU SKY SYSTEM - INTEGRATION"));
	       	 	
	       	if (target.showTenkoku){

	       		EditorGUI.LabelField(Rect(rt.x+margin+10, rt.y+30, 140, 18),"Use Wind Settings");
	       		target.tenkokuUseWind = EditorGUI.Toggle(Rect(rt.x+margin+125, rt.y+30, setWidth, 18),"", target.tenkokuUseWind);
				
				EditorGUI.LabelField(Rect(rt.x+margin+195, rt.y+30, 150, 18),"Calculate Sky Reflections");
	       		target.tenkokuUseReflect = EditorGUI.Toggle(Rect(rt.x+margin+350, rt.y+30, setWidth, 18),"", target.tenkokuUseReflect);

			}
	       	GUILayout.Space(50.0);
       	}
	    GUILayout.Space(10.0);


	        //target.setCamera = EditorGUILayout.ObjectField("Scene Camera Object",target.setCamera, Transform, true);

	        //target.setTrack = EditorGUILayout.ObjectField("Scene Track Object",target.setTrack, Transform, true);

	        //ERROR CHECK (require WATER_Module game object in scene!)
	        //if (GameObject.Find("WATER_Module").gameObject == null){
	        //	EditorGUILayout.HelpBox("A WATER_Module object is required in your scene!",MessageType.Error);
	        //}
	        
	        //SOUND SETTINGS
	        //target.soundObject = EditorGUILayout.ObjectField("Suimono Sound Object",target.soundObject, Transform, true);
	        //target.playSounds = EditorGUILayout.Toggle("Enable Sounds", target.playSounds);
			//target.maxVolume = EditorGUILayout.Slider("Max Sound Volume",target.maxVolume,0.0,1.0);
			//GUILayout.Space(186.0);
			
	        //GENERAL SETTINGS

	        //showUnderwater = EditorGUILayout.Foldout(showUnderwater, "__ UNDERWATER SETTINGS _____________________________________________________________________________");
            //if (showUnderwater){

            
            //if (isPro) EditorGUILayout.LabelField("UNITY BASIC!");
            
            	//if (!target.enableUnderwaterPhysics) GUI.color = colorDisabled;
            	//target.enableUnderwaterPhysics = EditorGUILayout.Toggle("Enable Underwater Physics", target.enableUnderwaterPhysics);
				//GUI.color = colorEnabled;
				
				//if (!target.enableUnderwaterFX) GUI.color = colorDisabled;
            	//target.enableUnderwaterFX = EditorGUILayout.Toggle("Enable Underwater FX", target.enableUnderwaterFX);
				//GUI.color = colorEnabled;
            	
            	
            	
            	
            	
            	/*
            	if (target.enableUnderwaterFX){
            		//target.enableRefraction = EditorGUILayout.Toggle("   Enable Refraction", target.enableRefraction);
            		//if (target.enableRefraction){
            		if (!target.enableRefraction) GUI.color = colorDisabled;
            		EditorGUILayout.BeginHorizontal();
            		target.enableRefraction = EditorGUILayout.Toggle("     Enable Refraction", target.enableRefraction);
            		target.refractionAmount = EditorGUILayout.Slider("",target.refractionAmount,0.0,1.0,GUILayout.Width(220));
            		EditorGUILayout.EndHorizontal();
            		GUI.color = colorEnabled;
            		//if (target.refractionAmount == 0.0) target.enableRefraction = false;
            		//if (target.refractionAmount > 0.0) target.enableRefraction = true;
            		//}
            		
            		//target.enableBlur = EditorGUILayout.Toggle("   Enable Blur", target.enableBlur);
            		//if (target.enableBlur){
            		if (!target.enableBlur) GUI.color = colorDisabled;
            		EditorGUILayout.BeginHorizontal();
            		target.enableBlur = EditorGUILayout.Toggle("     Enable Blur", target.enableBlur);
            		target.blurAmount = EditorGUILayout.Slider("",target.blurAmount,0.0,0.005,GUILayout.Width(220));
            		EditorGUILayout.EndHorizontal();
            		GUI.color = colorEnabled;
            		//if (target.blurAmount == 0.0) target.enableBlur = false;
            		//if (target.blurAmount > 0.0) target.enableBlur = true;
            		//}
            		EditorGUILayout.BeginHorizontal();
            		target.enableAutoColor = EditorGUILayout.Toggle("     Enable Auto-Color", target.enableAutoColor);
            		if (target.enableAutoColor){
            			GUI.color = colorEnabled;
            			EditorGUILayout.LabelField("Auto Color is ON");
            		}
            		if (!target.enableAutoColor){
            			GUI.color = colorDisabled;
            			EditorGUILayout.LabelField("Auto Color is OFF");
            		}
            		GUI.color = colorEnabled;
            		EditorGUILayout.EndHorizontal();
            		
            		if (!target.enableAutoColor){
            			target.underwaterColor = EditorGUILayout.ColorField("          Custom Color",target.underwaterColor);
            		}
            		
            		
            		
            		EditorGUILayout.Space();
            		EditorGUILayout.Space();
            		target.showDebug = EditorGUILayout.Toggle("Show Debug Effects", target.showDebug);
            		
            	}
            	*/
            
            
            //}
            
            /*
            target.lightAbsorb = EditorGUILayout.Slider("Light Absorption",target.lightAbsorb,0.0,1.0);
			target.lightRefract = EditorGUILayout.Slider("Refraction Amount",target.lightRefract,0.0,1.0);
			
			target.overallScale = EditorGUILayout.FloatField("Master Scale",target.overallScale);
			
			target.foamScale = EditorGUILayout.Slider("Foam Scale",target.foamScale,0.0,1.0);
			target.foamAmt = EditorGUILayout.Slider("Foam Amount",target.foamAmt,0.0,1.0);
			target.foamColor = EditorGUILayout.ColorField("Foam Color",target.foamColor);

			target.reflectDist = EditorGUILayout.Slider("Reflection Distance",target.reflectDist,0.0,1.0);
			target.reflectSpread = EditorGUILayout.Slider("Reflection Spread",target.reflectSpread,0.0,1.0);
			target.colorDynReflect = EditorGUILayout.ColorField("Reflection Color",target.colorDynReflect);

			target.surfaceSmooth = EditorGUILayout.Slider("Surface Roughness",target.surfaceSmooth,0.0,1.0);

			


	        //REFLECTION PROPERTIES
	        showUnder = EditorGUILayout.Foldout(showUnder, "__ UNDERWATER PROPERTIES _____________________________________________________________________________");
            if (showUnder){
            	target.etherealShift = EditorGUILayout.Slider("Ethereal Shift",target.etherealShift,0.0,5.0);
            }

	        //COLOR AND SCALING
	        showColor = EditorGUILayout.Foldout(showColor, "__ COLOR / SCALE _____________________________________________________________________________");
	        if (showColor){
				target.depthColor = EditorGUILayout.ColorField("Water Surface Color",target.depthColor);
            }
            
	        //REFLECTION PROPERTIES
	        showReflect = EditorGUILayout.Foldout(showReflect, "__ REFLECTION PROPERTIES _____________________________________________________________________________");
            
	        //WAVE and ANIMATION
	        showWaves = EditorGUILayout.Foldout(showWaves, "__ WAVE ANIMATION _____________________________________________________________________________");
            if(showWaves){
	        	
	        	EditorGUILayout.Space();
        		EditorGUILayout.LabelField("------------------------------------------------------------------------------------------");
        		target.flow_dir = EditorGUILayout.Vector2Field("Flow Direction", target.flow_dir);
        		target.waveSpeed = EditorGUILayout.Slider("Flow Speed",target.waveSpeed,0.0,0.25);
	        	target.foamSpeed = EditorGUILayout.Slider("Foam Speed",target.foamSpeed,0.0,0.5);
	       		EditorGUILayout.LabelField("------------------------------------------------------------------------------------------");
        		EditorGUILayout.Space();
        		
	        }
	        //var flow_dir : Vector2 = Vector2(0.0015,0.0015);
			//var wave_dir : Vector2 = Vector2(0.0015,0.0015);
			//var foam_dir : Vector2 = Vector2(-0.02,-0.02);
			//var water_dir : Vector2 = Vector2(0.0,0.0);



	        
        	
        	//PRESETS
        	showPresets = EditorGUILayout.Foldout(showPresets, "__ PRESETS _____________________________________________________________________________");
            if(showPresets){
            
        		EditorGUILayout.Space();
        		target.presetIndex = EditorGUILayout.Popup("CURRENT PRESET",target.presetIndex, target.presetOptions);
        		
        		
        		EditorGUILayout.LabelField("------------------------------------------------------------------------------------------");
        		target.presetSaveName = EditorGUILayout.TextField("Save New Preset as:", target.presetSaveName);
        		if(GUILayout.Button("Save Preset")){
        			target.PresetSave();
        		}
				
        		//showPresetTrans = EditorGUILayout.Foldout(showPresetTrans,"Manage Preset Transitions");
        		//EditorGUILayout.Space();
        		EditorGUILayout.LabelField("---------------------------------------------------------------------------------");
        		//if (showPresetTrans){
        			target.presetTransIndex = EditorGUILayout.Popup("Transition to Preset",target.presetTransIndex, target.presetOptions);
        		
        		    target.presetTransitionTime = EditorGUILayout.FloatField("Set Transition Time:", target.presetTransitionTime);
        		    EditorGUILayout.LabelField("Transitioning: " + (target.presetTransitionCurrent * target.presetTransitionTime) + " of " + target.presetTransitionTime + " seconds");
        			var transAction : String = "Start";
        			if (target.presetStartTransition) transAction = "Stop";
        			if(GUILayout.Button(transAction + " Transition")){
        				target.presetStartTransition = !target.presetStartTransition;
        			}
        		
        		//}
        	}
        	
        	
        	//SPLASH EFFECTS
        	showSplash = EditorGUILayout.Foldout(showSplash, "__ SPLASH EFFECTS _____________________________________________________________________________");
            if(showSplash){
				target.splashIsOn = EditorGUILayout.Toggle("Splash is Enabled:", target.splashIsOn);
				if (target.splashIsOn){
					target.UpdateSpeed = EditorGUILayout.FloatField("Update Speed:", target.UpdateSpeed);
					target.rippleSensitivity = EditorGUILayout.FloatField("Ripple Sensitivity:", target.rippleSensitivity);
					target.splashSensitivity = EditorGUILayout.FloatField("Splash Sensitivity:", target.splashSensitivity);
				}
			}
			
			
			
        	
        	//display image
			//var fileName = (Application.dataPath + "/" + "SUIMONO - WATER SYSTEM 2" + "/EDITOR/_PRESETS");
			//var previewTex : Texture = Resources.Load(filename);
			//EditorGUILayout.Label(previewTex);
			*/
			
        	
        if (GUI.changed)
            EditorUtility.SetDirty (target);
    }
    
    
    
    
    
}