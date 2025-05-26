using System.Threading.Tasks;   
using Codout.DynamicLinq;
using Codout.Framework.Application.Interfaces;
using Codout.Framework.Domain;
using Codout.Framework.Dto;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Codout.Framework.Api
{
    public abstract class RestApiEntityBase<TEntity, TDto, TId> : ControllerBase, 
        IRestApi<TDto, TId> 
        where TEntity : Entity<TId>
        where TDto : EntityDto<TId>
    {
        public ICrudAppService<TEntity, TDto, TId> AppService { get; }

        protected RestApiEntityBase(ICrudAppService<TEntity, TDto, TId> appService)
        {
            AppService = appService;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiException))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiException))]
        public virtual async Task<IActionResult> Get(TId id)
        {
            var result = await AppService.GetAsync(id);
            return Ok(result);
        }

        [HttpPost("")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiException))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiException))]
        public virtual async Task<IActionResult> Post([FromBody]TDto value)
        {
            var result = await AppService.SaveAsync(value);
            return Ok(result);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiException))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiException))]
        public virtual async Task<IActionResult> Put(TId id, [FromBody]TDto value)
        {
            value.Id = id;
            var result = await AppService.UpdateAsync(value);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiException))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiException))]
        public virtual async Task<IActionResult> Delete(TId id)
        {
            await AppService.DeleteAsync(id);
            return Ok();
        }

        [HttpPost("get-all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ApiException))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ApiException))]
        public virtual async Task<IActionResult> GetAll([FromBody]DataSourceRequest value)
        {
            var result = await AppService.GetAllAsync(value);
            return Ok(result);
        }
    }
}
