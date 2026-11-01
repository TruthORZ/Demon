using Demon.Slice.Domain;
using UnityEngine;
namespace Demon.Slice
{
    [CreateAssetMenu(menuName = "Demon/Bearing Cipher Definition")]
    public sealed class CipherDefinition : ScriptableObject
    {
        [Tooltip("Digits clockwise from the fixed north marker. Must contain 0-9 exactly once.")]
        public int[] DigitsClockwise = { 4, 7, 2, 9, 1, 6, 8, 0, 5, 3 };
        [TextArea(3, 6)] public string WheelInstructions = "轮盘北标与场地方向一致。\n观察电台、太阳、风向标相对轮盘的物体位置，各对应一个数字。\n按箱面符号顺序输入。风向标叶片的指向不参与密码。\nEsc 放下轮盘。";
        [TextArea(2, 4)] public string LockInstructions = "箱子 A · 三位机械锁\n顺序：电台 → 太阳 → 风向标\n点击每位数字加一（9后回到0），Enter 尝试；Esc 返回，输入会保留。";
        [TextArea(2, 4)] public string UnlockedText = "箱子 A 已打开。里面是 T06，靠近按 E 取出。";
    }
}
