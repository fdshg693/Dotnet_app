namespace GameEngine.Systems
{
    public static class UserInteraction
    {
        private const int MaxInputAttempts = 5;
        private const int InputTimeoutSeconds = 60;

        /// <summary>
        /// Clears the last line of output from the console.
        /// </summary>
        /// <remarks>This method moves the cursor up by one line and clears the entire line, effectively
        /// removing         the most recent output from the console. It is useful for scenarios where the last output  
        /// needs to be erased or replaced.</remarks>
        public static void ClearLastOutput()
        {
            Console.Write("\x1b[1A");  // 上へカーソル移動
            Console.Write("\x1b[2K");  // 行全体をクリア
        }

        /// <summary>
        /// コンソールから「1以上の整数」を入力させ、正しい値が来るまで繰り返すメソッド
        /// </summary>
        /// <param name="prompt">入力プロンプトメッセージ</param>
        /// <param name="interruptKeyWord">入力をスキップするキーワード（デフォルト: "Q"）</param>
        /// <param name="minValue">許容する最小値（デフォルト: 1）</param>
        /// <param name="maxValue">許容する最大値（デフォルト: int.MaxValue）</param>
        /// <returns>有効な整数値、またはスキップされた場合はnull</returns>
        public static int? ReadPositiveInteger(
            string prompt = "正の整数を入力してください: ", 
            string interruptKeyWord = "Q",
            int minValue = 1,
            int? maxValue = null)
        {
            if (minValue < 1)
                throw new ArgumentException("minValue must be at least 1", nameof(minValue));

            if (maxValue.HasValue && maxValue.Value < minValue)
                throw new ArgumentException("maxValue must be greater than or equal to minValue", nameof(maxValue));

            int attempts = 0;
            string rangeMessage = maxValue.HasValue 
                ? $"{minValue}～{maxValue.Value}の整数を入力してください" 
                : $"{minValue}以上の整数を入力してください";

            while (attempts < MaxInputAttempts)
            {
                attempts++;
                Console.WriteLine($"{interruptKeyWord}を入力することで、入力せずに次に進みます");
                Console.Write(prompt);
                
                string? line = Console.ReadLine();

                // 中断キーワードチェック
                if (!string.IsNullOrWhiteSpace(line) && 
                    line.Trim().Equals(interruptKeyWord, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                // 空入力チェック
                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine($"入力が空です。{rangeMessage}。");
                    continue;
                }

                // 数値変換と範囲チェック
                if (!int.TryParse(line.Trim(), out int value))
                {
                    Console.WriteLine($"'{line}'は有効な整数ではありません。{rangeMessage}。");
                    continue;
                }

                if (value < minValue)
                {
                    Console.WriteLine($"値が小さすぎます（{value} < {minValue}）。{rangeMessage}。");
                    continue;
                }

                if (maxValue.HasValue && value > maxValue.Value)
                {
                    Console.WriteLine($"値が大きすぎます（{value} > {maxValue.Value}）。{rangeMessage}。");
                    continue;
                }

                return value;
            }

            Console.WriteLine($"入力試行回数が上限（{MaxInputAttempts}回）に達しました。操作をスキップします。");
            return null;
        }

        /// <summary>
        /// Yes/No形式の確認入力を受け付ける
        /// </summary>
        /// <param name="prompt">確認メッセージ</param>
        /// <param name="defaultValue">デフォルト値（Enterキー押下時の値）</param>
        /// <returns>Yesの場合true、Noの場合false</returns>
        public static bool ReadConfirmation(string prompt, bool defaultValue = false)
        {
            string defaultText = defaultValue ? "Y/n" : "y/N";
            Console.Write($"{prompt} ({defaultText}): ");

            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return defaultValue;

            string normalized = input.Trim().ToLowerInvariant();
            
            if (normalized == "y" || normalized == "yes" || normalized == "はい")
                return true;
            
            if (normalized == "n" || normalized == "no" || normalized == "いいえ")
                return false;

            // 無効な入力の場合はデフォルト値を返す
            Console.WriteLine($"無効な入力です。デフォルト値（{(defaultValue ? "Yes" : "No")}）を使用します。");
            return defaultValue;
        }

        /// <summary>
        /// 複数の選択肢から1つを選択させる
        /// </summary>
        /// <param name="prompt">選択プロンプト</param>
        /// <param name="options">選択肢の配列</param>
        /// <returns>選択されたインデックス（0始まり）、またはキャンセルされた場合はnull</returns>
        public static int? ReadChoice(string prompt, string[] options, bool allowCancel = true)
        {
            if (options == null || options.Length == 0)
                throw new ArgumentException("Options cannot be null or empty", nameof(options));

            Console.WriteLine(prompt);
            for (int i = 0; i < options.Length; i++)
            {
                Console.WriteLine($"  {i + 1}. {options[i]}");
            }

            if (allowCancel)
                Console.WriteLine($"  0. キャンセル");

            int minValue = allowCancel ? 0 : 1;
            int? choice = ReadPositiveInteger(
                "選択してください: ", 
                "Q", 
                minValue: minValue, 
                maxValue: options.Length);

            if (!choice.HasValue || (allowCancel && choice.Value == 0))
                return null;

            return choice.Value - 1;
        }
        public static string SelectAttackStrategy()
        {
            //Player's turn
            //Choose attack strategy                
            var AttackStrategyArray = new string[] { "Default", "Melee", "Magic" };
            var StrategyIndex = 0;
            Console.WriteLine($"Selected Attack Strategy: {AttackStrategyArray[StrategyIndex]}");

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                if (new[] { ConsoleKey.LeftArrow, ConsoleKey.RightArrow, ConsoleKey.Enter }.Contains(keyInfo.Key))
                {
                    UserInteraction.ClearLastOutput();
                    if (keyInfo.Key == ConsoleKey.LeftArrow)
                    {
                        // カーソルを 1 行上に移動（\x1b[1A）して、その行をクリア（\x1b[2K）

                        StrategyIndex = (StrategyIndex - 1 + AttackStrategyArray.Length) % AttackStrategyArray.Length;
                        Console.WriteLine($"Selected Attack Strategy: {AttackStrategyArray[StrategyIndex]}");
                    }
                    else if (keyInfo.Key == ConsoleKey.RightArrow)
                    {
                        StrategyIndex = (StrategyIndex + 1) % AttackStrategyArray.Length;
                        Console.WriteLine($"Selected Attack Strategy: {AttackStrategyArray[StrategyIndex]}");
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }
            return AttackStrategyArray[StrategyIndex];

        }

        /// <summary>
        /// ゲームアクションを選択する（継続・停止・一時保存）
        /// </summary>
        /// <returns>選択されたアクション: "continue", "quit", "save"</returns>
        public static string SelectGameAction()
        {
            var actionArray = new string[] { "Continue", "Save & Continue", "Save & Quit", "Quit" };
            var actionIndex = 0;
            
            Console.WriteLine("\n--- What would you like to do? ---");
            Console.WriteLine($"> {actionArray[actionIndex]}");

            while (true)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                if (new[] { ConsoleKey.UpArrow, ConsoleKey.DownArrow, ConsoleKey.Enter }.Contains(keyInfo.Key))
                {
                    ClearLastOutput();
                    
                    if (keyInfo.Key == ConsoleKey.UpArrow)
                    {
                        actionIndex = (actionIndex - 1 + actionArray.Length) % actionArray.Length;
                        Console.WriteLine($"> {actionArray[actionIndex]}");
                    }
                    else if (keyInfo.Key == ConsoleKey.DownArrow)
                    {
                        actionIndex = (actionIndex + 1) % actionArray.Length;
                        Console.WriteLine($"> {actionArray[actionIndex]}");
                    }
                    else if (keyInfo.Key == ConsoleKey.Enter)
                    {
                        break;
                    }
                }
            }

            // アクション名を返す
            return actionArray[actionIndex].ToLowerInvariant().Replace(" ", "_").Replace("&_", "");
        }
    }

}
