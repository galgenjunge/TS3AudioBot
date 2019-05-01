// TS3AudioBot - An advanced Musicbot for Teamspeak 3
// Copyright (C) 2017  TS3AudioBot contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.

namespace TS3AudioBot.CommandSystem
{
	using Algorithm;
	using Dependency;
	using System;

	public class ExecutionInformation : IInjector
	{
		public IInjector ParentInjector { get; set; }
		private readonly IInjector dynamicObjects;

		public ExecutionInformation() : this(NullInjector.Instance) { }

		public ExecutionInformation(IInjector parent)
		{
			ParentInjector = parent ?? throw new ArgumentNullException(nameof(parent));
			dynamicObjects = new BasicInjector();
			AddModule(this);
		}

		public object GetModule(Type type)
		{
			var obj = dynamicObjects.GetModule(type);
			if (obj != null) return obj;
			obj = ParentInjector.GetModule(type);
			return obj;
		}

		public void AddModule(object obj) => dynamicObjects.AddModule(obj);
	}

	public static class CommandSystemExtensions
	{
		public static IFilterAlgorithm GetFilter(this IInjector injector)
		{
			if (injector.TryGet<IFilterAlgorithm>(out var filter))
				return filter;
			return Filter.DefaultFilter;
		}
	}
}
