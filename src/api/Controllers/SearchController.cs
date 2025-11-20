using api.Payloads;
using api.Services;
using Microsoft.AspNetCore.Mvc;

namespace api.Controllers
{
    [ApiController]
    [Route("search")]
    public sealed class SearchController : ControllerBase
    {
        private readonly SearchService _searchService;
        
        public SearchController(SearchService searchService)
        {
            _searchService = searchService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<CustomerPayload>> Search([FromQuery] string q)
        {
            return Ok(string.IsNullOrWhiteSpace(q) ? [] : _searchService.Search(q));
        }
    }
}