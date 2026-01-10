namespace JankenGame.Models.Janken
{
    /// <summary>
    /// ジャンケンチャレンジゲームの状態を管理するクラス
    /// </summary>
    public class JankenChallengeGame
    {
        /// <summary>
        /// 現在のスコア
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// コンピューターが出した手
        /// </summary>
        public JankenHand? ComputerHand { get; private set; }

        /// <summary>
        /// ゲームが進行中かどうか
        /// </summary>
        public bool IsPlaying { get; private set; }

        /// <summary>
        /// プレイ回数
        /// </summary>
        public int PlayCount { get; private set; }

        /// <summary>
        /// 正解回数
        /// </summary>
        public int CorrectCount { get; private set; }

        /// <summary>
        /// 不正解回数
        /// </summary>
        public int WrongCount { get; private set; }

        /// <summary>
        /// ゲームを開始する
        /// </summary>
        public void StartGame()
        {
            Score = 0;
            PlayCount = 0;
            CorrectCount = 0;
            WrongCount = 0;
            IsPlaying = true;
            ComputerHand = null;
        }

        /// <summary>
        /// コンピューターの手を設定する
        /// </summary>
        /// <param name="hand">コンピューターの手</param>
        public void SetComputerHand(JankenHand hand)
        {
            ComputerHand = hand;
        }

        /// <summary>
        /// 正解時の処理
        /// </summary>
        /// <param name="points">加算するポイント</param>
        public void OnCorrectAnswer(int points = 10)
        {
            Score += points;
            CorrectCount++;
            PlayCount++;
        }

        /// <summary>
        /// 不正解時の処理
        /// </summary>
        /// <param name="points">減算するポイント</param>
        public void OnWrongAnswer(int points = 5)
        {
            Score -= points;
            WrongCount++;
            PlayCount++;
        }

        /// <summary>
        /// ゲームを終了する
        /// </summary>
        public void EndGame()
        {
            IsPlaying = false;
        }

        /// <summary>
        /// 正解率を計算する
        /// </summary>
        /// <returns>正解率（パーセント）</returns>
        public double GetAccuracyRate()
        {
            if (PlayCount == 0) return 0;
            return (double)CorrectCount / PlayCount * 100;
        }
    }
}
