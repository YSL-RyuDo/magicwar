# Arcana Duel

[English](README.md), [Korean](README.ko.md), [Japanese](README.ja.md)

プログラマー1名とグラフィック担当1名で制作した、マルチプレイヤー魔法バトルゲームです。

| プロジェクト情報 | 内容 |
|---|---|
| 開発状況 | 開発記録 |
| 使用技術 | C++, IOCP, DB worker, Unity |
| 期間とチーム構成 | 2025.08 ~ 2026.02, 2名：プログラマー1名、グラフィック1名 |
| 担当 | プログラミング担当：C++サーバー・Unityゲーム連携 |

[開発事例を見る](https://ysl-ryudo.github.io/ja/work/magicwar/), [Portfolio](https://ysl-ryudo.github.io/ja/)

[![Arcana Duel](https://i.ytimg.com/vi/MJo1y09kzd4/hqdefault.jpg)](https://www.youtube.com/watch?v=MJo1y09kzd4)

[Arcana Duel](https://www.youtube.com/watch?v=MJo1y09kzd4), [Early development demo](https://www.youtube.com/watch?v=8sKpA2H9Ib4)

## 担当した内容

ゲーム・ネットワーク連携とクライアント側の魔法戦闘システムを担当しました。

ダメージ・回復、瞬間移動、透明化、バリア、壁、スロー効果が実装され、対応するサービスは別サーバーで処理します。

ソース履歴に記録されたオーディオ・UIの貢献と外部素材のクレジットを維持しています。

## 貢献の根拠

[Development history / 개발 이력 / 開発履歴](https://github.com/YSL-RyuDo/magicwar/commits/main/?author=YSL-RyuDo)

コミットは記載した変更の根拠であり、貢献率の点数ではありません。外部素材・チームメンバーの貢献は区別します。

## 実装と設計

マルチプレイヤー魔法戦闘では送受信、再利用する接続リソース、DBを使うゲームサービスが共存します。サーバー内の責務の分け方を示す事例です。

IOCPワーカー、セッション・バッファー・Overlappedプール、独立したDBワーカーを構成しました。魔法・クールダウン、試合・プロフィール・ランク、Unityとの連携を担当しました。

| 領域 | 実装 |
|---|---|
| ネットワーク処理 | 接続受付・IOCPワーカーによる送受信完了処理 |
| リソース寿命 | セッション・バッファー・Overlapped I/Oのプール |
| DB処理 | 専用DBワーカーでデータベース要求を処理 |
| ゲーム連携 | 魔法・クールダウン・試合・プロフィール・ランクとUnityを連携 |

## コードの動作と実装構造の理由

確認した実装が扱う制約と利点・代償を説明します。設計効果の説明はコード分析であり、比較する全代案を当時試作したという意味ではありません。担当リンクは共同制作内の変更を示し、ソースの閲覧権限は維持します。

### DB待機とネットワーク完了処理を分ける理由

**実装の流れ**

DBWorkerはミューテックスで保護したキューにタスクを受け取り、条件変数で専用スレッドを起こします。取り出し時だけロックし、解除してからDB処理を実行します。試合結果の保存ではアカウント識別子をタスクに渡します。

**この構造が必要な理由**

DBはソケットの状態と無関係にストレージを待つことがあります。分離によりネットワーク完了スレッドをDB呼出し内で待たせる結合を減らせます。実行前にキューのロックを解除すれば、その処理中も次の要求を投入できます。識別子の受渡しは、遅れて実行される検索の元セッションへの依存も減らします。

**代案と現在の実装の折合い**

DBの直接呼出しはキュー不要ですが保存待ちと通信処理を結びます。複数ワーカーは並列度を上げる代わりに順序と接続の管理を複雑にします。専用ワーカー一つはここで使う単純な分離であり、キューの継続的増加は再検討の根拠になります。

**利点・代償と検証範囲**

単一DBワーカーは順次実行を簡単にしますが、要求がたまるとボトルネックになります。キュー上限、取消、終了時の残タスク完了は別の課題であり、確認したStop実装は全処理完了の保証ではありません。この分離だけでは実測スループットや複数DB更新の原子性も示せません。

[magicwarserver/DBWorker.cpp](https://github.com/YSL-RyuDo/magicwarserver/blob/8089751ae5136876d6fe133fdb980ef24c97684c/projectMagicWar/DBWorker.cpp), [magicwarserver/MatchManager.cpp](https://github.com/YSL-RyuDo/magicwarserver/blob/8089751ae5136876d6fe133fdb980ef24c97684c/projectMagicWar/MatchManager.cpp)

### 1回の受信を1メッセージと見なさない理由

**実装の流れ**

受信バイトをセッションバッファに蓄積し、PacketParserはヘッダーと指定された本文がそろうまで待ちます。完成したメッセージだけを振り分け、残りは次回に保持します。IOCP_Recvは受信バッファとoverlappedオブジェクトをプールへ戻し、次の受信を登録します。

**この構造が必要な理由**

TCPは順序のあるバイトストリームなので、送信と受信の境界は一致しません。通信とゲーム処理の間でメッセージ境界を復元する層が必要です。プールは受信経路の反復割当てを扱い、セッションごとの蓄積は未完のデータを正しい接続に対応させます。

**代案と現在の実装の折合い**

区切り文字方式は読みやすい反面、本文内の区切りのエスケープが必要です。固定長は解析が単純ですが無駄な領域や拡張制限が生じます。現在のヘッダー・本文長方式は可変データを扱う代わりに長さと未完バッファの検証が必要です。

**利点・代償と検証範囲**

層を増やす分、長さ、容量、オブジェクト寿命の管理が必要です。プール容量は割当て設定であり、実証された同時接続人数ではありません。ヘッダー分割、本文分割、複数メッセージの同時受信、I/O待機中の切断が検証対象になります。

[magicwarserver/PacketParser.cpp](https://github.com/YSL-RyuDo/magicwarserver/blob/8089751ae5136876d6fe133fdb980ef24c97684c/projectMagicWar/PacketParser.cpp), [magicwarserver/IOCP_Recv.cpp](https://github.com/YSL-RyuDo/magicwarserver/blob/8089751ae5136876d6fe133fdb980ef24c97684c/projectMagicWar/IOCP_Recv.cpp)

## 実行・コードの確認

[ProjectVersion.txt](ProjectSettings/ProjectVersion.txt)記載のUnity版で開いてください。パッケージロックとシリアライズ済み素材を維持し、Unityを開く前に対象ブランチを選んでください。

Unity scene: [Assets/Scenes/LoginScene.unity](Assets/Scenes/LoginScene.unity)

[magicwarserver](https://github.com/YSL-RyuDo/magicwarserver)

## 開発・検証範囲

過去の開発状態を整理しています。実装範囲の説明であり、新たなプレイ検証や商用処理容量の結果ではありません。クライアントとサーバーの版を合わせてください。

## 関連リポジトリ

[magicwarserver](https://github.com/YSL-RyuDo/magicwarserver), 非公開・アクセス権が必要

[magicwar](https://github.com/YSL-RyuDo/magicwar), 非公開・アクセス権が必要
