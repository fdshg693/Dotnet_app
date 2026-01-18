using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// カテゴリデータアクセス用リポジトリインターフェース
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// 全カテゴリを取得（名前順）
    /// </summary>
    /// <returns>カテゴリのコレクション</returns>
    Task<IEnumerable<Category>> GetAllAsync();
    
    /// <summary>
    /// IDでカテゴリを取得
    /// </summary>
    /// <param name="id">カテゴリID</param>
    /// <returns>カテゴリ、見つからない場合はnull</returns>
    Task<Category?> GetByIdAsync(int id);
    
    /// <summary>
    /// カテゴリを追加
    /// </summary>
    /// <param name="category">追加するカテゴリ</param>
    /// <returns>追加されたカテゴリ</returns>
    Task<Category> AddAsync(Category category);
    
    /// <summary>
    /// カテゴリを更新
    /// </summary>
    /// <param name="category">更新するカテゴリ</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> UpdateAsync(Category category);
    
    /// <summary>
    /// カテゴリを削除
    /// </summary>
    /// <param name="id">削除するカテゴリのID</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> DeleteAsync(int id);
}
