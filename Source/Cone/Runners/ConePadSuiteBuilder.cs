﻿using System;
using System.Collections.Generic;
using Cone.Core;

namespace Cone.Runners
{
	public class ConePadSuiteBuilder : ConeSuiteBuilder<ConePadSuite>
	{
		readonly FixtureProvider objectProvider;

		public ConePadSuiteBuilder(ITestNamer testNamer, FixtureProvider objectProvider) : base(testNamer) {
			this.objectProvider = objectProvider;
		}
		
		protected override ConePadSuite NewSuite(Type type, IFixtureDescription description) {
			return new ConePadSuite(MakeFixture(type, description.Categories)) { 
				Name = description.SuiteName + "." + description.TestName
			};
		}

		protected ConeFixture MakeFixture(Type type, IEnumerable<string> categories) {
			return new ConeFixture(type, categories, objectProvider);
		}

		protected override void AddSubSuite(ConePadSuite suite, ConePadSuite subsuite) {
			suite.AddSubSuite(subsuite);
		}
	}
}
