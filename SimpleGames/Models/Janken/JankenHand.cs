using System.ComponentModel;

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