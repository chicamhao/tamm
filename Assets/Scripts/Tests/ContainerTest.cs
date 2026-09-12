using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Game.Core
{
	// Container is pure C# with no engine deps, so it runs as an Edit Mode test.
	public sealed class ContainerTest
	{
		private sealed class DisposableService : IDisposable
		{
			public List<int> Report;
			public DisposableService(List<int> report = null) => Report = report;
			public void Dispose()
			{
				if (Report != null) Report.Add(Report.Count);
			}
		}

		private sealed class UsesDep
		{
			public DisposableService Dep;
			public UsesDep(DisposableService dep) => Dep = dep;
		}

		[Test] public void ProvideInstance_GrabReturnsSameInstance()
		{
			Container c = new();
			DisposableService service = new();
			c.Provide(service);

			Assert.That(c.Grab<DisposableService>(), Is.SameAs(service));
		}

		[Test] public void ProvideFactory_RunsAtProvide_WithContainerResolvedDeps()
		{
			Container c = new();
			DisposableService dep = new();
			c.Provide(dep);
			c.Provide(g => new UsesDep(g.Grab<DisposableService>())); // runs NOW

			UsesDep built = c.Grab<UsesDep>();
			Assert.That(built.Dep, Is.SameAs(dep));
			Assert.That(c.Grab<UsesDep>(), Is.SameAs(built), "Grab must be a pure lookup");
		}

		[Test] public void Grab_UnknownType_Throws()
		{
			Container c = new();

			Assert.Throws<InvalidOperationException>(() => c.Grab<DisposableService>());
		}

		[Test] public void Dispose_TearsDownInReverseProvideOrder()
		{
			List<int> order = new();
			Container c = new();
			c.Provide(new DisposableService(order));
			c.Provide(new DisposableService(order));
			c.Provide(new DisposableService(order));

			c.Dispose();

			List<int> expected = new();
			expected.Add(0);
			expected.Add(1);
			expected.Add(2);
			Assert.That(order, Is.EqualTo(expected), "later providers must be disposed first");
		}
	}
}