using JankenGame.Models.BlackJack;

namespace JankenGame.Services.BlackJack
{
    /// <summary>
    /// ベッティングロジックを管理するサービス
    /// </summary>
    public class BlackJackBettingService
    {
        private BettingRound _currentRound;
        private const int AnteAmount = 10;
        private const int MinimumRaise = 10;

        public int CurrentBet => _currentRound.CurrentBet;
        public int Pot => _currentRound.Pot;
        public List<PlayerBettingState> PlayerStates => _currentRound.PlayerStates;
        public int CurrentPlayerIndex => _currentRound.CurrentPlayerIndex;

        public BlackJackBettingService()
        {
            _currentRound = new BettingRound();
        }

        /// <summary>
        /// ベッティングラウンドを初期化
        /// </summary>
        public void InitializeBettingRound(List<BlackJackPlayer> players, int initialChips = 1000)
        {
            _currentRound = new BettingRound();

            foreach (var player in players)
            {
                // プレイヤーの初期チップを設定
                player.Chips = initialChips;
                player.CurrentBet = 0;
                player.HasFolded = false;

                var state = new PlayerBettingState(player, initialChips);
                _currentRound.PlayerStates.Add(state);
            }
        }

        /// <summary>
        /// アンティ（入場料）を徴収
        /// </summary>
        public bool StartAnte()
        {
            foreach (var state in _currentRound.PlayerStates)
            {
                if (state.TotalChips < AnteAmount)
                {
                    // チップ不足のプレイヤーは参加できない
                    state.HasFolded = true;
                    continue;
                }

                state.TotalChips -= AnteAmount;
                state.CurrentBet = AnteAmount;
                state.Player.Chips = state.TotalChips;
                state.Player.CurrentBet = AnteAmount;
                _currentRound.Pot += AnteAmount;
            }

            _currentRound.CurrentBet = AnteAmount;
            return GetActivePlayers().Any();
        }

        /// <summary>
        /// プレイヤーがベット可能か判定
        /// </summary>
        public bool CanPlayerBet(BlackJackPlayer player, int amount)
        {
            var state = GetPlayerState(player);
            if (state == null || state.HasFolded || state.IsAllIn)
                return false;

            return state.TotalChips >= amount;
        }

        /// <summary>
        /// ベットを実行
        /// </summary>
        public bool PlaceBet(BlackJackPlayer player, int amount, BettingAction action)
        {
            var state = GetPlayerState(player);
            if (state == null || state.HasFolded || state.IsAllIn)
                return false;

            switch (action)
            {
                case BettingAction.Fold:
                    state.HasFolded = true;
                    state.Player.HasFolded = true;
                    break;

                case BettingAction.Check:
                    // チェックは現在のベットが0の場合のみ
                    if (_currentRound.CurrentBet > state.CurrentBet)
                        return false;
                    break;

                case BettingAction.Call:
                    int callAmount = _currentRound.CurrentBet - state.CurrentBet;
                    if (callAmount > state.TotalChips)
                    {
                        // オールインに変更
                        return PlaceBet(player, state.TotalChips, BettingAction.AllIn);
                    }
                    state.TotalChips -= callAmount;
                    state.CurrentBet += callAmount;
                    _currentRound.Pot += callAmount;
                    state.Player.Chips = state.TotalChips;
                    state.Player.CurrentBet = state.CurrentBet;
                    break;

                case BettingAction.Raise:
                    int totalRaiseAmount = amount - state.CurrentBet;
                    if (totalRaiseAmount < MinimumRaise || totalRaiseAmount > state.TotalChips)
                        return false;

                    // レイズが発生したら、他のプレイヤーの行動フラグをリセット
                    foreach (var s in _currentRound.PlayerStates)
                    {
                        if (s != state && !s.HasFolded && !s.IsAllIn)
                        {
                            s.HasActedThisRound = false;
                        }
                    }

                    state.TotalChips -= totalRaiseAmount;
                    state.CurrentBet += totalRaiseAmount;
                    _currentRound.Pot += totalRaiseAmount;
                    _currentRound.CurrentBet = state.CurrentBet;
                    state.Player.Chips = state.TotalChips;
                    state.Player.CurrentBet = state.CurrentBet;
                    break;

                case BettingAction.AllIn:
                    int allInAmount = state.TotalChips;
                    state.CurrentBet += allInAmount;
                    state.TotalChips = 0;
                    _currentRound.Pot += allInAmount;
                    state.IsAllIn = true;

                    if (state.CurrentBet > _currentRound.CurrentBet)
                    {
                        _currentRound.CurrentBet = state.CurrentBet;
                        // レイズ扱い
                        foreach (var s in _currentRound.PlayerStates)
                        {
                            if (s != state && !s.HasFolded && !s.IsAllIn)
                            {
                                s.HasActedThisRound = false;
                            }
                        }
                    }

                    state.Player.Chips = 0;
                    state.Player.CurrentBet = state.CurrentBet;
                    break;

                default:
                    return false;
            }

            state.HasActedThisRound = true;
            return true;
        }

