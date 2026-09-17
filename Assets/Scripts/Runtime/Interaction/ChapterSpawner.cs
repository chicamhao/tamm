using Game.Content;
using Game.Core;
using R3;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Game.Interaction
{
	// Applies the current chapter's actor state: visibility + spawn position, per
	// ChapterSettings entries ("actorId_chapter"). Scene bindings map actor id -> actor root.
	// ponytail: GameObject.Find by spawn id — fine at scene scale; swap for a spawn
	// registry list on this component the day scenes grow.
	public sealed class ChapterSpawner : MonoBehaviour
	{
		[Serializable]
		private sealed class ActorBinding
		{
			public string actorId;
			public Transform actor;
		}

		[SerializeField] private ChapterSettings _settings;
		[SerializeField] private ActorBinding[] _bindings;

		private IDisposable _onChapter;

		private void Start()
		{
			Assert.IsNotNull(_settings, "ChapterSpawner requires ChapterSettings assigned");

			ChapterState chapter = Services.Chapter;

			Apply(chapter.CurrentChapter.Value);
			_onChapter = chapter.CurrentChapter.Subscribe(Apply);
		}

		private void Apply(int chapterNumber)
		{
			foreach (ActorBinding binding in _bindings)
			{
				if (binding.actor == null || string.IsNullOrEmpty(binding.actorId)) continue;

				if (!_settings.Entries.TryGetValue($"{binding.actorId}_{chapterNumber}", out ChapterEntry entry))
				{
					binding.actor.gameObject.SetActive(true); // no override: visible as authored
					continue;
				}

				binding.actor.gameObject.SetActive(entry.IsVisible);

				if (!string.IsNullOrEmpty(entry.SpawnPointID))
				{
					GameObject spawn = GameObject.Find(entry.SpawnPointID);
					if (spawn != null) binding.actor.position = spawn.transform.position;
				}
			}
		}

		private void OnDestroy() => _onChapter?.Dispose();
	}
}