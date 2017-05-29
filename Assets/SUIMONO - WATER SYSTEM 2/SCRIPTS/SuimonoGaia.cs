#if GAIA_PRESENT && UNITY_EDITOR

using UnityEngine;
using System.Collections;
using System;
using System.Reflection;
using UnityEditor;

namespace Gaia.GX.TanukiDigital
{
    /// <summary>
    /// Suimono Setup
    /// </summary>
    public class Suimono_Gaia : MonoBehaviour
    {
        #region Generic informational methods

        /// <summary>
        /// Returns the publisher name if provided. 
        /// This will override the publisher name in the namespace ie Gaia.GX.PublisherName
        /// </summary>
        /// <returns>Publisher name</returns>
        public static string GetPublisherName()
        {
            return "Tanuki Digital";
        }

        /// <summary>
        /// Returns the package name if provided
        /// This will override the package name in the class name ie public class PackageName.
        /// </summary>
        /// <returns>Package name</returns>
        public static string GetPackageName()
        {
            return "Suimono";
        }

        #endregion

        #region Methods exposed by Gaia as buttons must be prefixed with GX_

        public static void GX_About()
        {
            EditorUtility.DisplayDialog("About Suimono", "Suimono is an interactive water system that brings advanced water rendering features to Unity 5.", "OK");
        }

