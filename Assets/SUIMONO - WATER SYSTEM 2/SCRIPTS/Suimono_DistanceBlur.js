#pragma strict


//public variables
var blurAmt : float = 0.0f;
var iterations : int = 3;
var blurSpread : float = 0.6f;
var blurShader : Shader = null;
var material : Material = null;


//private variables
private var offc : float;
private var off : float;
private var rtW : int;
private var rtH : int;
private var buffer : RenderTexture;
private var i : int;
private var buffer2 : RenderTexture;


function Start(){

    if (material == null) {
       material = new Material(blurShader);
        material.hideFlags = HideFlags.DontSave;
    }

    // Disable if we don't support image effects
    if (!SystemInfo.supportsImageEffects) {
        enabled = false;
        return;
    }
    // Disable if the shader can't run on the users graphics card
    if (!blurShader || !material.shader.isSupported) {
        enabled = false;
        return;
    }
}


// Performs one blur iteration.
function FourTapCone (source : RenderTexture, dest : RenderTexture, iteration : int){
    offc = 0.5f + iteration*blurSpread;
    Graphics.BlitMultiTap (source, dest, material,
                           new Vector2(-offc, -offc),
                           new Vector2(-offc,  offc),
                           new Vector2( offc,  offc),
                           new Vector2( offc, -offc)
        );
}

// Downsamples the texture to a quarter resolution.
function DownSample4x (source : RenderTexture, dest : RenderTexture){
    off = blurAmt * 1.4f;//1.0f;
    Graphics.BlitMultiTap (source, dest, material,
                           new Vector2(-off, -off),
                           new Vector2(-off,  off),
                           new Vector2( off,  off),
                           new Vector2( off, -off)
        );
}

// Called by the camera to apply the image effect
function OnRenderImage (source : RenderTexture, destination : RenderTexture) {
    rtW = source.width;// /1;
    rtH = source.height;// /1;
    buffer = RenderTexture.GetTemporary(rtW, rtH, 0);

    iterations = Mathf.Floor(Mathf.Lerp(0,2,blurAmt));
    blurSpread = Mathf.Lerp(0.6,2.0,blurAmt);

    // Copy source to the 4x4 smaller texture.
    DownSample4x (source, buffer);

    // Blur the small texture
    for(i = 0; i < iterations; i++)
    {
        buffer2 = RenderTexture.GetTemporary(rtW, rtH, 0);
        FourTapCone (buffer, buffer2, i);
        RenderTexture.ReleaseTemporary(buffer);
        buffer = buffer2;
    }
    Graphics.Blit(buffer, destination);
    RenderTexture.ReleaseTemporary(buffer);
}
