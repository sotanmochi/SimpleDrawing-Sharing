using UnityEngine;

namespace SimpleDrawing.Sharing
{
    public class DrawerController : MonoBehaviour
    {
        RemoteRayCastDrawer drawer;
        ColorPickerTriangle colorPicker;

        void Start()
        {
            drawer = transform.GetComponent<RemoteRayCastDrawer>();
            colorPicker = Camera.main.GetComponentInChildren<ColorPickerTriangle>();
        }

        void Update()
        {
            drawer.PenColor = colorPicker.TheColor;
        }
    }
}
