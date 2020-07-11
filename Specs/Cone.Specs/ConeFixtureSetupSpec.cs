using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CheckThat;
using Cone.Core;
using Moq;


namespace Cone
{
	class ConeFixtureMethods : IConeFixtureMethodSink
	{
		public List<Invokable> BeforeAll = new List<Invokable>();
		public List<Invokable> BeforeEach = new List<Invokable>();
		public List<Invokable> AfterEach = new List<Invokable>();
		public List<Invokable> AfterAll = new List<Invokable>();
		public List<Invokable> AfterEachWithResult = new List<Invokable>();

		void IConeFixtureMethodSink.Unintresting(Invokable method) { }
		void IConeFixtureMethodSink.BeforeAll(Invokable method) { BeforeAll.Add(method); }
		void IConeFixtureMethodSink.BeforeEach(Invokable method) { BeforeEach.Add(method); }
		void IConeFixtureMethodSink.AfterEach(Invokable method) { AfterEach.Add(method); }
		void IConeFixtureMethodSink.AfterEachWithResult(Invokable method) { AfterEachWithResult.Add(method); }
		void IConeFixtureMethodSink.AfterAll(Invokable method) { AfterAll.Add(method); }
	}

	[Describe(typeof(ConeFixtureSetup))]
	public class ConeFixtureSetupSpec
	{
		class DerivedFixture : SampleFixture 
		{
			[BeforeAll]
			public void DerivedBeforeAll() {}

			[BeforeEach]
			public void DerivedBeforeEach() {}

			[AfterEach]
			public void DerivedAfterEach() {}          

			[AfterEach]
			public void DerivedAfterEachWithResult(ITestResult result) {}

			[AfterAll]
			public void DerivedAfterAll() {}
		}

		[Context("base methods before derived ones")]
		public class DerivedFirst
		{
			ConeFixtureMethods FixtureMethods;
	
			[BeforeAll]
			public void GetFixtureMethods() {
				FixtureMethods = new ConeFixtureMethods();
				var testSink = new Mock<IConeTestMethodSink>().Object;
				var setup = new ConeFixtureSetup(new ConeMethodClassifier(FixtureMethods, testSink));
				setup.CollectFixtureMethods(typeof(DerivedFixture));
			}

			public void base_BeforeAll_before_derived() {
				Check.That(() => 
					FixtureMethods.BeforeAll.IndexOf(Base(x => x.BeforeAll())) <
					FixtureMethods.BeforeAll.IndexOf(Derived(x => x.DerivedBeforeAll())));
			}

			public void base_BeforeEach_before_derived() {
				Check.That(() => 
					FixtureMethods.BeforeEach.IndexOf(Base(x => x.BeforeEach())) <
					FixtureMethods.BeforeEach.IndexOf(Derived(x => x.DerivedBeforeEach())));
			}
		}

		[Context("virtual fixture methods")]
		public class VirtualFixtureMethods
		{
			class BaseFixture
			{
				[BeforeEach]
				public virtual void BeforeEach() { }
			}

			class DerivedFixture : BaseFixture
			{
				public override void BeforeEach() { }
			}

			public void are_classified_only_once() {
				var methods = new ConeFixtureMethods();
				var testSink = new Mock<IConeTestMethodSink>().Object;
				var setup = new ConeFixtureSetup(new ConeMethodClassifier(methods, testSink));
				setup.CollectFixtureMethods(typeof(DerivedFixture));
				Check.That(() => methods.BeforeEach.Count == 1);
			}
		}

		static Invokable Base(Expression<Action<SampleFixture>> x) => new Invokable(((MethodCallExpression)x.Body).Method);
		static Invokable Derived(Expression<Action<DerivedFixture>> x) => new Invokable(((MethodCallExpression)x.Body).Method);
	}
}
