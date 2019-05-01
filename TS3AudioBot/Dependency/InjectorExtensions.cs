// TS3AudioBot - An advanced Musicbot for Teamspeak 3
// Copyright (C) 2017  TS3AudioBot contributors
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the Open Software License v. 3.0
//
// You should have received a copy of the Open Software License along with this
// program. If not, see <https://opensource.org/licenses/OSL-3.0>.

namespace TS3AudioBot.Dependency
{
	using System;

	public static class InjectorExtensions
	{
		public static bool TryGet<T>(this IInjector injector, out T obj)
		{
			var ok = injector.TryGet(typeof(T), out var oobj);
			obj = ok ? (T)oobj : (default);
			return ok;
		}
		public static bool TryGet(this IInjector injector, Type t, out object obj)
		{
			obj = injector.GetModule(t);
			return obj != null;
		}
	}
}
