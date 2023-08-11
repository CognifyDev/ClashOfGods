namespace COG.UI.CustomWinner;

public interface ICustomWinner
{
    /// <summary>
    ///     检测是否可以胜利
    /// </summary>
    /// <returns></returns>
    public bool CanWin();

    /// <summary>
    ///     获取权重
    /// </summary>
    /// <returns></returns>
    public ulong GetWeight();

    public static ulong GetOrder(uint order)
    {
        return uint.MaxValue - order;
    }
}