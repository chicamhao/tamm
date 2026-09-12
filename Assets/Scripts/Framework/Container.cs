using System;
using System.Collections.Generic;

namespace Game.Core
{
	// Minimal DI container: register singletons by instance or factory, resolve by type.
	//
	// Usage:
	//   Container c = new();
	//   c.Provide(new Progression(0));                        // instance singleton
	//   c.Provide(g => new GuiModel(g.Grab<Progression>()));  // factory — runs NOW, deps grabbed
	//   Progression p = c.Grab<Progression>();                // pure lookup
	//
	// Construction happens at Provide, in Provide-call order: provide dependencies before the
	// services that use them (this is what makes wiring order explicit). Grab never constructs —
	// it is a typed map lookup. Container.Dispose() disposes registered IDisposables in
	// reverse Provide order.
	public sealed class Container : IDisposable
	{
		private readonly Dictionary<Type, Object> _providers = new();
		private readonly List<IDisposable> _disposables = new();

		public void Provide<T>(T instance)
		{
			_providers[typeof(T)] = instance;
			Track(instance);
		}

		public void Provide<T>(Func<Container, T> factory) => Provide(factory(this));

		public T Grab<T>()
		{
			if (!_providers.ContainsKey(typeof(T)))
				throw new InvalidOperationException($"No provider registered for {typeof(T)}");

			Object value = _providers[typeof(T)];
			return (T)value;
		}

		public void Dispose()
		{
			for (int i = _disposables.Count - 1; i >= 0; --i)
				_disposables[i].Dispose();
			_disposables.Clear();
		}

		private void Track(Object value)
		{
			if (value is IDisposable) _disposables.Add((IDisposable)value);
		}
	}
}