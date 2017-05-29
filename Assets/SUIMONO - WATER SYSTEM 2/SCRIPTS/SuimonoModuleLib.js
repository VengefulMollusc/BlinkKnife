#pragma strict

var texNormalC : Texture2D;
var texHeightC : Texture2D;
var texNormalT : Texture2D;
var texHeightT : Texture2D;
var texNormalR : Texture2D;
var texHeightR : Texture2D;

var texFoam : Texture2D;
var texRampWave : Texture2D;
var texRampDepth : Texture2D;
var texRampBlur : Texture2D;
var texRampFoam : Texture2D;
var texWave : Texture2D;
var texCube1 : Cubemap;
var texBlank : Texture2D;
var texMask : Texture2D;

var texDrops : Texture2D;

var materialSurface : Material;
var materialSurfaceScale : Material;
var materialSurfaceShadow : Material;

var soundObject : Transform;

var meshLevel : Mesh[];
var shaderRepository : Shader[];
var presetRepository : TextAsset[];


