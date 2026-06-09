using UnityEngine;

/// <summary>
/// Pico VR 传送脚本：用控制器射线瞄准地面，扣动扳机后将玩家（XR Origin）移动到瞄准点。
/// 需要导入 Pico Unity Integration SDK。
/// </summary>
public class PicoTeleportOnTrigger : MonoBehaviour
{
    [Header("玩家与控制器")]
    [Tooltip("玩家对象（通常是 XR Origin）")]
    public Transform playerTransform;

    [Tooltip("发射射线的控制器 Transform（如右手控制器的 GameObject）")]
    public Transform rayOrigin;

    public enum HandType
    {
        Left,
        Right
    }

    [Tooltip("使用左手还是右手扳机触发传送")]
    public HandType controllerHand = HandType.Right;

    [Header("地面设置")]
    [Tooltip("地面层级（在 Layer 中选择 Ground 对应的层）")]
    public LayerMask groundLayer = 1;

    [Tooltip("最大射线检测距离")]
    public float maxRayDistance = 100f;

    [Header("视觉反馈（可选）")]
    [Tooltip("传送目标点指示器（一个放在场景中的预制体，如圆环）")]
    public GameObject teleportIndicator;

    private void Start()
    {
        if (teleportIndicator != null)
            teleportIndicator.SetActive(false);
    }

    private void Update()
    {
        // 从控制器位置向前发射射线
        Ray ray = new Ray(rayOrigin.position, rayOrigin.forward);
        RaycastHit hit;
        bool hitGround = Physics.Raycast(ray, out hit, maxRayDistance, groundLayer);

        if (hitGround)
        {
            // 显示传送指示器
            if (teleportIndicator != null)
            {
                teleportIndicator.SetActive(true);
                teleportIndicator.transform.position = hit.point;
                // 让指示器垂直贴合地面（可选）
                teleportIndicator.transform.up = hit.normal;
            }

            // 检测扳机键按下
            if (GetTriggerDown())
            {
                TeleportPlayer(hit.point);
            }
        }
        else
        {
            // 未命中地面时隐藏指示器
            if (teleportIndicator != null)
                teleportIndicator.SetActive(false);
        }
    }

    /// <summary>
    /// 检测指定手柄的扳机是否刚刚按下。
    /// 这里使用 Pico Unity SDK 的 PXR_Input 接口。
    /// </summary>
    private bool GetTriggerDown()
    {
#if PICO_SDK
        PXR_Input.Controller controller = 
            controllerHand == HandType.Right ? 
            PXR_Input.Controller.RightController : 
            PXR_Input.Controller.LeftController;

        return PXR_Input.GetControllerButtonDown(controller, PXR_Input.Button.Trigger);
#else
        // 如果没有 Pico SDK，可退回通用 XR 输入（示例）：
        // 注意：这需要 Unity XR Plugin Management 和 Input System。
        // 实际开发中建议用 Pico SDK 以保证稳定性。
        UnityEngine.XR.InputDevice device = controllerHand == HandType.Right ?
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.RightHand) :
            UnityEngine.XR.InputDevices.GetDeviceAtXRNode(UnityEngine.XR.XRNode.LeftHand);

        bool triggerValue;
        if (device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.triggerButton, out triggerValue))
            return triggerValue;

        return false;
#endif
    }

    /// <summary>
    /// 将玩家瞬移到目标位置。
    /// 玩家 Y 坐标设为地面高度，让角色“站在”地面上。
    /// </summary>
    private void TeleportPlayer(Vector3 targetPoint)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("玩家 Transform 未赋值，无法传送。");
            return;
        }

        Vector3 newPos = targetPoint;
        // 如果你的玩家对象内部结构特殊（例如 Y 轴代表相机高度），
        // 可以改为仅修改 XZ，保留原 Y：
        // newPos.y = playerTransform.position.y;
        playerTransform.position = newPos;
    }

    private void OnDrawGizmosSelected()
    {
        if (rayOrigin != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOrigin.position, rayOrigin.forward * maxRayDistance);
        }
    }
}