// Arcane Afterglow — portable Unity state machine for discovery, reciprocal signals, independent chats, affection, XP, and skills.

using System;
using System.Collections.Generic;
using System.Linq;
using SmartphoneNovella.Data;

namespace SmartphoneNovella.Runtime
{
    public enum ChatStatus { Idle, Waiting, Typing }
    public enum NotificationKind { Message, Match, Request, Level }

    [Serializable]
    public sealed class PlayerState
    {
        public string name;
        public string email;
        public string password;
        public string about;
        public string portraitId = "alex";
        public int level = 1;
        public int xp;
        public int skillPoints;
        public int likes;
        public int matches;
        public int messagesSent;
        public int events;
        public Dictionary<SkillId, int> skills = Enum.GetValues(typeof(SkillId)).Cast<SkillId>().ToDictionary(skill => skill, _ => 1);
    }

    [Serializable]
    public sealed class MessengerSettings
    {
        public bool music = true;
        public bool sfx = true;
        public bool notifications = true;
    }

    [Serializable]
    public sealed class ChatMessage
    {
        public string id;
        public string sender;
        public string body;
        public MessageKind kind;
        public DateTime createdAt;
    }

    [Serializable]
    public sealed class ChatState
    {
        public string id;
        public string girlId;
        public List<ChatMessage> messages = new List<ChatMessage>();
        public int unreadCount;
        public ChatStatus status;
        public int affection = 12;
        public DateTime lastActivity;
        public float typingAt = -1f;
        public float responseAt = -1f;
    }

    public readonly struct NotificationItem
    {
        public readonly string title;
        public readonly string detail;
        public readonly NotificationKind kind;
        public NotificationItem(string title, string detail, NotificationKind kind) { this.title = title; this.detail = detail; this.kind = kind; }
    }

    public sealed class MessengerSession
    {
        private readonly MessengerContentLibrary content;
        private readonly List<string> liked = new List<string>();
        private readonly List<string> disliked = new List<string>();
        private readonly List<string> matched = new List<string>();
        private readonly Dictionary<string, float> pendingSignals = new Dictionary<string, float>();
        private readonly List<NotificationItem> notifications = new List<NotificationItem>();

        public PlayerState Player { get; private set; } = new PlayerState();
        public MessengerSettings Settings { get; private set; } = new MessengerSettings();
        public List<ChatState> Chats { get; } = new List<ChatState>();
        public bool ProfileCreated { get; private set; }
        public string IncomingLikeId { get; private set; }
        public string MatchedGirlId { get; private set; }
        public IReadOnlyList<NotificationItem> Notifications => notifications;
        public IEnumerable<CharacterDefinition> DiscoverCandidates => content.characters.Where(character => !liked.Contains(character.id) && !disliked.Contains(character.id) && !matched.Contains(character.id));

