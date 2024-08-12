using COG.Game.CustomWinner.Data;

namespace COG.Game.CustomWinner;

/// <summary>
/// 胜利判定接口
/// </summary>
public interface IWinnable
{
    /// <summary>
    /// 检测胜利
    /// </summary>
    /// <returns>判定数据</returns>
    public void CheckWin(WinnableData data);

    /// <summary>
    /// 返回权重大小
    /// </summary>
    /// <returns>权重</returns>
    public uint GetWeight();

    /// <summary>
    /// 获取顺序
    /// </summary>
    /// <returns></returns>
    public static uint GetOrder(uint order)
    {
        return uint.MaxValue - order;
    }
}