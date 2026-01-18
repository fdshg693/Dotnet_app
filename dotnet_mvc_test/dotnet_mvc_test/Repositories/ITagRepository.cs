using dotnet_mvc_test.Models.Entities;

namespace dotnet_mvc_test.Repositories;

/// <summary>
/// タグデータアクセス用リポジトリインターフェース
/// </summary>
public interface ITagRepository
{
    /// <summary>
    /// 全タグを取得（名前順）
    /// </summary>
    /// <returns>タグのコレクション</returns>
    Task<IEnumerable<Tag>> GetAllAsync();
    
    /// <summary>
    /// IDでタグを取得
    /// </summary>
    /// <param name="id">タグID</param>
    /// <returns>タグ、見つからない場合はnull</returns>
    Task<Tag?> GetByIdAsync(int id);
    
    /// <summary>
    /// 複数のIDでタグを取得
    /// </summary>
    /// <param name="ids">タグIDのコレクション</param>
    /// <returns>タグのコレクション</returns>
    Task<IEnumerable<Tag>> GetByIdsAsync(IEnumerable<int> ids);
    
    /// <summary>
    /// タグを追加
    /// </summary>
    /// <param name="tag">追加するタグ</param>
    /// <returns>追加されたタグ</returns>
    Task<Tag> AddAsync(Tag tag);
    
    /// <summary>
    /// タグを更新
    /// </summary>
    /// <param name="tag">更新するタグ</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> UpdateAsync(Tag tag);
    
    /// <summary>
    /// タグを削除
    /// </summary>
    /// <param name="id">削除するタグのID</param>
    /// <returns>成功時true、失敗時false</returns>
    Task<bool> DeleteAsync(int id);
}
