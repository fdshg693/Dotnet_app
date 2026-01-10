using JankenGame.Models.BlackJack;

namespace JankenGame.Services.BlackJack
{
    /// <summary>
    /// ブラックジャックのゲーム進行を統合管理するサービス
    /// DeckManager、StateManager、LogicService、BettingServiceを利用してゲーム全体を制御
    /// </summary>
    public class BlackJackGameService
    {
        private readonly BlackJackDeckManager _deckManager;
        private readonly BlackJackGameStateManager _stateManager;
        private readonly BlackJackLogicService _logicService;
        private readonly BlackJackBettingService _bettingService;

        // ゲーム状態への公開プロパティ（StateManagerへの委譲）
        public BlackJackGameState GameState => _stateManager.GameState;
        public List<BlackJackPlayer> Players => _stateManager.Players;
        public BlackJackDealer Dealer => _stateManager.Dealer;
        public BlackJackPlayer? CurrentPlayer => _stateManager.CurrentPlayer;

        // ベッティング関連の公開プロパティ
        public BlackJackBettingService BettingService => _bettingService;
        public int CurrentBet => _bettingService.CurrentBet;
        public int Pot => _bettingService.Pot;

        public BlackJackGameService()
        {
            _deckManager = new BlackJackDeckManager();
            _stateManager = new BlackJackGameStateManager();
            _logicService = new BlackJackLogicService();
            _bettingService = new BlackJackBettingService();
        }


        /// <summary>
        /// プレイヤーを追加
        /// </summary>
        public void AddPlayer(BlackJackPlayer player)
        {
            _stateManager.AddPlayer(player);
        }

        /// <summary>
        /// ゲームを開始（アンティ徴収と初期配牌）
        /// </summary>
        public void StartGame()
        {
            if (Players.Count == 0)
                throw new InvalidOperationException("プレイヤーが登録されていません");

            // ベッティングラウンドを初期化
            _bettingService.InitializeBettingRound(Players);

            // アンティを徴収
            _stateManager.StartAnte();
            if (!_bettingService.StartAnte())
            {
                throw new InvalidOperationException("全プレイヤーのチップが不足しています");
            }

            // 初期配牌
            _stateManager.StartInitialDeal();

            // 各プレイヤーに2枚配る
            foreach (var player in Players)
            {
                player.ResetCards();
                player.Cards.Add(_deckManager.DrawCard());
                player.Cards.Add(_deckManager.DrawCard());
            }

            // ディーラーに2枚配る
            Dealer.ResetCards();
            Dealer.Cards.Add(_deckManager.DrawCard());
            Dealer.Cards.Add(_deckManager.DrawCard());

            // ベッティングラウンドへ
            _stateManager.StartBettingRound();
        }


        /// <summary>
        /// プレイヤーのベット処理
        /// </summary>
        public bool PlaceBet(int amount, BettingAction action)
        {
            if (GameState != BlackJackGameState.BettingRound || CurrentPlayer == null)
                return false;

            bool success = _bettingService.PlaceBet(CurrentPlayer, amount, action);
            
            if (success)
            {
                // ベッティングラウンドが完了したかチェック
                if (_bettingService.IsBettingRoundComplete())
                {
                    // プレイヤーターンへ移行
                    _stateManager.StartPlayersTurn();
                    CheckCurrentPlayerBlackjackOrBust();
                }
                else
                {
                    // 次のプレイヤーへ移動（BettingServiceとStateManagerを同期）
                    _bettingService.MoveToNextPlayer();
                    _stateManager.SetCurrentPlayerIndex(_bettingService.CurrentPlayerIndex);
                }
            }

            return success;
        }

        /// <summary>
        /// 最小ベット額（コール額）を取得
        /// </summary>
        public int GetMinimumBet()
        {
            if (CurrentPlayer == null)
                return 0;

            return _bettingService.GetMinimumBet(CurrentPlayer);
        }

        /// <summary>
        /// 精算処理を実行
        /// </summary>
        public void ProcessShowdown()
        {
            _stateManager.StartShowdown();

            var activePlayers = _bettingService.GetActivePlayers()
                .Select(s => s.Player)
                .ToList();

            if (activePlayers.Count == 0)
            {
                // 全員フォールド（通常あり得ない）
                EndGame();
                return;
            }

            if (activePlayers.Count == 1)
            {
                // 1人だけ残った場合は自動的に勝利
                var winner = activePlayers[0];
                winner.RecordWin();
                Dealer.RecordDraw();
                _bettingService.DistributePot(new List<BlackJackPlayer> { winner });
                EndGame();
                return;
            }

            // 通常の勝敗判定（LogicServiceに委譲）
            var winners = _logicService.DetermineWinners(activePlayers, Dealer);
            _bettingService.DistributePot(winners);
            EndGame();
        }

