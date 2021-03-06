﻿using System.Diagnostics.CodeAnalysis;
using AutoMapper;

namespace Restaurant.Server.Core.Mappers
{
	[ExcludeFromCodeCoverage]
	public static class AutoMapperConfiguration
	{
		public static void Configure()
		{
			Mapper.Initialize(x => { x.AddProfile<RestaurantModelsToDtoProfile>(); });
		}
	}
}