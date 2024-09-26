using UnityEngine;
 
public class Resolution : MonoBehaviour
{
 
    public float width = 16;
    public float height = 9;

    public bool setSafeArea = false;
  
    static float wantedAspectRatio;
    static Camera cam;
    static Camera backgroundCam;
 
    void Awake()
    {
        cam = GetComponent<Camera>();
        if (!cam)
        {
            cam = Camera.main;
        }
        if (!cam)
        {
            Debug.LogError("No camera available");
            return;
        }

        if(!setSafeArea) wantedAspectRatio = width / height;
        
        SetCamera();
    }
 
    void SetCamera()
    {
        if(setSafeArea) {
            cam.rect = new Rect(Screen.safeArea.x / Screen.width, Screen.safeArea.y / Screen.height, Screen.safeArea.width / Screen.width, Screen.safeArea.height / Screen.height);
        } else {
            float currentAspectRatio = (float)Screen.width / Screen.height;

            // 비율이 같을 경우
            if ((int)(currentAspectRatio * 100) / 100.0f == (int)(wantedAspectRatio * 100) / 100.0f)
            {
                cam.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                if (backgroundCam)
                {
                    Destroy(backgroundCam.gameObject);
                }

                return;
            }
            
            // landscape 모드
            if (currentAspectRatio > wantedAspectRatio)
            {
                float inset = 1.0f - wantedAspectRatio / currentAspectRatio;
                cam.rect = new Rect(inset / 2, 0.0f, 1.0f - inset, 1.0f);
            }
            // portrait 모드
            else
            {
                float inset = 1.0f - currentAspectRatio / wantedAspectRatio;
                cam.rect = new Rect(0.0f, inset / 2, 1.0f, 1.0f - inset);
            }
        }

        if (!backgroundCam)
        {
            // Make a new camera behind the normal camera which displays black; otherwise the unused space is undefined
            backgroundCam = new GameObject("BackgroundCam", typeof(Camera)).GetComponent<Camera>();
            backgroundCam.depth = int.MinValue;
            backgroundCam.clearFlags = CameraClearFlags.SolidColor;
            backgroundCam.backgroundColor = Color.black;
            backgroundCam.cullingMask = 0;
        }
    }
 
    public static int screenHeight
    {
        get
        {
            return (int)(Screen.height * cam.rect.height);
        }
    }
 
    public static int screenWidth
    {
        get
        {
            return (int)(Screen.width * cam.rect.width);
        }
    }
 
    public static int xOffset
    {
        get
        {
            return (int)(Screen.width * cam.rect.x);
        }
    }
 
    public static int yOffset
    {
        get
        {
            return (int)(Screen.height * cam.rect.y);
        }
    }
 
    public static Rect screenRect
    {
        get
        {
            return new Rect(cam.rect.x * Screen.width, cam.rect.y * Screen.height, cam.rect.width * Screen.width, cam.rect.height * Screen.height);
        }
    }
 
    public static Vector3 mousePosition
    {
        get
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.y -= (int)(cam.rect.y * Screen.height);
            mousePos.x -= (int)(cam.rect.x * Screen.width);
            return mousePos;
        }
    }
 
    public static Vector2 guiMousePosition
    {
        get
        {
            Vector2 mousePos = Event.current.mousePosition;
            mousePos.y = Mathf.Clamp(mousePos.y, cam.rect.y * Screen.height, cam.rect.y * Screen.height + cam.rect.height * Screen.height);
            mousePos.x = Mathf.Clamp(mousePos.x, cam.rect.x * Screen.width, cam.rect.x * Screen.width + cam.rect.width * Screen.width);
            return mousePos;
        }
    }
}