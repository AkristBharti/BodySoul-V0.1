using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 轻微左右呼吸旋转 + 呼吸缩放
/// 适合标题、装饰物、UI 标识
/// </summary>
public class BreathingWobble : MonoBehaviour
{
    [Header("旋转（角度）")]
    public float rotationAmplitude = 5f;     // 最大旋转角度
    public float rotationSpeed = 1f;          // 旋转速度

    [Header("缩放")]
    public float scaleAmplitude = 0.05f;      // 缩放幅度（0.05 = 5%）
    public float scaleSpeed = 1f;              // 缩放速度

    private Quaternion initialRotation;
    private Vector3 initialScale;

    void Awake()
    {
        initialRotation = transform.localRotation;
        initialScale = transform.localScale;
    }

    void Update()
    {
        float time = Time.time;

        // 左右呼吸旋转（Z 轴，UI / 标题常用）
        float rotZ = Mathf.Sin(time * rotationSpeed) * rotationAmplitude;
        transform.localRotation = initialRotation * Quaternion.Euler(0f, 0f, rotZ);

        // 呼吸缩放
        float scaleOffset = Mathf.Sin(time * scaleSpeed) * scaleAmplitude;
        transform.localScale = initialScale * (1f + scaleOffset);
    }
}
