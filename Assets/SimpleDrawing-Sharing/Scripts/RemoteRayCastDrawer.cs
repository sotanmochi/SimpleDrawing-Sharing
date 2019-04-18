using UnityEngine;
using Photon.Pun;

namespace SimpleDrawing.Sharing
{
    [RequireComponent(typeof(PhotonView))]
    public class RemoteRayCastDrawer : MonoBehaviour, IPunObservable
    {
        private enum RayDirectionType
        {
            TransformForward,
            TransformBackward,
            TransformRight,
            TransformLeft,
            TransformUp,
            TransformDown,
        }

        public bool RayCastEnabled = true;
        public Color PenColor = Color.cyan;
        public int PenWidth = 3;
        public bool Erase = false;

        [SerializeField]
        RayDirectionType directionType = RayDirectionType.TransformDown;
        [SerializeField]
        float rayDistance = 5.0f;

        Vector2 defaultTexCoord = Vector2.zero;

        Material material;
        PhotonView photonView;

        int targetCanvasId;
        Vector2 currentTexCoord;
        Vector2 previousTexCoord;

        bool receivedRayCastEnabled;
        float receivedPenColorR;
        float receivedPenColorG;
        float receivedPenColorB;
        float receivedPenColorA;
        int receivedPenWidth;
        bool receivedErase;
        int receivedTargetCanvasId;
        Vector2 receivedCurrentTexCoord;
        Vector2 receivedPreviousTexCoord;

		void Start()
		{
            photonView = GetComponent<PhotonView>();
		}

        void Update()
        {
            if (!photonView.IsMine)
            {
                RayCastEnabled = receivedRayCastEnabled;
                PenColor.r = receivedPenColorR;
                PenColor.g = receivedPenColorG;
                PenColor.b = receivedPenColorB;
                PenColor.a = receivedPenColorA;
                PenWidth = receivedPenWidth;
                Erase = receivedErase;
                targetCanvasId = receivedTargetCanvasId;
                currentTexCoord = receivedCurrentTexCoord;
                previousTexCoord = receivedPreviousTexCoord;

                if (RayCastEnabled)
                {
                    var target = PhotonView.Find(targetCanvasId);
                    if (target != null)
                    {
                        var drawObject = target.transform.GetComponent<DrawableCanvas>();
                        if (drawObject != null)
                        {
                            if (Erase)
                            {
                                drawObject.Erase(currentTexCoord, previousTexCoord, PenWidth);
                            }
                            else
                            {
                                drawObject.Draw(currentTexCoord, previousTexCoord, PenWidth, PenColor);
                            }
                        }
                    }
                }
            }
            else
            {
                if (RayCastEnabled)
                {
                    var ray = new Ray(this.transform.position, GetCurrentDirection());
                    RaycastHit hitInfo;
                    if(Physics.Raycast(ray, out hitInfo, rayDistance))
                    {
                        if(hitInfo.collider != null && hitInfo.collider is MeshCollider)
                        {
                            var drawObject = hitInfo.transform.GetComponent<DrawableCanvas>();
                            if (drawObject != null)
                            {
                                var target = drawObject.transform.GetComponent<PhotonView>();
                                targetCanvasId = (target != null) ? target.ViewID : -1;

                                previousTexCoord = currentTexCoord;
                                currentTexCoord = hitInfo.textureCoord;
                                if (Erase)
                                {
                                    drawObject.Erase(currentTexCoord, previousTexCoord, PenWidth);
                                }
                                else
                                {
                                    drawObject.Draw(currentTexCoord, previousTexCoord, PenWidth, PenColor);
                                }
                            }
                        }
                        else
                        {
                            Debug.LogWarning("If you want to draw using a RaycastHit, need set MeshCollider for object.");
                        }
                    }
                    else
                    {
                        targetCanvasId = -1;
                        previousTexCoord = defaultTexCoord;
                    }
                }
                else
                {
                    targetCanvasId = -1;
                    previousTexCoord = defaultTexCoord;
                }
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(RayCastEnabled);
                stream.SendNext(PenColor.r);
                stream.SendNext(PenColor.g);
                stream.SendNext(PenColor.b);
                stream.SendNext(PenColor.a);
                stream.SendNext(PenWidth);
                stream.SendNext(Erase);
                stream.SendNext(targetCanvasId);
                stream.SendNext(currentTexCoord);
                // stream.SendNext(previousTexCoord);
            }
            else
            {
                receivedPreviousTexCoord = receivedCurrentTexCoord;

                receivedRayCastEnabled = (bool)stream.ReceiveNext();
                receivedPenColorR = (float)stream.ReceiveNext();
                receivedPenColorG = (float)stream.ReceiveNext();
                receivedPenColorB = (float)stream.ReceiveNext();
                receivedPenColorA = (float)stream.ReceiveNext();
                receivedPenWidth = (int)stream.ReceiveNext();
                receivedErase = (bool)stream.ReceiveNext();
                receivedTargetCanvasId = (int)stream.ReceiveNext();
                receivedCurrentTexCoord = (Vector2)stream.ReceiveNext();
                // receivedPreviousTexCoord = (Vector2)stream.ReceiveNext();
            }
        }

        private Vector3 GetCurrentDirection()
        {
            Vector3 direction = Vector3.zero;
            switch(directionType)
            {
                case RayDirectionType.TransformForward:
                    direction =  this.transform.forward;
                    break;
                case RayDirectionType.TransformBackward:
                    direction = -this.transform.forward;
                    break;
                case RayDirectionType.TransformRight:
                    direction =  this.transform.right;
                    break;
                case RayDirectionType.TransformLeft:
                    direction = -this.transform.right;
                    break;
                case RayDirectionType.TransformUp:
                    direction =  this.transform.up;
                    break;
                case RayDirectionType.TransformDown:
                    direction = -this.transform.up;
                    break;
            }
            return direction;
        }
    }
}
