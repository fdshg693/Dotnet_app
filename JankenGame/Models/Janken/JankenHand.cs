using System.ComponentModel;
using System.Reflection;


namespace JankenGame.Models.Janken
{
    public enum JankenHand
    {
        [Description("グー")]
        Rock = 0,
        [Description("パー")]
        Paper = 1,
        [Description("チョキ")]
        Scissors = 2
    }
}

/// <summary>
/// enum 全般を拡張する静的クラス
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// enum の全ての値を取得する
    /// </summary>
    public static IEnumerable<T> GetValues<T>() where T : Enum
        => Enum.GetValues(typeof(T)).Cast<T>();

    /// <summary>
    /// enum の名前 (識別子) を取得する。ToString() と同じ
    /// </summary>
    public static string GetName<T>(this T value) where T : Enum
        => value.ToString();

    /// <summary>
    /// DescriptionAttribute を付けていればそれを、なければ ToString() を返す
    /// </summary>
    public static string GetDescription<T>(this T value) where T : Enum
    {
        var fi = value.GetType().GetField(value.ToString());
        if (fi == null)
            return value.ToString();
        var attr = fi.GetCustomAttribute<DescriptionAttribute>();
        return attr != null
            ? attr.Description
            : value.ToString();
    }
}