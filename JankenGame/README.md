# JankenGame

Blazor Webアプリケーションで実装された、ジャンケンとブラックジャックのゲームです。

## 概要

このプロジェクトは、ASP.NET Core 9.0とBlazor Interactive Server Renderingを使用したウェブアプリケーションです。複数のカジュアルゲームをプレイできます。

## 機能

- **ジャンケンゲーム**: コンピューターとの多人数対戦（2〜6人）
- **ジャンケンチャレンジ**: コンピューターの手に勝つ手を選ぶチャレンジモード
- **ブラックジャック**: 多人数対応（1〜5人）の21ゲーム、統計情報表示

## プロジェクト構成

```
Components/
├── Pages/                    # ゲームページコンポーネント
│   ├── Home.razor            # ホームページ
│   ├── Janken.razor          # ジャンケンゲームページ（多人数対応）
│   ├── JankenChallenge.razor # ジャンケンチャレンジページ
│   └── Blackjack.razor       # ブラックジャックゲームページ
├── Layout/                   # レイアウトコンポーネント
│   ├── MainLayout.razor
│   └── NavMenu.razor
└── App.razor                 # ルートコンポーネント

Models/
├── Janken/                   # ジャンケン関連モデル
│   ├── JankenHand.cs
│   ├── JankenPlayer.cs
│   ├── JankenRecord.cs
│   ├── JankenResultEnum.cs
│   ├── JankenChallengeGame.cs
│   ├── JankenGameResult.cs
│   └── MultiPlayerGameRecord.cs
└── BlackJack/                # ブラックジャック関連モデル
    ├── BlackJackParticipant.cs
    ├── BlackJackPlayer.cs
    ├── BlackJackDealer.cs
    ├── BlackJackGameState.cs
    ├── BlackJackGameRecord.cs
    ├── Card.cs
    ├── Deck.cs
    └── Hand.cs

Services/
├── Janken/                   # ジャンケン関連サービス
│   ├── JankenLogicService.cs
│   ├── JankenGameService.cs
│   └── JankenChallengeService.cs
└── BlackJack/                # ブラックジャック関連サービス
    ├── BlackJackGameService.cs
    ├── BlackJackGameStateManager.cs
    ├── BlackJackLogicService.cs
    └── BlackJackDeckManager.cs
```

## 技術スタック

- **フレームワーク**: ASP.NET Core 9.0
- **UI**: Blazor Interactive Server Rendering
- **言語**: C# 13

## 実行方法

アプリケーションを実行するには、以下のコマンドを使用します：

```bash
dotnet run
```

または開発環境では：

```bash
dotnet watch
```

アプリケーションは `https://localhost:5001` にてアクセスできます。