        /// <summary>
        /// 現在のプレイヤーを次へ移動
        /// </summary>
        public bool MoveToNextPlayer()
        {
            int startIndex = _currentRound.CurrentPlayerIndex;

            do
            {
                _currentRound.CurrentPlayerIndex = (_currentRound.CurrentPlayerIndex + 1) % _currentRound.PlayerStates.Count;

                var state = _currentRound.PlayerStates[_currentRound.CurrentPlayerIndex];
                if (!state.HasFolded && !state.IsAllIn && !state.HasActedThisRound)
                {
                    return false; // まだ行動していないプレイヤーがいる
                }

                // 一周した
                if (_currentRound.CurrentPlayerIndex == startIndex)
                {
                    break;
                }

            } while (true);

            return true; // 全員行動完了
        }

        /// <summary>
        /// ベッティングラウンドが完了したか判定
        /// </summary>
        public bool IsBettingRoundComplete()
        {
            var activePlayers = GetActivePlayers();
            
            if (activePlayers.Count <= 1)
                return true; // フォールドで1人以下になった

            // 全員が行動済みで、ベット額が揃っているか
            foreach (var state in activePlayers)
            {
                if (!state.HasActedThisRound)
                    return false;

                if (!state.IsAllIn && state.CurrentBet != _currentRound.CurrentBet)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// ポットを分配
        /// </summary>
        public void DistributePot(List<BlackJackPlayer> winners)
        {
            if (!winners.Any())
                return;

            int winAmount = _currentRound.Pot / winners.Count;
            int remainder = _currentRound.Pot % winners.Count;

            for (int i = 0; i < winners.Count; i++)
            {
                var winner = winners[i];
                var state = GetPlayerState(winner);
                if (state != null)
                {
                    int amount = winAmount;
                    if (i == 0) // 最初の勝者に端数を配分
                        amount += remainder;

                    state.TotalChips += amount;
                    winner.Chips = state.TotalChips;
                }
            }

            _currentRound.Pot = 0;
        }

        /// <summary>
        /// アクティブなプレイヤー（フォールドしていない）を取得
        /// </summary>
        public List<PlayerBettingState> GetActivePlayers()
        {
            return _currentRound.PlayerStates
                .Where(s => !s.HasFolded)
                .ToList();
        }

        /// <summary>
        /// 現在のプレイヤー状態を取得
        /// </summary>
        public PlayerBettingState? GetCurrentPlayerState()
        {
            if (_currentRound.CurrentPlayerIndex < 0 || 
                _currentRound.CurrentPlayerIndex >= _currentRound.PlayerStates.Count)
                return null;

            return _currentRound.PlayerStates[_currentRound.CurrentPlayerIndex];
        }

        /// <summary>
        /// 特定のプレイヤーの状態を取得
        /// </summary>
        public PlayerBettingState? GetPlayerState(BlackJackPlayer player)
        {
            return _currentRound.PlayerStates
                .FirstOrDefault(s => s.Player == player);
        }

        /// <summary>
        /// ベッティングラウンドをリセット
        /// </summary>
        public void ResetBettingRound()
        {
            // チップは保持したまま、ベット額とフラグをリセット
            foreach (var state in _currentRound.PlayerStates)
            {
                state.CurrentBet = 0;
                state.HasFolded = false;
                state.IsAllIn = false;
                state.HasActedThisRound = false;
                state.Player.CurrentBet = 0;
                state.Player.HasFolded = false;
            }

            _currentRound.CurrentBet = 0;
            _currentRound.Pot = 0;
            _currentRound.CurrentPlayerIndex = 0;
            _currentRound.RoundComplete = false;
        }

        /// <summary>
        /// 最小ベット額（コール額）を取得
        /// </summary>
        public int GetMinimumBet(BlackJackPlayer player)
        {
            var state = GetPlayerState(player);
            if (state == null || state.HasFolded || state.IsAllIn)
                return 0;

            return _currentRound.CurrentBet - state.CurrentBet;
        }

        /// <summary>
        /// 最小レイズ額を取得
        /// </summary>
        public int GetMinimumRaise()
        {
            return MinimumRaise;
        }

        /// <summary>
        /// アンティ額を取得
        /// </summary>
        public int GetAnteAmount()
        {
            return AnteAmount;
        }
    }
}
