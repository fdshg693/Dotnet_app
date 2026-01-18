using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// 記事データアクセス用リポジトリインターフェース
/// </summary>
public interface IArticleRepository
{
    /// <summary>
    /// 全記事を取得（削除されていない記事のみ、関連エンティティを含む）
    /// </summary>
    /// <returns>記事のコレクション</returns>
    Task<IEnumerable<Article>> GetAllAsync();
    
    /// <summary>
    /// 公開済み記事を取得（公開日時が現在より前のもの、関連エンティティを含む）
    /// </summary>
    /// <returns>公開記事のコレクション</returns>
    Task<IEnumerable<Article>> GetPublishedAsync();
    
    /// <summary>
    /// IDで記事を取得（関連エンティティを含む）
    /// </summary>
    /// <param name="id">記事ID</param>
    /// <returns>記事、見つからない場合はnull</returns>
    Task<Article?> GetByIdAsync(int id);
    
    /// <summary>
    /// スラッグで記事を取得（公開記事のみ、承認済みコメントを含む）
    /// </summary>
    /// <param name="slug">記事のスラッグ</param>
    /// <returns>記事、見つからない場合はnull</returns>
    Task<Article?> GetBySlugAsync(string slug);
    
    /// <summary>
    /// カテゴリIDで記事を取得（公開記事のみ）
    /// </summary>
    /// <param name="categoryId">カテゴリID</param>
    /// <returns>記事のコレクション</returns>
    Task<IEnumerable<Article>> GetByCategoryIdAsync(int categoryId);
    
    /// <summary>
    /// タグIDで記事を取得（公開記事のみ）
    /// </summary>
    /// <param name="tagId">タグID</param>
    /// <returns>記事のコレクション</returns>
    Task<IEnumerable<Article>> GetByTagIdAsync(int tagId);
    
    /// <summary>
    /// キーワードで記事を検索（タイトル、本文、抜粋が対象、公開記事のみ）
    /// </summary>
    /// <param name="keyword">検索キーワード</param>
    /// <returns>マッチした記事のコレクション</returns>
    Task<IEnumerable<Article>> SearchAsync(string keyword);
    
    /// <summary>
    /// 記事を追加
    /// </summary>
    /// <param name="article">追加する記事</param>
    /// <returns>追加された記事</returns>
    Task<Article> AddAsync(Article article);
    
    /// <summary>
    /// 記事を更新
    /// </summary>
    /// <param name="article">更新する記事</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> UpdateAsync(Article article);
    
    /// <summary>
    /// 記事を削除
    /// </summary>
    /// <param name="id">削除する記事のID</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> DeleteAsync(int id);
    
    /// <summary>
    /// スラッグの一意性をチェック
    /// </summary>
    /// <param name="slug">チェックするスラッグ</param>
    /// <param name="excludeId">除外する記事ID（編集時に自身を除外するため）</param>
    /// <returns>一意の場合true、重複している場合false</returns>
    Task<bool> IsSlugUniqueAsync(string slug, int? excludeId = null);
}
