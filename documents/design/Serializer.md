# Serializer 設計メモ

## 目的

Editor 内で扱う多様な型を、保存形式（Json / Xml など）に依存せずに柔軟にシリアライズ/デシリアライズできる仕組みを構築する。  
標準ルールで処理できるケースと、型ごとの特殊な変換が必要なケースの両方を、同じパイプラインで扱えるようにする。

## 構成要素

Serializer システムは以下 4 要素から構成する。

### 1. Backend

- シリアライズ形式ごとの実処理を担当する層
- 例: JsonBackend, XmlBackend
- 役割:
  - 中間データ（辞書/配列/プリミティブ）を文字列やバイナリへ変換
  - 逆に文字列/バイナリを中間データへ復元
- 方針:
  - 型の意味（業務知識）は持たない
  - 「形式依存の入出力」に責務を限定する

### 2. Archiver

- 型に応じた格納手順を記述する層
- 役割:
  - どのフィールド/プロパティをどう並べるかを決定
  - 配列/リスト/辞書/複合オブジェクトを再帰的に処理
  - 必要に応じて CustomSerializer や DataTemplate を呼び分ける
- 方針:
  - Backend から独立し、どの形式でも同じ手順ロジックを使える設計にする

### 3. CustomSerializer

- `Serialize` と `Deserialize` を持つ拡張ポイント
- 継承クラスで特定型のシリアライズ規則をカスタマイズ可能にする
- 想定用途:
  - 標準ルールでは扱いにくい型の専用変換
  - バージョン互換や圧縮表現など特殊要件
- 方針:
  - 型変換の知識を明示的に隔離し、Archiver 本体を肥大化させない

### 4. DataTemplate<T>

- ジェネリック型で、ライブラリ内クラスのように `CustomSerializer` を継承できない型に対するカスタマイズ手段
- 役割:
  - 外部型/既存型に対して「型ごとの変換ルール」を後付け登録
- 方針:
  - 継承ベースの拡張ができない型でも同等のカスタム経路を提供する

## 全体フロー

1. 呼び出し側が Serializer に対象オブジェクトを渡す  
2. Archiver が対象型を解析し、適用ルールを選択する
   - `CustomSerializer` があれば優先
   - なければ `DataTemplate<T>` を確認
   - どちらもなければ標準規約で処理
3. Archiver が生成した中間データを Backend に渡す
4. Backend が Json/XML などの最終形式へ変換する

逆方向（Deserialize）も同じ責務分離で処理する。

## 優先順位ルール（案）

- 1: 型専用 `CustomSerializer`
- 2: `DataTemplate<T>`
- 3: Archiver の標準規約

この順序で選択することで、明示的なカスタマイズが常に優先され、想定外の規約処理を避けられる。

## インターフェース設計方針（案）

- `IBackend`
  - 形式ごとの読み書き API を定義
- `IArchiver`
  - オブジェクト <-> 中間データの変換 API を定義
- `CustomSerializer`（抽象クラス）
  - `Serialize` / `Deserialize` を抽象メソッドで提供
- `DataTemplate<T>`
  - 型 T 向けの変換ロジックを保持する登録単位

## API 具体案

`CustomSerializer` と `DataTemplateSerializer` の `Serialize` / `Deserialize` は、  
どちらも `Archiver` を引数に受け取る形にする。

```csharp
public interface IArchiver
{
    IBackend Backend { get; }

    ArchiveNode Archive<T>(T value, string? name = null);
    T Unarchive<T>(ArchiveNode node);

    object? Unarchive(Type type, ArchiveNode node);
}

public interface IBackend
{
    string Name { get; } // "Json", "Xml" など

    string Write(ArchiveNode root);
    ArchiveNode Read(string raw);
}

public sealed class ArchiveNode
{
    public string? Name { get; set; } // プロパティ名/フィールド名
    public Type? ValueType { get; set; } // 実値の型情報
    public object? Value { get; set; } // プリミティブ/文字列/null 等
    public List<ArchiveNode> Children { get; } = []; // 複合型用
    public Dictionary<string, object?> Metadata { get; } = []; // バージョン等
}

public abstract class CustomSerializer
{
    public abstract Type TargetType { get; }

    // Archiver を受け取り、必要に応じて再帰的に Archive/Unarchive を呼ぶ
    public abstract ArchiveNode Serialize(IArchiver archiver, object value, string? name = null);
    public abstract object Deserialize(IArchiver archiver, ArchiveNode node);
}

public abstract class DataTemplateSerializer<T>
{
    public virtual string? TemplateName => typeof(T).FullName;

    // CustomSerializer と同じく Archiver を受け取る
    public abstract ArchiveNode Serialize(IArchiver archiver, T value, string? name = null);
    public abstract T Deserialize(IArchiver archiver, ArchiveNode node);
}
```

### 補足

- `ArchiveNode.Name` はプロパティ名を保持する（例: `"Position"`, `"Items"`）
- `ArchiveNode.Value` は単一値の格納に使う
- 複合型は `Children` に展開し、必要なら `Metadata` に補助情報を持たせる
- `CustomSerializer` は `object` ベース、`DataTemplateSerializer<T>` は強い型付けで扱う

## 拡張性のための設計ポイント

- Backend 追加（Yaml など）を Archiver 非変更で可能にする
- 型ごとの最適化は `CustomSerializer` / `DataTemplate` 側に閉じ込める
- 変換失敗時のエラー情報に「対象型」「パス」「Backend種別」を含める
- 将来的なバージョン移行対応のため、メタ情報保存を考慮する

## 期待効果

- 形式依存の実装と型依存の実装を明確に分離できる
- 外部ライブラリ型を含む幅広い型に対してカスタマイズ可能
- 機能追加時の影響範囲が局所化され、保守性が高い Serializer システムになる

