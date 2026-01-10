using System.ComponentModel;

namespace JankenGame.Models.Janken
{
    public enum JankenResultEnum
    {
        [Description("勝ち")]
        Win = 1,
        [Description("負け")]
        Lose = -1,
        [Description("引き分け")]
        Draw = 0
    }
}