        public MessengerSession(MessengerContentLibrary content)
        {
            this.content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public void CompleteOnboarding(string name, string email, string password, string about, string portraitId)
        {
            Player.name = name;
            Player.email = email;
            Player.password = password;
            Player.about = about;
            Player.portraitId = portraitId;
            ProfileCreated = true;
        }

        public void StartDemo()
        {
            CompleteOnboarding("Alex", "alex@veyla.demo", "moon-arc-47", "Люблю путешествовать и искать что-то новое.", "alex");
        }

        public void Like(string characterId, float now)
        {
            CharacterDefinition character = content.FindCharacter(characterId);
            if (character == null || liked.Contains(characterId) || disliked.Contains(characterId) || matched.Contains(characterId)) return;
            liked.Add(characterId);
            Player.likes++;
            GrantXp(2);
            if (character.reciprocal) pendingSignals[characterId] = now + 1.4f;
        }

        public void Dislike(string characterId)
        {
            if (!disliked.Contains(characterId)) disliked.Add(characterId);
        }

        public void AcceptIncoming()
        {
            if (string.IsNullOrWhiteSpace(IncomingLikeId)) return;
            CharacterDefinition character = content.FindCharacter(IncomingLikeId);
            string girlId = IncomingLikeId;
            if (!matched.Contains(girlId)) matched.Add(girlId);
            if (Chats.All(chat => chat.girlId != girlId))
            {
                Chats.Insert(0, new ChatState
                {
                    id = $"chat-{girlId}",
                    girlId = girlId,
                    unreadCount = 1,
                    lastActivity = DateTime.UtcNow,
                    messages = new List<ChatMessage>
                    {
                        new ChatMessage { id = $"opener-{girlId}", sender = "npc", body = character.opener, kind = MessageKind.Text, createdAt = DateTime.UtcNow }
                    }
                });
            }
            Player.matches++;
            GrantXp(10);
            MatchedGirlId = girlId;
            IncomingLikeId = null;
            Notify("Взаимная симпатия", $"Теперь вы и {character.displayName} можете общаться.", NotificationKind.Match);
        }

        public void DeclineIncoming()
        {
            if (!string.IsNullOrWhiteSpace(IncomingLikeId)) Dislike(IncomingLikeId);
            IncomingLikeId = null;
        }

        public void OpenChat(string chatId)
        {
            ChatState chat = Chats.FirstOrDefault(item => item.id == chatId);
            if (chat != null) chat.unreadCount = 0;
        }

        public bool TrySendReply(string chatId, ReplyDefinition reply, float now)
        {
            ChatState chat = Chats.FirstOrDefault(item => item.id == chatId);
            if (chat == null || chat.status != ChatStatus.Idle || !CanUse(reply)) return false;
            chat.messages.Add(new ChatMessage { id = $"you-{Guid.NewGuid():N}", sender = "player", body = reply.body, kind = MessageKind.Text, createdAt = DateTime.UtcNow });
            chat.affection = Math.Max(0, Math.Min(100, chat.affection + reply.affection));
            chat.status = ChatStatus.Waiting;
            chat.typingAt = now + 1.2f;
            chat.responseAt = now + 3.5f;
            chat.lastActivity = DateTime.UtcNow;
            Player.messagesSent++;
            GrantXp(reply.xp + 3);
            return true;
        }

        public bool UpgradeSkill(SkillId skill)
        {
            if (Player.skillPoints < 1 || Player.skills[skill] >= 3) return false;
            Player.skillPoints--;
            Player.skills[skill]++;
            return true;
        }

        public void SetSettings(MessengerSettings settings) => Settings = settings ?? new MessengerSettings();

        public void Tick(float now, string activeChatId)
        {
            KeyValuePair<string, float> dueSignal = pendingSignals.FirstOrDefault(pair => pair.Value <= now);
            if (!string.IsNullOrWhiteSpace(dueSignal.Key) && string.IsNullOrWhiteSpace(IncomingLikeId))
            {
                string girlId = dueSignal.Key;
                pendingSignals.Remove(girlId);
                IncomingLikeId = girlId;
                Player.events++;
                GrantXp(10);
                CharacterDefinition girl = content.FindCharacter(girlId);
                Notify("Новый запрос", $"{girl.displayName} поставила вам знак.", NotificationKind.Request);
            }

            foreach (ChatState chat in Chats)
            {
                if (chat.status == ChatStatus.Waiting && chat.typingAt <= now) chat.status = ChatStatus.Typing;
                if (chat.status != ChatStatus.Typing || chat.responseAt > now) continue;
                CharacterDefinition girl = content.FindCharacter(chat.girlId);
                chat.messages.Add(new ChatMessage { id = $"npc-{Guid.NewGuid():N}", sender = "npc", body = girl.response, kind = girl.responseKind, createdAt = DateTime.UtcNow });
                chat.status = ChatStatus.Idle;
                chat.lastActivity = DateTime.UtcNow;
                if (chat.id != activeChatId) chat.unreadCount++;
                if (chat.id != activeChatId) Notify(girl.displayName, "Новое письмо ждёт в Письмах.", NotificationKind.Message);
            }
        }

        public void DismissNotification(int index)
        {
            if (index >= 0 && index < notifications.Count) notifications.RemoveAt(index);
        }

        public void Reset()
        {
            liked.Clear(); disliked.Clear(); matched.Clear(); pendingSignals.Clear(); notifications.Clear(); Chats.Clear();
            Player = new PlayerState(); Settings = new MessengerSettings(); ProfileCreated = false; IncomingLikeId = null; MatchedGirlId = null;
        }

        private bool CanUse(ReplyDefinition reply) => !reply.requiresSkill || Player.skills[reply.requiredSkill] >= reply.requiredLevel;
        private void GrantXp(int amount)
        {
            int oldLevel = Player.level;
            Player.xp += amount;
            Player.level = Math.Min(4, Player.xp / 30 + 1);
            if (Player.level <= oldLevel) return;
            Player.skillPoints += Player.level - oldLevel;
            Notify("Новый уровень", $"Получено очко навыка. Уровень {Player.level}.", NotificationKind.Level);
        }
        private void Notify(string title, string detail, NotificationKind kind)
        {
            if (!Settings.notifications) return;
            notifications.Insert(0, new NotificationItem(title, detail, kind));
            if (notifications.Count > 3) notifications.RemoveAt(notifications.Count - 1);
        }
    }
}
