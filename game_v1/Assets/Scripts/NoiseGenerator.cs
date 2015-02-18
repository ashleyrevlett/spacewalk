using System.IO;
using UnityEngine;
using System.Collections;

public class NoiseGenerator : MonoBehaviour {

	public float seed;
	public int pixWidth = 1000;
	public int pixHeight = 1000;
	public float scale = 10.0F;
	public int octaves = 2;
	public string saveFileName = "noise-image.png";
	private float xOrg = 0;
	private float yOrg = 0;
	private Texture2D noiseTex;
	private Color[] pix;



	public void Generate() {
		seed = Time.time;
		noiseTex = new Texture2D(pixWidth, pixHeight);
		pix = new Color[noiseTex.width * noiseTex.height];
		renderer.material.mainTexture = noiseTex;
		float y = 0.0F;

		while (y < noiseTex.height) {
			float x = 0.0F;
			while (x < noiseTex.width) {
				float sample = 0f;
				float octaveScale = 1f;
				for (int i = 1; i <= octaves; i++) {
					octaveScale = i/octaves;
					float xCoord = xOrg + (x / noiseTex.width * scale/i);
					float yCoord = yOrg + (y / noiseTex.height * scale/i);
//					sample += (Mathf.PerlinNoise(xCoord, yCoord) * 1/(octaves/i));
					sample += Mathf.PerlinNoise(xCoord, yCoord) * octaveScale;
				}
				sample = Mathf.Max(0, Mathf.Min(1, sample));
				int index = (int)(y * noiseTex.width + x);
				pix[index] = new Color(sample, sample, sample);
				x++;
			}
			y++;
		}

		noiseTex.SetPixels(pix);
		noiseTex.Apply();
	}
	
	public void SaveTexture() {
		// nothing
		Texture2D newTexture = new Texture2D(noiseTex.width, noiseTex.height, TextureFormat.ARGB32, false);		
		newTexture.SetPixels(0,0, noiseTex.width, noiseTex.height, noiseTex.GetPixels());
		newTexture.Apply();
		byte[] bytes = newTexture.EncodeToPNG();

		File.WriteAllBytes(Application.dataPath + "/Textures/" + saveFileName, bytes);
		Debug.Log ("Saved file! " + Application.dataPath);

		DestroyImmediate (newTexture);

	}

	public void Clear() {
	
		MeshRenderer renderer = gameObject.GetComponent<MeshRenderer> ();
		renderer.material.mainTexture = null;

	
	}
	

}
