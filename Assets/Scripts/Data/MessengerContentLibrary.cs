// Arcane Afterglow — editable Unity content library migrated from the original Fantasy Messenger demo data.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmartphoneNovella.Data
{
    public enum SkillId { Flirt, Messaging, Empathy, Confidence, Attention }
    public enum MessageKind { Text, Photo, Voice, Video }
    public enum CharacterPalette { Mulberry, Moss, Astral, Teal }

    [Serializable]
    public sealed class CharacterDefinition
    {
        public string id;
        public string displayName;
        public string age;
        public string species;
        public string world;
        public string title;
        [TextArea(2, 4)] public string bio;
        public List<string> interests = new List<string>();
        public CharacterPalette palette;
        public bool reciprocal;
        public Sprite portrait;
        [TextArea(2, 4)] public string opener;
        [TextArea(2, 4)] public string response;
        public MessageKind responseKind;
        public List<ReplyDefinition> replyOptions = new List<ReplyDefinition>();
    }

    [Serializable]
    public sealed class ReplyDefinition
    {
        public string id;
        [TextArea(2, 4)] public string body;
        public int affection;
        public int xp;
        public bool requiresSkill;
        public SkillId requiredSkill;
        [Range(1, 3)] public int requiredLevel = 1;
    }

    [Serializable]
    public sealed class SkillDefinition
    {
        public SkillId id;
        public string label;
        public string glyph;
        [TextArea(2, 3)] public string description;
    }

    [CreateAssetMenu(fileName = "FantasyMessengerContent", menuName = "Fantasy Messenger/Content Library")]
    public sealed class MessengerContentLibrary : ScriptableObject
    {
        [Header("Discovery order")]
        public List<CharacterDefinition> characters = new List<CharacterDefinition>();

        [Header("Progression")]
        public List<SkillDefinition> skills = new List<SkillDefinition>();

        [Header("Profile ritual")]
        public List<string> emails = new List<string>();
        public List<string> passwords = new List<string>();
        public List<string> playerNames = new List<string>();
        [TextArea(2, 3)] public List<string> bios = new List<string>();

        public CharacterDefinition FindCharacter(string id) => characters.FirstOrDefault(character => character.id == id);
        public SkillDefinition FindSkill(SkillId id) => skills.FirstOrDefault(skill => skill.id == id);
    }
}
