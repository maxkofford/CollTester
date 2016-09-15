using UnityEngine;
using System.Collections;
using System;
using System.IO;

public class GetCollada : MonoBehaviour {
    public GameObject cameraGameObject;
    public Texture2D defaultTexture;
    //public GameObject rulerIndicatorPrototype;
    public Color[] demoColors;

    private string url = @"file:///D:/githubrepos/ModelImportTesting/ModelImporter/Assets/ColladaFiles/medbottle/MedBottle.dae";
    //"http://orbcreation.com/SimpleCollada/mushroom.dae";
    //	Use this when you want to use your own local files
    //	private string url = "file:///ontwikkel/AssetStore/SimpleCollada/colladafiles/heli_dae.dae";
    //  private string url = "file:///ontwikkel/AssetStore/SimpleCollada 5.2/colladafiles/cube.dae";

    private string downloadedPath = "";
    private float importScale = 1f;
    private float importedScale = 1f;
    private Vector3 importTranslation = new Vector3(0, 0, 0);
    private Vector3 importedTranslation = new Vector3(0, 0, 0);
    private Vector3 importRotation = new Vector3(0, 0, 0);
    private Vector3 importedRotation = new Vector3(0, 0, 0);
    private bool importEmptyNodes = true;
    private bool importedEmptyNodes = false;

    private string logMsgs = "";
    private string fileContentString;

    private GameObject targetObject;
    private Bounds overallBounds;
    private float cameraMovement = 0f;

    private string modelInfo = "";
    private GUIStyle rightAligned = null;
    private int screenshotCounter = 0;

    void Start()
    {
        overallBounds = new Bounds(Vector3.zero, Vector3.zero);

        url = @"file:///D:/githubrepos/ModelImportTesting/ModelImporter/Assets/ColladaFiles/medbottle/MedBottle.dae"; 
        url = @"http://orbcreation.com/SimpleCollada/mushroom.dae";
        downloadedPath = url;
        importedTranslation = new Vector3(1,1,1);
        importedScale = 1;
        importedRotation = new Vector3(1,1,1);
        importedEmptyNodes = true;
        StartCoroutine(DownloadAndImportFile(url, Quaternion.Euler(importRotation), new Vector3(importScale, importScale, importScale), importTranslation));

        // set up ruler
        /*
        for (int i = -100; i <= 100; i++)
        {
            if (i != 0 && (i % 10 == 0 || (i > -10 && i < 10)))
            {
                GameObject go = (GameObject)Instantiate(rulerIndicatorPrototype);
                go.GetComponent<TextMesh>().text = "" + i + "m.";
                go.transform.position = new Vector3(i, 0, 0);
                go.name = "x" + i + "m.";
            }
        }
        for (int i = -100; i <= 100; i++)
        {
            if (i != 0 && (i % 10 == 0 || (i > -10 && i < 10)))
            {
                GameObject go = (GameObject)Instantiate(rulerIndicatorPrototype);
                go.GetComponent<TextMesh>().text = "" + i + "m.";
                go.transform.position = new Vector3(0, 0, i);
                go.name = "z" + i + "m.";
            }
        }*/
        ResetCameraPosition();
    }

    void Update()
    {
        // slowly rotate model
        if (targetObject != null)
        {
            Vector3 rotVec = targetObject.transform.rotation.eulerAngles;
            rotVec.y += Time.deltaTime * 20f;
            targetObject.transform.rotation = Quaternion.Euler(rotVec);
        }
        PositionCameraToShowTargetObject();
        if (Input.GetKeyDown(KeyCode.V))
        {
            
            url = @"file:///D:/githubrepos/ModelImportTesting/ModelImporter/Assets/ColladaFiles/medbottle/MedBottle.dae";
            url = @"http://orbcreation.com/SimpleCollada/mushroom.dae";
            downloadedPath = url;
            importedTranslation = new Vector3(1,1,1);
            importedScale = 1;
            importedRotation = new Vector3(1,1,1);
            importedEmptyNodes = true;
            StartCoroutine(DownloadAndImportFile(url, Quaternion.Euler(importRotation), new Vector3(importScale, importScale, importScale), importTranslation));
        }
        if (Input.GetKeyDown(KeyCode.P)) StartCoroutine(Screenshot());
    }

