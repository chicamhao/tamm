using Game.Content;
using R3;
using System;
using System.Collections.Generic;

namespace Game.Core
{
	// Chapter gating: watches player progress (card grants + conducted conversations)
	// and auto-advances the chapter when the next gate's conditions all hold.
	// Gates are designer data (chapters.yaml -> ChapterSettings.Gates).
	public sealed class ChapterState : IDisposable
	{
		private readonly ChapterSettings _settings;
		private readonly CardInventory _cards;
		private readonly DialogueService _dialogue;
		private readonly List<IDisposable> _subs = new();

		public ReactiveProperty<int> CurrentChapter { get; } = new(1);

		public ChapterState(CardInventory cards, DialogueService dialogue, ChapterSettings settings)
		{
			_cards = cards;
			_dialogue = dialogue;
			_settings = settings;

			_subs.Add(cards.Granted.Subscribe(_ => Evaluate()));
			_subs.Add(dialogue.ConversationConducted.Subscribe(_ => Evaluate()));
		}

		public void Restore(int chapter)
		{
			if (chapter >= 1) CurrentChapter.Value = chapter;
		}

		// Unsafe steps for debug/test flows: bypasses gate evaluation, floors at chapter 1.
		public void ForceAdvance() => CurrentChapter.Value = Math.Max(1, CurrentChapter.Value + 1);

		private void Evaluate()
		{
			int next = CurrentChapter.Value + 1;
			if (_settings == null || _settings.Gates == null) return;

			ChapterGate gate = _settings.Gates.Find(g => g.Chapter == next);
			if (gate == null || gate.Conditions == null) return;

			foreach (ProgressCondition condition in gate.Conditions)
			{
				if (!IsMet(condition)) return;
			}

			CurrentChapter.Value = next;
		}

		private bool IsMet(ProgressCondition condition)
		{
			switch (condition.Type)
			{
				case ProgressConditionType.OwnsCard:
					return _cards.Owns(condition.CardId);
				case ProgressConditionType.HadConversation:
					return _dialogue.HadConversation(condition.CardId, condition.ActorId);
				default:
					return false;
			}
		}

		public void Dispose()
		{
			foreach (IDisposable sub in _subs) sub?.Dispose();
			CurrentChapter.Dispose();
		}
	}
}