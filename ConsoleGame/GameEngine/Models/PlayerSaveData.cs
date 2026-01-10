using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameEngine.Models
{
    /// <summary>
    /// MongoDBに保存するプレイヤーデータのモデル
    /// </summary>
    public class PlayerSaveData
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("playerName")]
        public required string PlayerName { get; set; }

        [BsonElement("currentHP")]
        public int CurrentHP { get; set; }

        [BsonElement("maxHP")]
        public int MaxHP { get; set; }

        [BsonElement("baseAP")]
        public int BaseAP { get; set; }

        [BsonElement("baseDP")]
        public int BaseDP { get; set; }

        [BsonElement("totalGold")]
        public int TotalGold { get; set; }

        [BsonElement("totalPotions")]
        public int TotalPotions { get; set; }

        [BsonElement("level")]
        public int Level { get; set; }

        [BsonElement("totalExperience")]
        public int TotalExperience { get; set; }

        [BsonElement("equippedWeapon")]
        public WeaponData EquippedWeapon { get; set; } = new WeaponData();

        [BsonElement("attackStrategy")]
        public string AttackStrategy { get; set; } = "Default";

        [BsonElement("savedAt")]
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("saveSlotName")]
        public string SaveSlotName { get; set; } = "auto_save";
    }

    /// <summary>
    /// 武器データのサブモデル
    /// </summary>
    public class WeaponData
    {
        [BsonElement("name")]
        public string Name { get; set; } = "Default";

        [BsonElement("hp")]
        public int HP { get; set; }

        [BsonElement("ap")]
        public int AP { get; set; }

        [BsonElement("dp")]
        public int DP { get; set; }
    }
}