        /// <summary>
        /// Add Suimono to the scene
        /// </summary>
        public static void GX_AddSuimono()
        {

            //Add the Suimono objects to the scene


            //Get scene info
            GaiaSceneInfo sceneInfo = GaiaSceneInfo.GetSceneInfo();


            //INSTALL Suimono Module
            GameObject suimonoPrefab = Gaia.Utils.GetAssetPrefab("SUIMONO_Module");
            if (suimonoPrefab == null)
            {
                Debug.LogWarning("Unable to locate SuimonoModule - Aborting!");
                return;
            }

            //See if we can locate it
            if (GameObject.Find("SUIMONO_Module") != null)
            {
                Debug.LogWarning("Suimono Module already in scene - Aborting!");
                return;
            }

            //See if we can create it
            GameObject suimonoObj = Instantiate(suimonoPrefab);
            if (suimonoObj == null)
            {
                Debug.LogWarning("Unable to create Suimono Module object - Aborting!");
                return;
            }
            else
            {
                suimonoObj.name = "SUIMONO_Module";
            }




            //INSTALL Suimono Surface
            GameObject surfacePrefab = Gaia.Utils.GetAssetPrefab("SUIMONO_Surface");
            if (suimonoPrefab == null)
            {
                Debug.LogWarning("Unable to locate SuimonoSurface - Aborting!");
                return;
            }

            //See if we can locate it
            if (GameObject.Find("SUIMONO_Surface") != null)
            {
                Debug.LogWarning("Suimono Surface already in scene - Aborting!");
                return;
            }

            //See if we can create it
            GameObject surfaceObj = Instantiate(surfacePrefab);
            if (surfaceObj == null)
            {
                Debug.LogWarning("Unable to create Suimono Surface object - Aborting!");
                return;
            }
            else
            {
                //set surface
                surfaceObj.name = "SUIMONO_Surface";

                //set surface position
                surfaceObj.transform.position = new Vector3(0.0f,0.0f,0.0f);

                //Set Water Y-Height and expansion
                if (sceneInfo != null)
                {
                    surfaceObj.transform.position = new Vector3(sceneInfo.m_centrePointOnTerrain.x, sceneInfo.m_seaLevel, sceneInfo.m_centrePointOnTerrain.z);
                    surfaceObj.transform.localScale = sceneInfo.m_sceneBounds.extents*1;
                }


                //set surface-specific settings
                var surfaceObject = surfaceObj.GetComponent("SuimonoObject");
                if (surfaceObject != null)
                {
                    //set surface as infinite ocean
                    FieldInfo typeIndex = surfaceObject.GetType().GetField("typeIndex", BindingFlags.Public | BindingFlags.Instance);
                    if (typeIndex != null) typeIndex.SetValue(surfaceObject, 0);
                }

            }




            //See if we can configure Module - via reflection as JS and C# dont play nice
            var suimonoModule = suimonoObj.GetComponent("SuimonoModule");
            if (suimonoModule != null)
            {


                //Set the camera
                Camera camera = Camera.main;
                if (camera == null)
                {
                    camera = FindObjectOfType<Camera>();
                }
                if (camera != null)
                {
                    FieldInfo manualCamera = suimonoModule.GetType().GetField("manualCamera", BindingFlags.Public | BindingFlags.Instance);
                    if (manualCamera != null)
                    {
                        manualCamera.SetValue(suimonoModule, camera.transform);
                    }
                }


                //Add scene directional light if it exists
                Light lightObj = GameObject.Find("Directional Light").GetComponent<Light>();
                if (lightObj != null)
                {
                    FieldInfo setLight = suimonoModule.GetType().GetField("setLight", BindingFlags.Public | BindingFlags.Instance);
                    if (setLight != null)
                    {
                        setLight.SetValue(suimonoModule, lightObj);
                    }
                }


            }












        }


/*
        public static void GX_SetMorning()
        {
            GameObject tenkokuObj = GameObject.Find("Tenkoku DynamicSky");
            if (tenkokuObj == null)
            {
                Debug.LogWarning("Unable to locate Tenkoku DynamicSky object - Aborting!");
                return;
            }
            //See if we can configure it - via reflection as JS and C# dont play nice
            var tenkokuModule = tenkokuObj.GetComponent("TenkokuModule");
            if (tenkokuModule != null)
            {
                FieldInfo setHour = tenkokuModule.GetType().GetField("currentHour", BindingFlags.Public | BindingFlags.Instance);
                if (setHour != null) setHour.SetValue(tenkokuModule, 7);
                FieldInfo setMin = tenkokuModule.GetType().GetField("currentMinute", BindingFlags.Public | BindingFlags.Instance);
                if (setMin != null) setMin.SetValue(tenkokuModule, 30);
            }
        }



        /// <summary>
        /// Set scene weather to partly cloudy
        /// </summary>
        public static void GX_WeatherPartlyCloudy()
        {
            GameObject tenkokuObj = GameObject.Find("Tenkoku DynamicSky");
            if (tenkokuObj == null)
            {
                Debug.LogWarning("Unable to locate Tenkoku DynamicSky object - Aborting!");
                return;
            }
            //See if we can configure it - via reflection as JS and C# dont play nice
            var tenkokuModule = tenkokuObj.GetComponent("TenkokuModule");
            if (tenkokuModule != null)
            {
                FieldInfo setCloudAlto = tenkokuModule.GetType().GetField("weather_cloudAltoStratusAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setCloudAlto != null) setCloudAlto.SetValue(tenkokuModule, 0.3f);
                FieldInfo setCloudCirrus = tenkokuModule.GetType().GetField("weather_cloudCirrusAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setCloudCirrus != null) setCloudCirrus.SetValue(tenkokuModule, 0.6f);
                FieldInfo setCloudCumulus = tenkokuModule.GetType().GetField("weather_cloudCumulusAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setCloudCumulus != null) setCloudCumulus.SetValue(tenkokuModule, 0.7f);
                FieldInfo setOvercast = tenkokuModule.GetType().GetField("weather_OvercastAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setOvercast != null) setOvercast.SetValue(tenkokuModule, 0.0f);
                FieldInfo setCloudSpd = tenkokuModule.GetType().GetField("weather_cloudSpeed", BindingFlags.Public | BindingFlags.Instance);
                if (setCloudSpd != null) setCloudSpd.SetValue(tenkokuModule, 0.1f);
                FieldInfo setRainAmt = tenkokuModule.GetType().GetField("weather_RainAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setRainAmt != null) setRainAmt.SetValue(tenkokuModule, 0.0f);
                FieldInfo setSnowAmt = tenkokuModule.GetType().GetField("weather_SnowAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setSnowAmt != null) setSnowAmt.SetValue(tenkokuModule, 0.0f);
                FieldInfo setFogAmt = tenkokuModule.GetType().GetField("weather_FogAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setFogAmt != null) setFogAmt.SetValue(tenkokuModule, 0.0f);
                FieldInfo setWindAmt = tenkokuModule.GetType().GetField("weather_WindAmt", BindingFlags.Public | BindingFlags.Instance);
                if (setWindAmt != null) setWindAmt.SetValue(tenkokuModule, 0.25f);

            }
        }

*/



        #endregion
    }
}

#endif