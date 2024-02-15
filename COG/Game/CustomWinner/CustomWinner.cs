namespace COG.Game.CustomWinner;

public interface IWinnable
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

    /// <summary>
    ///     获取名次
    ///     权重建议都用这个，有利于代码维护
    /// </summary>
    /// <param name="order">名次</param>
    /// <returns>名次对应的数值</returns>
    public static ulong GetOrder(uint order)
    {
        return uint.MaxValue - order;
    }
}