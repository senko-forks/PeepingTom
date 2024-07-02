using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;

namespace PeepingTom.Ipc {
    [Serializable]
    public class Targeter {
        [JsonConverter(typeof(SeStringConverter))]
        public SeString Name { get; }

        public uint HomeWorldId { get; }
        public ulong GameObjectId { get; }
        public uint EntityId { get; }
        public DateTime When { get; }

        public Targeter(IPlayerCharacter character) {
            this.Name = character.Name;
            this.HomeWorldId = character.HomeWorld.Id;
            this.EntityId = character.EntityId;
            this.GameObjectId = character.GameObjectId;
            this.When = DateTime.UtcNow;
        }

        [JsonConstructor]
        public Targeter(SeString name, uint homeWorldId, uint entityId, ulong gameObjectId, DateTime when) {
            this.Name = name;
            this.HomeWorldId = homeWorldId;
            this.EntityId = entityId;
            this.GameObjectId = gameObjectId;
            this.When = when;
        }

        public IPlayerCharacter? GetPlayerCharacter(IObjectTable objectTable) {
            return objectTable.FirstOrDefault(actor => actor.GameObjectId == this.GameObjectId && actor is IPlayerCharacter) as IPlayerCharacter;
        }
    }
}
