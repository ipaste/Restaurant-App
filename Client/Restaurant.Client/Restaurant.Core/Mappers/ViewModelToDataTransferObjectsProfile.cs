﻿using AutoMapper;
using Restaurant.Common.DataTransferObjects;
using Restaurant.Core.ViewModels;

namespace Restaurant.Core.Mappers
{
	public class ViewModelToDataTransferObjectsProfile : Profile
	{
	    public ViewModelToDataTransferObjectsProfile()
	    {
	        //CreateMap<SignUpViewModel, RegisterDto>()
	        //    .ForMember(x => x.UserName, map => map.MapFrom(x => x.Name))
	        //    .ForMember(x => x.Email, map => map.MapFrom(x => x.Email))
	        //    .ForMember(x => x.Password, map => map.MapFrom(vm => vm.Password))
	        //    .ForMember(x => x.ConfirmPassword, map => map.MapFrom(vm => vm.ConfirmPassword));

	        CreateMap<SignInViewModel, LoginDto>()
	            .ForMember(x => x.Login, map => map.MapFrom(vm => vm.Email))
	            .ForMember(x => x.Password, map => map.MapFrom(vm => vm.Password));
        }
	}
}