    void OnGUI()
    {
        float margin = 5f;
        float inputLineHeight = 25f;
        float x = margin;
        float y = margin;
        GUI.skin.label.normal.textColor = Color.black;
        GUI.skin.toggle.normal.textColor = Color.black;

        float w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Collada file:");
        x += w + margin;

        w = 350f;
        url = GUI.TextField(new Rect(x, y, w, inputLineHeight), url);
        x += w + margin;
        w = 80;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "Example 1"))
        {
            url = "http://orbcreation.com/SimpleCollada/mushroom.dae";
        }
        x += w + margin;
        w = 25;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "2"))
        {
            url = "http://orbcreation.com/SimpleCollada/cube.dae";
        }
        x += w + margin;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "3"))
        {
            url = "http://orbcreation.com/SimpleCollada/cone.dae";
        }
        x += w + margin;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "4"))
        {
            url = "http://orbcreation.com/SimpleCollada/donut.dae";
        }
        // skinned meshes are not supported
        //		x+=w+margin;
        //		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "5")) {
        //			url = "http://orbcreation.com/SimpleCollada/dummy.dae";
        //		}
        x += w + margin;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "5"))
        {
            url = "http://orbcreation.com/SimpleCollada/hat.dae";
        }
        x += w + margin;
        if (GUI.Button(new Rect(x, y, w, inputLineHeight), "6"))
        {
            url = "http://orbcreation.com/SimpleCollada/wine.dae";
        }
        //		x+=w+margin;
        //		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "7")) {
        //			url = "file:///Users/richardknol/Desktop/test.dae";
        //		}


        //		American prudishness may not be up to this.
        //		x+=w+margin;
        //		if(GUI.Button(new Rect(x,y,w,inputLineHeight), "8")) {
        //			url = "http://orbcreation.com/SimpleCollada/female.dae";
        //		}
        y += inputLineHeight;

        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Rotate X:");
        x += w + margin;
        w = 200f;
        importRotation.x = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importRotation.x, 0f, 360f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importRotation.x));
        x += w + margin;
        y += inputLineHeight;

        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Rotate Y:");
        x += w + margin;
        w = 200f;
        importRotation.y = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importRotation.y, 0f, 360f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importRotation.y));
        x += w + margin;
        y += inputLineHeight;

        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Rotate Z:");
        x += w + margin;
        w = 200f;
        importRotation.z = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importRotation.z, 0f, 360f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importRotation.z));
        x += w + margin;
        y += inputLineHeight;

        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Scale:");
        x += w + margin;
        w = 200f;
        importScale = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importScale, -0.01f, 2.5f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + (Mathf.RoundToInt(importScale * 100) / 100f));
        y += inputLineHeight;

        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Translate X:");
        x += w + margin;
        w = 200f;
        importTranslation.x = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importTranslation.x, -10f, 10f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importTranslation.x));
        x += w + margin;
        y += inputLineHeight;
        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Translate Y:");
        x += w + margin;
        w = 200f;
        importTranslation.y = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importTranslation.y, -10f, 10f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importTranslation.y));
        x += w + margin;
        y += inputLineHeight;
        x = margin;
        w = 80f;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "Translate Z:");
        x += w + margin;
        w = 200f;
        importTranslation.z = GUI.HorizontalSlider(new Rect(x, y + 5, w, inputLineHeight), importTranslation.z, -10f, 10f);
        x += w + margin;
        w = 50 - margin;
        GUI.Label(new Rect(x, y, w, inputLineHeight), "" + Mathf.RoundToInt(importTranslation.z));
        x += w + margin;
        y += inputLineHeight;
        x = margin;
        w = 80f;
        x += w + margin;
        w = 200f;
        importEmptyNodes = GUI.Toggle(new Rect(x, y, w, inputLineHeight), importEmptyNodes, "Import empty nodes");
        y += inputLineHeight;



        if (downloadedPath != url || importedTranslation != importTranslation || importedScale != importScale || importedRotation != importRotation || importedEmptyNodes != importEmptyNodes)
        {
            w = 150;
            if (GUI.Button(new Rect(x, y, w, inputLineHeight), "Download and import"))
            {
                //	Reset();
                downloadedPath = url;
                importedTranslation = importTranslation;
                importedScale = importScale;
                importedRotation = importRotation;
                importedEmptyNodes = importEmptyNodes;
                StartCoroutine(DownloadAndImportFile(url, Quaternion.Euler(importRotation), new Vector3(importScale, importScale, importScale), importTranslation));
            }
            y += inputLineHeight;
        }

        w = 250f;
        if (rightAligned == null)
        {
            rightAligned = new GUIStyle(GUI.skin.label);
            rightAligned.alignment = TextAnchor.UpperRight;
        }
        GUI.Label(new Rect(Screen.width - w - margin, margin, w, 150), modelInfo, rightAligned);
    }


    /* ------------------------------------------------------------------------------------- */
    /* ------------------------------- Downloading files  ---------------------------------- */

    private IEnumerator DownloadAndImportFile(string url, Quaternion rotate, Vector3 scale, Vector3 translate)
    {
        fileContentString = null;
        if (targetObject)
        {
            Destroy(targetObject);
            targetObject = null;
        }
        ResetCameraPosition();
        modelInfo = "";

        yield return StartCoroutine(DownloadFile(url, fileContents => fileContentString = fileContents));
        if (fileContentString != null && fileContentString.Length > 0)
        {
            targetObject = ColladaImporter.Import(fileContentString, rotate, scale, translate, importEmptyNodes);
            yield return StartCoroutine(DownloadTextures(targetObject, url));

            // place the bottom on the floor
            overallBounds = GetBounds(targetObject);
            targetObject.transform.position = new Vector3(0, overallBounds.min.y * -1f, 0);
            overallBounds = GetBounds(targetObject);

            modelInfo = GetModelInfo(targetObject, overallBounds);

            ResetCameraPosition();
        }
    }

    private IEnumerator DownloadFile(string url, System.Action<string> result)
    {
        AddToLog("Downloading " + url);
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null)
        {
            AddToLog(www.error);
        }
        else
        {
            AddToLog("Downloaded " + www.bytesDownloaded + " bytes");
        }
        result(www.text);
    }
    private IEnumerator DownloadTexture(string url, System.Action<Texture2D> result)
    {
        AddToLog("Downloading " + url);
        WWW www = new WWW(url);
        yield return www;
        if (www.error != null)
        {
            AddToLog(www.error);
        }
        else
        {
            AddToLog("Downloaded " + www.bytesDownloaded + " bytes");
        }
        result(www.texture);
    }

    private IEnumerator DownloadTextures(GameObject go, string originalUrl)
    {
        string path = originalUrl;
        int lastSlash = path.LastIndexOf('/', path.Length - 1);
        if (lastSlash >= 0) path = path.Substring(0, lastSlash + 1);
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                if (m.mainTexture != null)
                {
                    Texture2D texture = null;
                    string texUrl = path + m.mainTexture.name;
                    yield return StartCoroutine(DownloadTexture(texUrl, retval => texture = retval));
                    if (texture != null)
                    {
                        m.mainTexture = texture;
                    }
                }
            }
        }
    }

    private void SetTextureInAllMaterials(GameObject go, Texture2D texture)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.mainTexture = texture;
            }
        }
    }

    private void SetColorInAllMaterials(GameObject go, Texture2D texture)
    {
        int i = 0;
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                m.color = demoColors[i++ % demoColors.Length];
            }
        }
    }

    private string GetModelInfo(GameObject go, Bounds bounds)
    {
        string infoString = "";
        int meshCount = 0;
        int subMeshCount = 0;
        int vertexCount = 0;
        int triangleCount = 0;

        MeshFilter[] meshFilters = go.GetComponentsInChildren<MeshFilter>();
        if (meshFilters != null) meshCount = meshFilters.Length;
        foreach (MeshFilter mf in meshFilters)
        {
            Mesh mesh = mf.mesh;
            subMeshCount += mesh.subMeshCount;
            vertexCount += mesh.vertices.Length;
            triangleCount += mesh.triangles.Length / 3;
        }
        infoString = infoString + meshCount + " mesh(es)\n";
        infoString = infoString + subMeshCount + " sub meshes\n";
        infoString = infoString + vertexCount + " vertices\n";
        infoString = infoString + triangleCount + " triangles\n";
        infoString = infoString + bounds.size + " meters";
        return infoString;
    }
    /* ------------------------------------------------------------------------------------- */


    /* ------------------------------------------------------------------------------------- */
    /* --------------------- Position camera to include entire model ----------------------- */
    private Bounds GetBounds(GameObject go)
    {
        Bounds goBounds = new Bounds(go.transform.position, Vector3.zero);
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
        if (renderers != null && renderers.Length > 0) goBounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            Bounds bounds = r.bounds;
            goBounds.Encapsulate(bounds);
        }
        return goBounds;
    }

    private void ResetCameraPosition()
    {
        float nearClip = cameraGameObject.GetComponent<Camera>().nearClipPlane * 1.5f;
        Vector3 camPos = new Vector3(0, 0, nearClip * -1f);
        camPos.y = (overallBounds.size.magnitude) * 0.3f;
        if (camPos.y <= nearClip) camPos.y = nearClip * 1.5f;
        cameraGameObject.transform.position = camPos;
        cameraMovement = 0f;
        QualitySettings.shadowDistance = 10f;
    }

    private void PositionCameraToShowTargetObject()
    {
        if (cameraGameObject != null && targetObject != null)
        {
            cameraGameObject.transform.rotation = Quaternion.LookRotation(overallBounds.center - cameraGameObject.transform.position);
            Vector3 p1 = cameraGameObject.GetComponent<Camera>().WorldToViewportPoint(overallBounds.min);
            Vector3 p2 = cameraGameObject.GetComponent<Camera>().WorldToViewportPoint(overallBounds.max);
            float diff = 0f;
            if (p1.z < 0.02f) diff = Mathf.Max(p1.z * -0.04f, diff);
            if (p2.z < 0.02f) diff = Mathf.Max(p2.z * -0.04f, diff);
            if (p1.x < 0.05f) diff = Mathf.Max(0.05f - p1.x, diff);
            if (p2.x < 0.05f) diff = Mathf.Max(0.05f - p2.x, diff);
            if (p1.x > 0.95f) diff = Mathf.Max(p1.x - 0.95f, diff);
            if (p2.x > 0.95f) diff = Mathf.Max(p2.x - 0.95f, diff);
            if (p1.y < 0.05f) diff = Mathf.Max(0.05f - p1.y, diff);
            if (p2.y < 0.05f) diff = Mathf.Max(0.05f - p2.y, diff);
            if (p1.y > 0.95f) diff = Mathf.Max(p1.y - 0.95f, diff);
            if (p2.y > 0.95f) diff = Mathf.Max(p2.y - 0.95f, diff);
            if (diff > 0f)
            {
                cameraMovement += diff * (overallBounds.size.magnitude) * 0.1f * Time.deltaTime;
                Vector3 camPos = cameraGameObject.transform.position;
                camPos.z -= cameraMovement;
                cameraGameObject.transform.position = camPos;

                QualitySettings.shadowDistance = Mathf.Max(10f, camPos.z * -2.2f);
            }
            else cameraMovement = 0f;
        }
    }

    /* ------------------------------------------------------------------------------------- */


    /* ------------------------------------------------------------------------------------- */
    /* ------------------------------- Logging functions  ---------------------------------- */

    private void AddToLog(string msg)
    {
        Debug.Log(msg + "\n" + DateTime.Now.ToString("yyy/MM/dd hh:mm:ss.fff"));

        // for some silly reason the Editor will generate errors if the string is too long
        int lenNeeded = msg.Length + 1;
        if (logMsgs.Length + lenNeeded > 4096) logMsgs = logMsgs.Substring(0, 4096 - lenNeeded);

        logMsgs = logMsgs + "\n" + msg;
    }

    private string TruncateStringForEditor(string str)
    {
        // for some silly reason the Editor will generate errors if the string is too long
        if (str.Length > 4096) str = str.Substring(0, 4000) + "\n .... display truncated ....\n";
        return str;
    }
    /* ------------------------------------------------------------------------------------- */

    // To make the screenshots used for the Asset Store submission
    private IEnumerator Screenshot()
    {
        yield return new WaitForEndOfFrame(); // wait for end of frame to include GUI

        Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenshot.Apply(false);

        if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.WindowsPlayer && Application.platform != RuntimePlatform.LinuxPlayer || Application.isEditor)
        {
            byte[] bytes = screenshot.EncodeToPNG();
            FileStream fs = new FileStream("Screenshot" + screenshotCounter + ".png", FileMode.OpenOrCreate);
            BinaryWriter w = new BinaryWriter(fs);
            w.Write(bytes);
            w.Close();
            fs.Close();
        }
        screenshotCounter++;

    }


    private   IEnumerator initObject()

    {
        string importString = "";
        string output = Environment.CurrentDirectory;
        string filepath1 = @"\Assets\ColladaFiles\medbottle\MedBottle.dae";
        string filepath2 = @"\Assets\ColladaFiles\ColladaFiles\choppa-dae";
        string filepath3 = @"file:///D:/githubrepos/ModelImportTesting/ModelImporter/Assets/ColladaFiles/medbottle/MedBottle.dae";
        Quaternion rotate = new Quaternion();
        Vector3 scale = new Vector3();
        Vector3 translate = new Vector3();
        GameObject myGameObject = null;
        string fileContentString = null;
        Debug.Log("precoroutine............ ");
        yield return StartCoroutine(DownloadFile(filepath3, fileContents => fileContentString = fileContents));
        if (fileContentString != null && fileContentString.Length > 0)
        {
            Debug.Log("WHY ME............ ");
            myGameObject = ColladaImporter.Import(fileContentString, rotate, scale, translate, true);
            yield return StartCoroutine(DownloadTextures(myGameObject, filepath3));
        }
        Debug.Log("postcreate............ ");
    }
    /*
    void Start() {

        string importString = "";
        string output = Environment.CurrentDirectory;
        string filepath1 = @"\Assets\ColladaFiles\medbottle\MedBottle.dae";
        string filepath2 = @"\Assets\ColladaFiles\ColladaFiles\choppa-dae";
        string filepath3 = @"file:///D:/githubrepos/ModelImportTesting/ModelImporter/Assets/ColladaFiles/medbottle/MedBottle.dae";
        Quaternion rotate = new Quaternion();
        Vector3 scale = new Vector3();
        Vector3 translate = new Vector3();
        GameObject myGameObject = null;


        Debug.Log(Environment.CurrentDirectory+ "\n"); 
       // Debug.Log(output + "\n");
        Debug.Log("PLEASE DONT SUCK YOU PEACE OF PIZZA" + "\n");
        StartCoroutine(initObject());

       
            // Destroy(targetObject);
            // targetObject = null;
           // myGameObject = ColladaImporter.Import(output + filepath3, rotate, scale, translate, false);
    }
   
    // Update is called once per frame
    void Update () {
	
	}
    */
}
