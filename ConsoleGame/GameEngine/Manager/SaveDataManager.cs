using MongoDB.Driver;
using GameEngine.Models;
using GameEngine.Interfaces;

namespace GameEngine.Manager
{
    /// <summary>
    /// MongoDBを使用してプレイヤーデータの保存・読み込みを管理するクラス
    /// </summary>
    public class SaveDataManager
    {
        private readonly IMongoCollection<PlayerSaveData> _collection;
        private readonly string _connectionString;
        private readonly string _databaseName;

        public SaveDataManager(string connectionString, string databaseName, string collectionName)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            if (string.IsNullOrWhiteSpace(databaseName))
                throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentException("Collection name cannot be null or empty", nameof(collectionName));

            _connectionString = connectionString;
            _databaseName = databaseName;

            try
            {
                var client = new MongoClient(_connectionString);
                var database = client.GetDatabase(_databaseName);
                _collection = database.GetCollection<PlayerSaveData>(collectionName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MongoDB接続エラー: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// プレイヤーデータをMongoDBに保存する
        /// </summary>
        public async Task<bool> SavePlayerDataAsync(IPlayer player, string saveSlotName = "auto_save")
        {
            try
            {
                var saveData = player.GetSaveData(saveSlotName);

                // 同じスロット名のデータを上書き保存
                var filter = Builders<PlayerSaveData>.Filter.And(
                    Builders<PlayerSaveData>.Filter.Eq(x => x.PlayerName, saveData.PlayerName),
                    Builders<PlayerSaveData>.Filter.Eq(x => x.SaveSlotName, saveSlotName)
                );

                var options = new ReplaceOptions { IsUpsert = true };
                await _collection.ReplaceOneAsync(filter, saveData, options);

                Console.WriteLine($"\n✓ セーブデータを保存しました（スロット: {saveSlotName}）");
                Console.WriteLine($"  保存時刻: {saveData.SavedAt.ToLocalTime():yyyy/MM/dd HH:mm:ss}");
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ セーブデータの保存に失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// プレイヤーデータをMongoDBから読み込む
        /// </summary>
        public async Task<PlayerSaveData?> LoadPlayerDataAsync(string playerName, string saveSlotName = "auto_save")
        {
            try
            {
                var filter = Builders<PlayerSaveData>.Filter.And(
                    Builders<PlayerSaveData>.Filter.Eq(x => x.PlayerName, playerName),
                    Builders<PlayerSaveData>.Filter.Eq(x => x.SaveSlotName, saveSlotName)
                );

                var saveData = await _collection.Find(filter).FirstOrDefaultAsync();

                if (saveData != null)
                {
                    Console.WriteLine($"\n✓ セーブデータを読み込みました");
                    Console.WriteLine($"  保存時刻: {saveData.SavedAt.ToLocalTime():yyyy/MM/dd HH:mm:ss}");
                }
                else
                {
                    Console.WriteLine($"\n✗ セーブデータが見つかりませんでした（プレイヤー: {playerName}, スロット: {saveSlotName}）");
                }

                return saveData;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n✗ セーブデータの読み込みに失敗しました: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 指定プレイヤーのセーブスロット一覧を取得
        /// </summary>
        public async Task<List<PlayerSaveData>> GetSaveListAsync(string playerName)
        {
            try
            {
                var filter = Builders<PlayerSaveData>.Filter.Eq(x => x.PlayerName, playerName);
                var saves = await _collection.Find(filter).ToListAsync();
                return saves;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セーブリストの取得に失敗しました: {ex.Message}");
                return new List<PlayerSaveData>();
            }
        }

        /// <summary>
        /// セーブデータを削除
        /// </summary>
        public async Task<bool> DeleteSaveDataAsync(string playerName, string saveSlotName)
        {
            try
            {
                var filter = Builders<PlayerSaveData>.Filter.And(
                    Builders<PlayerSaveData>.Filter.Eq(x => x.PlayerName, playerName),
                    Builders<PlayerSaveData>.Filter.Eq(x => x.SaveSlotName, saveSlotName)
                );

                var result = await _collection.DeleteOneAsync(filter);
                
                if (result.DeletedCount > 0)
                {
                    Console.WriteLine($"✓ セーブデータを削除しました（スロット: {saveSlotName}）");
                    return true;
                }
                else
                {
                    Console.WriteLine($"✗ 削除するセーブデータが見つかりませんでした");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"セーブデータの削除に失敗しました: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// MongoDB接続をテストする
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                var client = new MongoClient(_connectionString);
                await client.ListDatabaseNamesAsync();
                Console.WriteLine("✓ MongoDB接続成功");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ MongoDB接続失敗: {ex.Message}");
                Console.WriteLine("  Docker上でMongoDBが起動しているか確認してください");
                Console.WriteLine($"  接続文字列: {_connectionString}");
                return false;
            }
        }
    }
}
