using Game.Content;
using R3;
using System;
using System.Collections.Generic;

namespace Game.Core
{
	// Dialogue playback: one service owns dialogue state + rules. The panel leaf drives
	// pacing (interact press or DisplayDuration via Update); the service only goes
	// line-to-line on Advance(). A missing (cardId, actorId) entry is a silent no-op —
	// the NPC has nothing to say for that card.
	public sealed class DialogueService : IDisposable
	{
		private readonly DialogueSettings _settings;
		private readonly CardInventory _cards;
		private readonly HashSet<string> _conducted = new(); // "cardId_actorId"

		private string _currentKey;
		private DialogueEntry _current;
		private int _lineIndex;

		public ReactiveProperty<bool> IsPlaying { get; } = new(false);
		public ReactiveProperty<string> SpeakerName { get; } = new(string.Empty);
		public ReactiveProperty<DialogueLine> CurrentLine { get; } = new(default);

		/// <summary>Fires with "cardId_actorId" whenever a dialogue finishes (used by chapter gates).</summary>
		public Subject<string> ConversationConducted { get; } = new();

		/// <summary>Snapshot of every conducted conversation key (persisted by ProgressStore).</summary>
		public IReadOnlyCollection<string> Conducted => _conducted;

		public DialogueService(CardInventory cards, DialogueSettings settings)
		{
			_cards = cards;
			_settings = settings;
		}

		public bool HadConversation(string cardId, string actorId) => _conducted.Contains(Key(cardId, actorId));

		// Restore from save: adds without firing (silent, like CardInventory.GrantIds).
		public void RestoreConducted(IEnumerable<string> keys)
		{
			foreach (string key in keys) _conducted.Add(key);
		}

		/// <summary>Start playback. Chapter-aware: prefers "cardId_actorId_chapter", falls back to "cardId_actorId".</summary>
		public void Play(string cardId, string actorId, int chapter = 0)
		{
			if (IsPlaying.Value) return;

			DialogueEntry entry = Resolve(cardId, actorId, chapter);
			if (entry == null || entry.Lines == null || entry.Lines.Count == 0) return;

			_currentKey = Key(cardId, actorId);
			_current = entry;
			_lineIndex = 0;
			SpeakerName.Value = actorId;
			IsPlaying.Value = true;
			CurrentLine.Value = entry.Lines[0];
		}

		public void Advance()
		{
			if (!IsPlaying.Value) return;

			_lineIndex++;
			if (_lineIndex < _current.Lines.Count)
			{
				CurrentLine.Value = _current.Lines[_lineIndex];
				return;
			}

			Finish();
		}

		private DialogueEntry Resolve(string cardId, string actorId, int chapter)
		{
			if (chapter > 0 && _settings.Entries.TryGetValue($"{cardId}_{actorId}_{chapter}", out DialogueEntry chapterEntry))
				return chapterEntry;
			return _settings.Entries.TryGetValue(Key(cardId, actorId), out DialogueEntry entry) ? entry : null;
		}

		private void Finish()
		{
			_conducted.Add(_currentKey);
			ConversationConducted.OnNext(_currentKey);

			if (!string.IsNullOrEmpty(_current.RewardCardId))
				_cards.GrantId(_current.RewardCardId);

			IsPlaying.Value = false;
			SpeakerName.Value = string.Empty;
			CurrentLine.Value = default;
			_current = null;
		}

		private static string Key(string cardId, string actorId) => $"{cardId}_{actorId}";

		public void Dispose()
		{
			IsPlaying.Dispose();
			SpeakerName.Dispose();
			CurrentLine.Dispose();
			ConversationConducted.Dispose();
		}
	}
}