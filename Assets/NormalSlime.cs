using UnityEngine;

public class NormalSlime : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        // 如果史莱姆需要立即检测
        SetImmediateDetection(); // 使用基类提供的方法

        base.Start();
    }
}
