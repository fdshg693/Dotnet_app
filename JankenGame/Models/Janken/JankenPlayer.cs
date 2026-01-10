namespace JankenGame.Models.Janken
{
    /// <summary>
    /// プレイヤーのタイプを表す列挙型
    /// </summary>
    public enum JankenPlayerType
    {
        Human,
        Computer
    }

    /// <summary>
    /// ジャンケンゲームのプレイヤーを表すクラス
    /// </summary>
    public class JankenPlayer
    {
        /// <summary>
        /// プレイヤーの一意なID
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// プレイヤー名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// プレイヤーのタイプ（ユーザーまたはコンピューター）
        /// </summary>
        public JankenPlayerType Type { get; set; }

        /// <summary>
        /// 出された手
        /// </summary>
        public JankenHand? Hand { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        /// <param name="name">プレイヤー名</param>
        /// <param name="type">プレイヤーのタイプ</param>
        public JankenPlayer(string name = "プレイヤー", JankenPlayerType type = JankenPlayerType.Computer)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Type = type;
            Hand = null;
        }
    }
}

