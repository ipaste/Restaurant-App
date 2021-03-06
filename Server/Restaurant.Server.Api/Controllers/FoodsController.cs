using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Restaurant.Common.DataTransferObjects;
using Restaurant.Server.Abstraction.Facades;
using Restaurant.Server.Abstraction.Providers;
using Restaurant.Server.Abstraction.Repositories;
using Restaurant.Server.Models;

namespace Restaurant.Server.Api.Controllers
{
	[Produces("application/json")]
	[Route("api/[controller]")]
	public class FoodsController : Controller
	{
		private readonly IFileUploadProvider _fileUploadProvider;
		private readonly IMapperFacade _mapperFacade;
		private readonly IRepository<Food> _repository;

		public FoodsController(
			IMapperFacade mapperFacade,
			IRepository<Food> repository,
			IFileUploadProvider fileUploadProvider)
		{
			_mapperFacade = mapperFacade;
			_repository = repository;
			_fileUploadProvider = fileUploadProvider;
		}

		[HttpGet]
		public IEnumerable<FoodDto> Get()
		{
			var entities = _repository.GetAll().ToList();

			return _mapperFacade.Map<IEnumerable<FoodDto>>(entities);
		}

		[HttpGet("{id}")]
		public FoodDto Get(Guid id)
		{
			return _mapperFacade.Map<FoodDto>(_repository.Get(id));
		}

		[HttpPost]
		public async Task<IActionResult> Post([FromBody] FoodDto foodDto)
		{
			try
			{
				var food = _mapperFacade.Map<Food>(foodDto);
				food.Picture = _fileUploadProvider.GetUploadedFileByUniqId(food.Id.ToString());
				_repository.Create(food);
				var result = await _repository.Commit();
				if (result)
				{
					_fileUploadProvider.Reset();
					return Ok();
				}
				_fileUploadProvider.RemoveUploadedFileByUniqId(foodDto.Id.ToString());
				return BadRequest();
			}
			catch (Exception)
			{
				_fileUploadProvider.RemoveUploadedFileByUniqId(foodDto.Id.ToString());
				return BadRequest();
			}
		}

		[HttpPost]
		[Route("UploadFile")]
		public async Task Post([Bind] IFormFile file, [Bind] string foodId)
		{
			await _fileUploadProvider.Upload(file, foodId);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Put(Guid id, [FromBody] FoodDto foodDto)
		{
			try
			{
				if (id != foodDto.Id)
					return BadRequest();
				var food = _mapperFacade.Map<Food>(foodDto);
				if (_fileUploadProvider.HasFile(id.ToString()))
				{
					_fileUploadProvider.Remove(food.Picture);
					food.Picture = _fileUploadProvider.GetUploadedFileByUniqId(id.ToString());
				}

				_repository.Update(id, food);
				return await _repository.Commit() ? Ok() : (IActionResult) BadRequest();
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(Guid id)
		{
			try
			{
				var food = _repository.Get(id);
				_fileUploadProvider.Remove(food.Picture);
				_repository.Delete(food);
				return await _repository.Commit() ? Ok() : (IActionResult) BadRequest();
			}
			catch (Exception)
			{
				return BadRequest();
			}
		}
	}
}