        /// <summary>
        /// 現在のプレイヤーがヒット
        /// </summary>
        public void Hit()
        {
            if (GameState != BlackJackGameState.PlayersTurn || CurrentPlayer == null)
                throw new InvalidOperationException("現在はヒットできません");

            CurrentPlayer.Cards.Add(_deckManager.DrawCard());

            // バストチェック
            if (CurrentPlayer.IsBust)
            {
                MoveToNextPlayer();
            }
        }

        /// <summary>
        /// 現在のプレイヤーがスタンド
        /// </summary>
        public void Stand()
        {
            if (GameState != BlackJackGameState.PlayersTurn)
                throw new InvalidOperationException("現在はスタンドできません");

            MoveToNextPlayer();
        }

        /// <summary>
        /// 次のプレイヤーに移動
        /// </summary>
        private void MoveToNextPlayer()
        {
            bool allPlayersFinished = _stateManager.MoveToNextPlayer();

            if (allPlayersFinished)
            {
                // 全プレイヤー終了 → ディーラーのターン
                StartDealerTurn();
            }
            else
            {
                // 次のプレイヤーのブラックジャック・バストチェック
                CheckCurrentPlayerBlackjackOrBust();
            }
        }


        /// <summary>
        /// 現在のプレイヤーがブラックジャックまたはバストなら自動的に次へ
        /// </summary>
        private void CheckCurrentPlayerBlackjackOrBust()
        {
            if (CurrentPlayer == null)
                return;

            if (CurrentPlayer.IsBlackjack || CurrentPlayer.IsBust)
            {
                MoveToNextPlayer();
            }
        }

        /// <summary>
        /// ディーラーのターンを開始
        /// </summary>
        private void StartDealerTurn()
        {
            _stateManager.StartDealerTurn();

            // フォールドしていないプレイヤーを確認
            var activePlayers = _bettingService.GetActivePlayers();
            
            // 全プレイヤーがフォールドまたはバストしていたらディーラーは引かない
            if (activePlayers.Count == 0 || _stateManager.AreAllPlayersBust())
            {
                ProcessShowdown();
                return;
            }

            // ディーラーは17以上になるまで引く
            while (_logicService.ShouldDealerHit(Dealer.Score))
            {
                Dealer.Cards.Add(_deckManager.DrawCard());
            }

            ProcessShowdown();
        }

        /// <summary>
        /// ゲームを終了し、勝敗を判定
        /// </summary>
        private void EndGame()
        {
            _stateManager.EndGame();
            // 勝敗記録はLogicServiceに委譲
            _logicService.RecordGameResults(Players, Dealer);
        }

        /// <summary>
        /// 各プレイヤーの結果メッセージを取得
        /// </summary>
        public string GetResultMessage(BlackJackPlayer player)
        {
            return _logicService.DetermineWinner(
                player.Score,
                Dealer.Score,
                player.IsBust,
                Dealer.IsBust
            );
        }

        /// <summary>
        /// デバッグ用：現在の状態情報を取得
        /// </summary>
        public string GetDebugStatusInfo()
        {
            var lines = new List<string>();
            lines.Add($"=== ゲーム状態デバッグ情報 ===");
            lines.Add($"ゲーム状態: {GameState}");
            lines.Add($"StateManager CurrentPlayerIndex: {_stateManager.CurrentPlayerIndex}");
            lines.Add($"BettingService CurrentPlayerIndex: {_bettingService.CurrentPlayerIndex}");
            lines.Add($"現在のプレイヤー: {CurrentPlayer?.Name ?? "なし"}");
            lines.Add($"ポット: {Pot} チップ");
            lines.Add($"現在のベット: {CurrentBet} チップ");
            lines.Add("");
            lines.Add("プレイヤー状態:");
            
            for (int i = 0; i < Players.Count; i++)
            {
                var player = Players[i];
                var state = _bettingService.GetPlayerState(player);
                string marker = (i == _stateManager.CurrentPlayerIndex) ? "★" : " ";
                lines.Add($"{marker} [{i}] {player.Name}:");
                lines.Add($"    チップ: {player.Chips}, ベット: {player.CurrentBet}");
                if (state != null)
                {
                    lines.Add($"    フォールド: {state.HasFolded}, オールイン: {state.IsAllIn}, 行動済み: {state.HasActedThisRound}");
                }
            }
            
            return string.Join(Environment.NewLine, lines);
        }

        /// <summary>
        /// ゲームをリセット（カードを場から回収）
        /// </summary>
        public void ResetGame()
        {
            // デッキマネージャーでカードをクリア
            _deckManager.ClearCardsInPlay();

            // 状態マネージャーでゲームをリセット
            _stateManager.ResetGame();

            // ベッティングサービスをリセット
            _bettingService.ResetBettingRound();
        }
    }
}
